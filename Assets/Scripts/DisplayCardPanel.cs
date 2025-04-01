using System.Collections;
using Models;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class DisplayCardPanel : MonoBehaviour
{
    public static DisplayCardPanel Instance;

    [SerializeField] private TMP_Text cardNameText; // Text field for the card name
    //[SerializeField] private TMP_Text cardDescriptionText; // Text field for the card description
    [SerializeField] private Image cardImage; // Image component for the card image
    [SerializeField] private GameObject cardDisplay;

    private void Awake()
    {
        Instance = this;
    }
    public void HideCard()
    {
        cardDisplay.SetActive(false);
        cardNameText.text = "";
        cardImage.sprite = null;
    }
    public void ShowCard(Cards card, string cardUrl)
    {
        cardNameText.text = card.Name;
        if (!string.IsNullOrEmpty(cardUrl))
        {
            StartCoroutine(LoadImageFromUrl(cardUrl));
        }
        
    }
    private IEnumerator LoadImageFromUrl(string url)
    {
        Debug.Log($"Loading image from URL: {url}");

        using (UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.Log("Image successfully loaded.");
                Texture2D texture = ((UnityEngine.Networking.DownloadHandlerTexture)request.downloadHandler).texture;
                cardImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                cardDisplay.SetActive(true);
            }
            else
            {
                Debug.LogError($"Error loading image: {request.error}");
                Debug.LogError($"Response Code: {request.responseCode}");
                Debug.LogError($"URL: {url}");
            }
        }
    }

}
