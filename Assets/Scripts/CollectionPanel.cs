using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Models;
using System.Collections;
using UnityEngine.Networking;

public class CollectionPanel : MonoBehaviour
{
    [SerializeField] private Transform cardContainer; // Grid layout parent
    [SerializeField] private GameObject cardPrefab; // Card UI prefab
    [SerializeField] private Button nextButton, prevButton;
    [SerializeField] private TMP_Text pageText;

    private List<CardData> allCards; // Stores all cards
    private int currentPage = 0;
    private const int CardsPerPage = 8; // Show 8 cards at a time

    void Start()
    {
        allCards = CardsManager.Instance.allCards;
        UpdateUI();
    }

    void UpdateUI()
    {
        ClearCards();

        int startIdx = currentPage * CardsPerPage;
        int endIdx = Mathf.Min(startIdx + CardsPerPage, allCards.Count);

        for (int i = startIdx; i < endIdx; i++)
        {
            GameObject cardObj = Instantiate(cardPrefab, cardContainer);
            TMP_Text cardText = cardObj.transform.Find("Name Background/Card Name")?.GetComponent<TMP_Text>();
            TMP_Text cardDescription = cardObj.transform.Find("Description Background/Card Description")?.GetComponent<TMP_Text>();

            Image cardImage = cardObj.GetComponentInChildren<Image>();
            cardText.text = allCards[i].cardName; // Display card name
            cardDescription.text = allCards[i].cardDescription;
            cardImage.sprite = allCards[i].cardImage; // Display card image
        }

        pageText.text = $"Page {currentPage + 1}/{Mathf.CeilToInt((float)allCards.Count / CardsPerPage)}";
        prevButton.interactable = currentPage > 0;
        nextButton.interactable = endIdx < allCards.Count;
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
    //IEnumerator LoadImage(string url, Image cardImage, GameObject cardObj)
    //{
    //    if (string.IsNullOrEmpty(url)) yield break;
    //    url = SupabaseManager.Instance.GetCardUrl(url);
    //    UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
    //    yield return request.SendWebRequest();

    //    if (request.result == UnityWebRequest.Result.Success)
    //    {
    //        Texture2D texture = DownloadHandlerTexture.GetContent(request);
    //        cardImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    //        cardObj.SetActive(true);
    //    }
    //    else
    //    {
    //        Debug.LogError($"Failed to load image: {url}");
    //    }
    //}
}
