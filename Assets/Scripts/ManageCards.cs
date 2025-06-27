using UnityEngine;
using TMPro;
using Supabase.Storage;
using Models;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using static EffectsPanel;
using UnityEngine.UI;
using System;
using System.Linq;
public class ManageCards : MonoBehaviour
{
    //public SupabaseIntegration supabaseIntegration;
    [SerializeField] private TMP_InputField cardName;
    [SerializeField] private GameObject gameID;
    [SerializeField] private EditCardPanel editCardPanel;
    [SerializeField] private EffectsPanel effectsPanel;
    [SerializeField] private TMP_Text fileText;
    [SerializeField] private MyCardsPanel myCardsPanel;
    [SerializeField] private GameObject errorMessage;
    [SerializeField] private NewCardPanel newCardPanel;
    public async void UploadFileToSupabase()
    {
        if (effectsPanel.existingEffects.All(kvp => kvp.Value == null || kvp.Value.Count == 0))
        {
            Debug.LogError("NO EFFECTS");
            ShowError();
            return;
        }
        
        int fk_game_id = int.Parse(gameID.GetComponent<TextMeshProUGUI>().text);
        Dictionary<string, List<string>> cardEffects = new Dictionary<string, List<string>>();
        Dictionary<string, List<EffectWithX>>  existingEffects = effectsPanel.existingEffects;
        Debug.Log("UPLOAD FILE TO SUPABASE");
        Debug.Log(FileUpload.selectedFilePath);
        string filePath = FileUpload.selectedFilePath;
        
        
        if (!string.IsNullOrEmpty(filePath))
        {
            
            await SupabaseManager.Instance.UploadCard(filePath, cardName.text, fk_game_id, existingEffects);
            FileUpload.selectedFilePath = null;
            myCardsPanel.DisplayCards();
        }
        else
        {
            Debug.LogWarning("No file selected for upload.");
        }
        cardName.text = string.Empty;
        fileText.text = string.Empty;
        myCardsPanel.DisplayCards();
        newCardPanel.gameObject.SetActive(false);

    }
    public async void EditFileInSupabase()
    {
        Debug.Log(FileUpload.selectedFilePath);
        Cards card = editCardPanel.card;
        Debug.Log(cardName.text);
        if (card != null)
        {
            var updatedEffects = effectsPanel.existingEffects;
            if (updatedEffects.All(kvp => kvp.Value == null || kvp.Value.Count == 0))
            {
                Debug.LogError("NO EFFECTS");
                ShowError();
                return;
            }
            int uploadFlag = editCardPanel.uploadFlag;
            if (cardName.text != card.Name || uploadFlag == 1)
            {
                Debug.Log("upload flag: " + editCardPanel.uploadFlag);
                await SupabaseManager.Instance.EditCardOnDatabase(card, cardName.text, card.Image_URL, editCardPanel.uploadFlag == 1);
            }
            if (!string.IsNullOrEmpty(FileUpload.selectedFilePath))
            {
                await SupabaseManager.Instance.EditCardOnStorage(card, FileUpload.selectedFilePath);
                editCardPanel.uploadFlag = 0;
            }
            
            if (updatedEffects != null)
            {
                await SupabaseManager.Instance.EditCardEffectsInDatabase(updatedEffects, card.Card_ID);
            }
            Debug.Log("Card and effects updated successfully.");
            FileUpload.selectedFilePath = null;
            myCardsPanel.DisplayCards();
            editCardPanel.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("No card selected for editing.");
        }
    }
    private void ShowError()
    {
        errorMessage.GetComponent<Animator>().SetTrigger("ErrorTrigger");
    }
}
