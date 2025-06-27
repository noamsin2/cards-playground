using Microsoft.IdentityModel.Tokens;
using Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Michsky.UI.Reach;
public class MyCardsPanel : MonoBehaviour
{
    [SerializeField] private Transform contentPanel; // The Content area of your ScrollView (where the games will be listed)
    [SerializeField] private PanelButton cardItemPrefab; // A prefab that represents a card item (button)
    [SerializeField] private EditCardPanel editCardPanel; // A prefab that represents a card item (button)
    [SerializeField] private DeleteValidationPanel deleteValidationPanel;
    [SerializeField] private EffectsPanel editEffectsPanel;
    private CardGames game;
    public void InitializePanel(CardGames game)
    {
        this.game = game;
        DisplayCards();
    }
    public async void DisplayCards()
    {
        Debug.Log("DISPLAY CARDS");
        if (SupabaseManager.Instance != null)
        {
            List<Cards> cards = await SupabaseManager.Instance.GetCardsFromDatabase(game);
            
            ShowCards(cards);
        }
        else
        {
            Debug.LogError("SupabaseManager.Instance is null.");
        }
    }
    private void ShowCards(List<Cards> cards)
    {
        print("CARDS COUNT: " + cards.Count());
        // Clear the previous content
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        // Add each game to the UI
        foreach (var card in cards)
        {
            // Instantiate the prefab for each game
            PanelButton gameItem = Instantiate(cardItemPrefab, contentPanel);

            Button editButton = gameItem.transform.Find("Edit Card Btn").GetComponent<Button>(); // The edit button inside the prefab
            Button deleteButton = gameItem.transform.Find("Delete Card Btn").GetComponent<Button>(); // The delete button inside the prefab
            // Set the game name
            gameItem.buttonText = card.Name;
            
            // edit the game button
            editButton.onClick.AddListener(async () => await OnEditClick(card));
            deleteButton.onClick.AddListener(() => deleteValidationPanel.InitializePanel(card));
            // If the item is clickable (like a button), you can attach a click handler
            gameItem.onClick.AddListener(() => ShowCard(card));
            gameItem.onExit.AddListener(() => HideCard());

            gameItem.UpdateUI();
        }
    }
    private void HideCard()
    {
        DisplayCardPanel.Instance.HideCard();
    }
    private void ShowCard(Cards card)
    {
        Debug.Log($"Fetching details for card: {card.Name}");
        if (SupabaseManager.Instance != null)
        {
            // Fetch the card details by ID from the database
            string cardUrl = SupabaseManager.Instance.GetCardUrl(SupabaseManager.CARD_BUCKET,card.Image_URL);

            if (!cardUrl.IsNullOrEmpty())
            {
                Debug.Log($"Card details retrieved: {cardUrl}");
                // Pass the retrieved card details to the display panel
                DisplayCardPanel.Instance.ShowCard(card, cardUrl);
            }
            else
            {
                Debug.LogError("Card details not found.");
            }
        }
        else
        {
            Debug.LogError("SupabaseManager.Instance is null.");
        }
    }

    private async Task OnEditClick(Cards card)
    {
        await editEffectsPanel.OnEditCard(card.Card_ID);
        editCardPanel.InitializePanel(card);
    }
    
}
