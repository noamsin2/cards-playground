using Models;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Michsky.UI.Reach;
public class GameDetailsPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text gameNameText;
    [SerializeField] private GameObject gameDescriptionText;
    [SerializeField] private GameObject settingsText;
    [SerializeField] private Michsky.UI.Reach.ButtonManager playButton;
    private PublishedGames game;

    private void Start()
    {
        gameDescriptionText.SetActive(false);
        settingsText.SetActive(false);
        gameNameText.gameObject.SetActive(false);
        playButton.gameObject.SetActive(false);
    }
    public void ShowGameDetails(PublishedGames game)
    {
        this.game = game;
        gameNameText.text = game.Name;
        gameNameText.gameObject.SetActive(true);
        gameDescriptionText.transform.Find("Game's Description").GetComponent<TMP_Text>().text = game.Description;

        gameDescriptionText.gameObject.SetActive(true);
        try
        {
            GameSettings settings = JsonConvert.DeserializeObject<GameSettings>(game.Game_Settings);
            settingsText.transform.Find("Game Settings").GetComponent<TMP_Text>().text = $"Win Condition: {(settings.health_win_condition ? "Health" : "Cards")}\n" +
                                $"Player Health: {settings.player_health}\n" +
                                $"Deck Size: {settings.deck_size}\n" +
                                $"Initial Hand: {settings.initial_hand_size}\n" +
                                $"Max Hand Size: {settings.max_hand_size}\n" +
                                $"Turn Length: {settings.turn_length} sec\n" +
                                $"Card Copies: {settings.card_copies}\n" +
                                $"Limit Hand Size: {settings.limit_hand_size}";
            settingsText.gameObject.SetActive(true);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to parse game settings: " + e.Message);
            settingsText.transform.Find("Game Settings").GetComponent<TMP_Text>().text = "Error loading settings.";
        }

        playButton.gameObject.SetActive(true);
        playButton.onClick.RemoveAllListeners();
        playButton.onClick.AddListener(() =>
        {
            Debug.Log("Play button clicked");
            PlayGame(game);
        });
    }
    private async void PlayGame(PublishedGames game)
    {
        int gameID = game.Game_ID;
        if (game.Updated_At == null)
        {
            Debug.Log("PlayGame called with ID: " + gameID);
            await SupabaseManager.Instance.LogGameLogin(UserManager.Instance.userId, gameID);
            SceneLoader.Instance.LoadGameScene(gameID.ToString());
        }
        else
        {
            // view an error message (game is being updated)
        }
    }
}
