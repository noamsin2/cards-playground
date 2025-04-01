using UnityEngine;
using TMPro;
using Supabase.Storage;
using Models;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using static EffectsPanel;
public class ManageCards : MonoBehaviour
{
    //public SupabaseIntegration supabaseIntegration;
    [SerializeField] private TMP_InputField cardName;
    [SerializeField] private GameObject gameID;
    [SerializeField] private EditCardPanel editCardPanel;
    [SerializeField] private EffectsPanel effectsPanel;
    [SerializeField] private TMP_Text fileText;
    [SerializeField] private MyCardsPanel myCardsPanel;
    public async void UploadFileToSupabase()
    {
        int fk_game_id = int.Parse(gameID.GetComponent<TextMeshProUGUI>().text);
        Dictionary<string, List<string>> cardEffects = new Dictionary<string, List<string>>();
        Dictionary<string, List<EffectWithX>>  existingEffects = effectsPanel.existingEffects;
        Debug.Log("UPLOAD FILE TO SUPABASE");
        Debug.Log(FileUpload.selectedFilePath);
        if (!string.IsNullOrEmpty(FileUpload.selectedFilePath))
        {
            await SupabaseManager.Instance.UploadCard(FileUpload.selectedFilePath, cardName.text, fk_game_id, existingEffects);
            FileUpload.selectedFilePath = null;
            myCardsPanel.DisplayCards();
        }
        else
        {
            Debug.LogWarning("No file selected for upload.");
        }
        cardName.text = string.Empty;
        fileText.text = string.Empty;
    }
    public async void EditFileInSupabase()
    {
        Debug.Log(FileUpload.selectedFilePath);
        Cards card = editCardPanel.card;
        Debug.Log(cardName.text);
        if (card != null)
        {
            if (cardName.text != card.Name)
            {
                await SupabaseManager.Instance.EditCardOnDatabase(card, cardName.text, card.Image_URL);
            }
            if (!string.IsNullOrEmpty(FileUpload.selectedFilePath))
            {
                await SupabaseManager.Instance.EditCardOnStorage(card, FileUpload.selectedFilePath);
            }
            var updatedEffects = effectsPanel.existingEffects;
            if (updatedEffects != null)
            {
                await SupabaseManager.Instance.EditCardEffectsInDatabase(updatedEffects, card.Card_ID);
            }
            Debug.Log("Card and effects updated successfully.");
            FileUpload.selectedFilePath = null;
        }
        else
        {
            Debug.LogWarning("No card selected for editing.");
        }
    }
}
