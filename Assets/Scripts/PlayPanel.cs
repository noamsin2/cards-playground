using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Michsky.UI.Reach;
using Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayPanel : MonoBehaviour
{
    [SerializeField] GameObject matchFound;
    public Transform deckContainer;
    public GameObject deckButtonPrefab;
    
    private Button selectedButton;
    public void InitializePanel()
    {
        gameObject.SetActive(true);
        TriggerFadeInPlayPanel();
        LoadPlayerDecks();
    }
    private void LoadPlayerDecks()
    {
        var decks = DecksManager.Instance.decks
        .OrderByDescending(deck => deck.cardsInDeck)  // Highest quantity first
        .ThenBy(deck => deck.deckID)               // Then by DeckID ascending
        .ToList();

        foreach (Transform child in deckContainer)
            Destroy(child.gameObject); // clear previous UI
        selectedButton = null;
        foreach (var deck in decks)
        {
            GameObject deckItem = Instantiate(deckButtonPrefab, deckContainer);
            deckItem.GetComponentInChildren<TMP_Text>().text = deck.deckName;

            deckItem.transform.Find("Cards Quantity").GetComponent<TMP_Text>().text = $"{deck.cardsInDeck}/{deck.maxDeckSize}";
            Button button = deckItem.GetComponent<Button>();
            ColorBlock colors = button.colors;
            
            if (deck.cardsInDeck < deck.maxDeckSize)
            {
                
                Color white = Color.white;
                deckItem.GetComponent<Image>().color = white;
                colors.normalColor = white;
                colors.highlightedColor = white;
                colors.pressedColor = white;
                colors.selectedColor = white;
                colors.disabledColor = Color.gray;

                button.colors = colors;
                //button.interactable = false; // Optional: disable interaction
            }
            button.onClick.AddListener(() => OnSelectDeck(deck, deckItem));
        }

        Debug.Log($"Loaded {decks.Count} decks.");
    }

    private void OnSelectDeck(DeckData deck, GameObject clickedButton)
    {
        DecksManager.Instance.currentDeck = deck;
        Debug.Log($"Selected deck: {deck.deckName} (ID: {deck.deckID})");
        var newSelected = clickedButton.GetComponent<Button>();
        // Reset previous selection
        if (selectedButton != null)
            SetButtonToNormalState(selectedButton);

        // Highlight new selection
        selectedButton = newSelected;
        SetButtonToHighlightedState(newSelected);
    }
    private void SetButtonToHighlightedState(Button btn)
    {
        
        var colors = btn.colors;
        if (colors.normalColor != Color.gray)
        {
            btn.GetComponent<Image>().color = colors.highlightedColor;
        }
    }

    private void SetButtonToNormalState(Button btn)
    {
        var colors = btn.colors;
        Color targetColor;
        if (colors.disabledColor != Color.gray)
        {
            if (ColorUtility.TryParseHtmlString("#45BDFF", out targetColor))
            {
                // Optional: only update if current color is not already the target
                if (colors.normalColor != targetColor)
                {
                    btn.GetComponent<Image>().color = targetColor;
                }
            }
        }
        
    }
    public async void OnPlayButton()
    {
        var currentDeck = DecksManager.Instance.currentDeck;
        if (currentDeck == null || (currentDeck != null && currentDeck.cardsInDeck < currentDeck.maxDeckSize))
            return;
        Debug.Log("OnPlayButton() called");
        matchFound.gameObject.SetActive(true);
        matchFound.transform.Find("Searching Games").gameObject.SetActive(true);
        matchFound.GetComponentInChildren<UIPopup>().PlayIn();
        bool foundMatch = await SupabaseManager.Instance.CheckForMatch(UserManager.Instance.userId);
    }

    public void MatchFound()
    {
        Debug.Log("MatchFound() called");
        //matchFound.transform.Find("Searching Games/Text").GetComponent<TMP_Text>().text = "Match Found";
        matchFound.GetComponentInChildren<UIPopup>().PlayOut();
        Invoke("TriggerFadeOutPlayPanel", 1f);
    }
    private void TriggerFadeInPlayPanel()
    {

        gameObject.GetComponent<Animator>().SetTrigger("FadeIn_Trigger");
    }
    private void TriggerFadeOutPlayPanel()
    {
        
        gameObject.GetComponent<Animator>().SetTrigger("FadeOut_Trigger");
    }
}
