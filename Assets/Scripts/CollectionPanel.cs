using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Models;
using System.Collections;
using UnityEngine.Networking;
using System.Linq;

public class CollectionPanel : MonoBehaviour
{
    [SerializeField] private Transform cardContainer; // Grid layout parent
    [SerializeField] private GameObject cardPrefab; // Card UI prefab
    [SerializeField] private Button nextButton, prevButton;
    [SerializeField] private TMP_Text pageText;

    private Dictionary<int, CardData> allCards; // Stores all cards as a dictionary
    private List<int> cardIds; // Stores list of IDs for pagination purposes
    private int currentPage = 0;
    private const int CardsPerPage = 8; // Show 8 cards at a time

    void Start()
    {
        allCards = CardsManager.Instance.allCards;
        if(allCards == null)
        {
            Debug.Log("null");
        }
        cardIds = allCards.Keys
       .OrderBy(id => allCards[id].cardName)
       .ToList();
        UpdateUI();
    }

    void UpdateUI()
    {
        ClearCards();

        int startIdx = currentPage * CardsPerPage;
        int endIdx = Mathf.Min(startIdx + CardsPerPage, cardIds.Count);

        for (int i = startIdx; i < endIdx; i++)
        {
            int cardId = cardIds[i];  // Get the card ID from the list
            CardData card = allCards[cardId];
            GameObject cardObj = Instantiate(cardPrefab, cardContainer);
            TMP_Text cardText = cardObj.transform.Find("Name Background/Card Name")?.GetComponent<TMP_Text>();
            TMP_Text cardDescription = cardObj.transform.Find("Description Background/Card Description")?.GetComponent<TMP_Text>();

            Image cardImage = cardObj.GetComponentInChildren<Image>();
            cardText.text = card.cardName; // Display card name
            cardDescription.text = card.cardDescription;
            cardImage.sprite = card.cardImage; // Display card image
            Button cardButton = cardObj.GetComponentInChildren<Button>();
            cardButton.onClick.AddListener(() => AddCardToDeck(cardId));
        }

        pageText.text = $"Page {currentPage + 1}/{Mathf.CeilToInt((float)allCards.Count / CardsPerPage)}";
        prevButton.interactable = currentPage > 0;
        nextButton.interactable = endIdx < allCards.Count;
    }
    void AddCardToDeck(int cardId)
    {
        //CardData cardToAdd = allCards[cardId];

        DecksManager.Instance.AddCardToDeck(cardId);
    }
    public void NextPage()
    {
        if ((currentPage + 1) * CardsPerPage < allCards.Count)
        {
            currentPage++;
            UpdateUI();
        }
    }

    public void PrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            UpdateUI();
        }
    }

    void ClearCards()
    {
        foreach (Transform child in cardContainer)
        {
            Destroy(child.gameObject);
        }
    }
}
