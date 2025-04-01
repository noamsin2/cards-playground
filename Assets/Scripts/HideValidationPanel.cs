using Models;
using UnityEngine;

public class HideValidationPanel : MonoBehaviour
{
    CardGames cardGame;
    [SerializeField] SettingsPanel settingsPanel;
    [SerializeField] CreateGamePanel createGamePanel;
    public void InitializePanel(CardGames cardGame)
    {
        this.cardGame = cardGame;
        gameObject.SetActive(true);
    }
    public async void HideGame()
    {
        await SupabaseManager.Instance.UnpublishGame(cardGame);
        cardGame.Is_Published = false;
        settingsPanel.OnHideButton();
        createGamePanel.OnHideButton();
    }
}
