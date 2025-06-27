using System;
using Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditCardPanel : MonoBehaviour
{
    public Cards card { private set; get; }
    [SerializeField] private TMP_InputField cardNameTMP;
    [SerializeField] private TextMeshProUGUI cardID;
    [SerializeField] private FileUpload fileUpload;
    [SerializeField] private GameObject cardPreview;
    [SerializeField] private TMP_Text cardImageText;
    public int uploadFlag { get; set; } = 0; 
    private int cardId = -1;
    public async void InitializePanel(Cards card)
    {
        
        this.card = card;
        if (card.Card_ID != cardId || uploadFlag == 1)
        {
            cardId = card.Card_ID;
            cardPreview.SetActive(false);
            var fileBytes = await SupabaseManager.Instance.DownloadFileFromSupabase(SupabaseManager.CARD_BUCKET, card.Image_URL);
            ShowImageInPanel(fileBytes);
            cardImageText.text = "";
        }
        cardNameTMP.text = card.Name;
        cardID.text = card.Card_ID.ToString();
        
        //cardImageText.text = "";
       
        gameObject.SetActive(true);
    }
    public void UploadCard()
    {
        fileUpload.ShowFileDialog(path =>
        {
            if (path != null)
            {
                ShowImageInPanel(path);
                uploadFlag = 1;
            }
            else
                Debug.Log("User canceled the dialog.");
        });

        //ShowImageInPanel(filePath);
        
    }
    // gets called when loading the edit card panel
    private void ShowImageInPanel(byte[] fileBytes)
    {
        Texture2D texture = new Texture2D(2, 2);
        if (texture.LoadImage(fileBytes))
        {
            RawImage rawImage = cardPreview.GetComponent<RawImage>();
            rawImage.texture = texture;
            cardPreview.SetActive(true);
        }
        else
        {
            Debug.LogError("Failed to load texture from bytes.");
        }
    }

    // gets called after updating a card image
    private void ShowImageInPanel(string filePath)
    {
        byte[] fileBytes;
        try
        {
            fileBytes = System.IO.File.ReadAllBytes(filePath);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to read file: " + e.Message);
            return;
        }
        Texture2D texture = new Texture2D(2, 2);
        if (texture.LoadImage(fileBytes))
        {
            RawImage rawImage = cardPreview.GetComponent<RawImage>();
            rawImage.texture = texture;
            //rawImage.rectTransform.sizeDelta = new Vector2(texture.width, texture.height);
            cardPreview.SetActive(true);
        }
        else
        {
            Debug.LogError("Failed to load texture from bytes.");
        }
    }
}
