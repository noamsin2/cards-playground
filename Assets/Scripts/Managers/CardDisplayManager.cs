//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;
//using System.Collections;
//using UnityEngine.Networking;
//using Models;

//public class CardDisplayManager : MonoBehaviour
//{
//    CardGames game;
//    public GameObject cardButtonPrefab;
//    public Transform cardListPanel;
//    public Image cardImage;
//    public TMP_Text cardNameText;

//    private Dictionary<string, string> cardData = new Dictionary<string, string>();

//    private SupabaseManager supabaseManager;  // Reference to your SupabaseManager

//    public void InitializeManager(CardGames game)
//    {
//        this.game = game;
//        supabaseManager = SupabaseManager.Instance;
        
//        FetchCardData(game);
//    }

//    private async void FetchCardData(CardGames game)
//    {
//        // Fetch the card data from Supabase through SupabaseManager
//        try
//        {
//            var cards = await supabaseManager.GetCardsFromDatabase(game);

//            if (cards != null)
//            {
//                // Process the fetched card data
//                var fetchedCards = new Dictionary<string, string>();
//                foreach (var card in cards)
//                {
//                    // Assuming card has "Name" and "ImageUrl" columns
//                    fetchedCards[card.Name] = card.ImageUrl;
//                }

//                // Populate the UI with the fetched cards
//                PopulateCardList(fetchedCards);
//            }
//            else
//            {
//                Debug.LogWarning("No card data found in Supabase.");
//            }
//        }
//        catch (System.Exception e)
//        {
//            Debug.LogError($"Error fetching card data: {e.Message}");
//        }
//    }

//    // Called after fetching card data from Supabase
//    public void PopulateCardList(Dictionary<string, string> fetchedCards)
//    {
//        cardData = fetchedCards;

//        // Clear the previous card buttons
//        foreach (Transform child in cardListPanel)
//        {
//            Destroy(child.gameObject);
//        }

//        // Populate the card list with new buttons
//        foreach (var card in cardData)
//        {
//            GameObject buttonObj = Instantiate(cardButtonPrefab, cardListPanel);
//            TMP_Text buttonText = buttonObj.GetComponentInChildren<TMP_Text>();
//            buttonText.text = card.Key;

//            Button button = buttonObj.GetComponent<Button>();
//            string imageUrl = card.Value; // Store the image URL
//            button.onClick.AddListener(() => ShowCard(imageUrl, card.Key));
//        }
//    }

//    private void ShowCard(string imageUrl, string cardName)
//    {
//        cardNameText.text = cardName;
//        StartCoroutine(LoadCardImage(imageUrl));
//    }

//    private IEnumerator LoadCardImage(string imageUrl)
//    {
//        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl))
//        {
//            yield return request.SendWebRequest();

//            if (request.result == UnityWebRequest.Result.Success)
//            {
//                Texture2D texture = DownloadHandlerTexture.GetContent(request);
//                cardImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
//            }
//            else
//            {
//                Debug.LogError("Failed to load card image: " + request.error);
//            }
//        }
//    }
//}
