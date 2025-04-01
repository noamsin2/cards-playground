using Models;
using TMPro;
using UnityEngine;

public class RestoreValidationPanel : MonoBehaviour
{
    private CardGames cardGame;
    [SerializeField] new private TextMeshProUGUI name;
    [SerializeField] private MyGamesPanel myGamesPanel;

    public void InitializePanel(CardGames cardGame)
    {
        this.cardGame = cardGame;
        name.text += cardGame.Name + "?";
        gameObject.SetActive(true);
    }
    public void OnNoClick()
    {
        cardGame = null;
        name.text = "Restore ";
    }
    public async void RestoreGame()
    {
        if (cardGame != null)
        {
            // change isDeleted to true in database
            await SupabaseManager.Instance.RestoreGame(cardGame);
            // refresh the game list
            myGamesPanel.DisplayGames();
            myGamesPanel.DisplayDeletedGames();
            cardGame = null;
            name.text = "Restore ";
        }
    }
}