using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewCardPanel : MonoBehaviour
{
    [SerializeField] private FileUpload fileUpload;
    [SerializeField] private GameObject cardPreview;
    [SerializeField] private TMP_InputField cardNameInput;
    [SerializeField] private TMP_Text cardImageText;
    public void InitializePanel()
    {
        cardPreview.SetActive(false);
        cardNameInput.text = "";
        cardImageText.text = "";
        gameObject.SetActive(true);
    }
    public void UploadCard()
    {
        fileUpload.ShowFileDialog(path =>
        {
            if (path != null)
            {
                ShowImageInPanel(path);
            }
            else
                Debug.Log("User canceled the dialog.");
        });
    }
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
