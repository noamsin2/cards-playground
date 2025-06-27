using Models;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Michsky.UI.Reach;
public class DecksPanel : MonoBehaviour
{
    [SerializeField] private Transform contentPanel;
    [SerializeField] private Michsky.UI.Reach.PanelButton deckItemPrefab;
    [SerializeField] private Michsky.UI.Reach.PanelButton addDeckButtonPrefab;
    [SerializeField] private DeleteValidationPanel deleteValidationPanel;
    [SerializeField] private GameObject deckTitle;
    [SerializeField] private GameObject decksText;
    [SerializeField] private Transform cardsContentPanel;
    [SerializeField] private Michsky.UI.Reach.PanelButton cardInDeckPrefab;
    [SerializeField] private Transform cardWithDescriptionPrefab;
    [SerializeField] private TMP_Text errorMessage;
    [SerializeField] private TMP_Text cardsCount;
    [SerializeField] private ButtonManager saveButton;
    private Michsky.UI.Reach.PanelButton selectedDeck = null;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ShowDecks();
        DecksManager.Instance.OnCardAdded += ShowCards;
        DecksManager.Instance.OnCardError += ShowError;
    }
    
    private void ShowDecks()
    {
        
        List<DeckData> decks = DecksManager.Instance.decks;
        Debug.Log($"SHOW DECKS {decks.Count}");
        // Clear the previous content
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        // Add each deck to the UI
        foreach (var deck in decks)
        {
            // Instantiate the prefab for each deck
            Michsky.UI.Reach.PanelButton deckItem = Instantiate(deckItemPrefab, contentPanel);
            // Find the Text or Button inside the prefab (depending on your design)
            deckItem.buttonText = deck.deckName;
            deckItem.onClick.AddListener(() => EditDeck(deckItem, deck));
            Button deleteButton = deckItem.transform.Find("Delete Button").GetComponent<Button>();
            // open delete game validation panel
            deleteButton.onClick.AddListener(() => DeleteDeck(deck));
            deckItem.UpdateUI();
        }
        if (decks.Count < DecksManager.Instance.MAX_DECKS)
        {
            Michsky.UI.Reach.PanelButton addItem = Instantiate(addDeckButtonPrefab, contentPanel);
            addItem.onClick.AddListener(() => CreateNewDeck());
        }
    }
   
   
    private void ShowCards(int? card_id, bool isCalledFromDeck)
    {
        // Assuming that each deck has a list of Card_IDs that reference cards in your database.
        foreach (Transform child in cardsContentPanel)
        {
            Destroy(child.gameObject);
        }
        DeckData deck = DecksManager.Instance.currentDeck;
        cardsCount.text = $"{deck.cardsInDeck}/{DecksManager.Instance.maxDeckSize}";
        // Instantiate card items based on the deck's card list
        var sortedCards = deck.cardsCount
        .OrderBy(cardEntry => CardsManager.Instance.allCards[cardEntry.Key].cardName)
        .ToList();

        foreach (var cardEntry in sortedCards)
        {
            int cardID = cardEntry.Key;    // The Card ID
            int cardCount = cardEntry.Value; // The Card Count

            // Instantiate the card item prefab
            Michsky.UI.Reach.PanelButton cardItem = Instantiate(cardInDeckPrefab, cardsContentPanel);
            if (!isCalledFromDeck)
            {
             
                Animator animator = cardItem.GetComponent<Animator>();
                animator.SetTrigger("Idle");
            }

            // Get the TMP_Text to display the card's name and the count
            TMP_Text cardCountText = cardItem.transform.Find("Card Count").GetComponent<TMP_Text>();
            // Get card information from your card database
            CardData cardData = CardsManager.Instance.allCards[cardID];

            // Update the text to display the card's name and count (e.g., "CardName x3")
            cardItem.buttonText = cardData.cardName;
            if(cardCount > 1)
                cardCountText.text = $"x{cardCount}";

            cardItem.onClick.AddListener(() =>
            {
                DecksManager.Instance.RemoveCardFromDeck(cardID);
                ShowCards(null, false);
            });
            cardItem.onHover.AddListener(() => ShowCardDescription(cardData));
            cardItem.onExit.AddListener(() => cardWithDescriptionPrefab.gameObject.SetActive(false));

            cardItem.gameObject.SetActive(true);
            if(card_id != null && card_id == cardID)
                cardItem.GetComponent<Animator>().SetTrigger("CardAddedTrigger");
            cardItem.UpdateUI();
        }
    }
    public void ShowError(string message)
    {
        errorMessage.text = message;
        errorMessage.GetComponent<Animator>().SetTrigger("ErrorTrigger");
    }
    private void ShowCardDescription(CardData card)
    {
        TMP_Text cardText = cardWithDescriptionPrefab.transform.Find("Name Background/Card Name")?.GetComponent<TMP_Text>();
        TMP_Text cardDescription = cardWithDescriptionPrefab.transform.Find("Description Background/Card Description")?.GetComponent<TMP_Text>();

        Image cardImage = cardWithDescriptionPrefab.GetComponentInChildren<Image>();
        cardText.text = card.cardName; // Display card name
        cardDescription.text = card.cardDescription;
        cardImage.sprite = card.cardImage; // Display card image
    }
    private void EditDeck(Michsky.UI.Reach.PanelButton clickedDeck, DeckData deck)
    {
        if (selectedDeck != null)
            return; // already selected, ignore further clicks

        DecksManager.Instance.currentDeck = deck;
        ShowCards(null, true);
        selectedDeck = clickedDeck;
        deckTitle.GetComponent<TMP_Text>().text = deck.deckName;
        FadeOutAllDecks();
        FadeOutDecksTextAndFadeInTitle();
        cardsCount.gameObject.SetActive(true);
        saveButton.gameObject.SetActive(true);
    }
    private void FadeOutAllDecks()
    {
        foreach (Transform child in contentPanel)
        {
            Animator animator = child.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("FadeOut");
            }
        }
    }
    public void FadeOutDecksTextAndFadeInTitle()
    {
        // First, trigger the fade-out animation for "Decks" text
        Animator decksTextAnimator = decksText.GetComponent<Animator>();
        decksTextAnimator.SetTrigger("FadeOutText");

        // After the fade-out animation is done (wait for 1 second), trigger the fade-in for the deck title
        Invoke("TriggerFadeInTitle", 0.75f); // Adjust the delay to match the fade-out duration
        Invoke("TriggerFadeInCards", 0.75f); // Adjust the delay to match the fade-out duration
    }
   
    private void TriggerFadeInCards()
    {
        foreach (Transform child in cardsContentPanel)
        {
            Animator animator = child.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("FadeIn");
            }
        }
    }
    
    private void TriggerFadeInDecks()
    {
        foreach (Transform child in contentPanel)
        {
            Animator animator = child.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("FadeIn");
            }
        }
    }
    private void TriggerFadeInTitle()
    {
        Animator deckTitleAnimator = deckTitle.GetComponent<Animator>();
        deckTitleAnimator.SetTrigger("FadeInText");
    }
    private void DeleteDeck(DeckData deck)
    {
        deleteValidationPanel.InitializePanel(deck);
        deleteValidationPanel.OnDeletion -= ShowDecks;
        deleteValidationPanel.OnDeletion += ShowDecks;
    }
    private void CreateNewDeck()
    {

        DeckData newDeck = DecksManager.Instance.CreateNewDeck();
        ShowDecks();
        int addButtonExists = 1;
        if (DecksManager.Instance.decks.Count < 5)
            addButtonExists++;

        Transform secondToLastItem = contentPanel.GetChild(contentPanel.childCount - addButtonExists);
        PanelButton panelButton = secondToLastItem.GetComponentInChildren<PanelButton>(true);
        TMP_InputField inputField = secondToLastItem.GetComponentInChildren<TMP_InputField>(true);
        //TMP_Text deckNameText = secondToLastItem.GetComponentInChildren<TMP_Text>();
        if (inputField != null)
        {
            Debug.Log("InputField found!");
            if (panelButton != null)
                panelButton.isInteractable = false;
            // Set focus to the InputField or perform other actions
            inputField.gameObject.SetActive(true);
            //inputField.Select();
            inputField.text = "New Deck";
            inputField.ActivateInputField(); // This activates the InputField for user input
            inputField.onEndEdit.AddListener((string newDeckName) => {
                UpdateDeckName(newDeck, newDeckName);
                panelButton.buttonText = newDeckName;
                inputField.DeactivateInputField();
                inputField.gameObject.SetActive(false);
                if (panelButton != null)
                    panelButton.isInteractable = true;
                panelButton.UpdateUI();
            });
        }
        else
        {
            Debug.LogError("No InputField found in the second-to-last deckItemPrefab.");
        }
    }
    private void UpdateDeckName(DeckData deck, string newDeckName)
    {
        if (newDeckName != null && newDeckName != deck.deckName)
        {
            deck.deckName = newDeckName;
            DecksManager.Instance.UpdateDeckInDB(deck);
        }
    }
    public void SaveDeck()
    {
        DecksManager.Instance.SaveDeck();
        selectedDeck = null;
        FadeOutAllCards();
        FadeOutTitleAndFadeInDecksText();
        cardsCount.gameObject.SetActive(false);
        saveButton.gameObject.SetActive(false);
    }
    private void FadeOutAllCards()
    {
        foreach (Transform child in cardsContentPanel)
        {
            Animator animator = child.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("FadeOut");
            }
        }
    }
    public void FadeOutTitleAndFadeInDecksText()
    {
        // First, trigger the fade-out animation for "Decks" text
        Animator deckTitleAnimator = deckTitle.GetComponent<Animator>();
        deckTitleAnimator.SetTrigger("FadeOutText");

        // After the fade-out animation is done (wait for 1 second), trigger the fade-in for the deck title
        Invoke("TriggerFadeInText", 0.75f); // Adjust the delay to match the fade-out duration
        Invoke("TriggerFadeInDecks", 0.75f); // Adjust the delay to match the fade-out duration
    }
    private void TriggerFadeInText()
    {
        Animator decksTextAnimator = decksText.GetComponent<Animator>();
        decksTextAnimator.SetTrigger("FadeInText");
    }
}
