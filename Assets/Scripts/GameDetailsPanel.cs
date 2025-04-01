using Models;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameDetailsPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text gameNameText;
    [SerializeField] private GameObject gameDescriptionText;
    [SerializeField] private GameObject settingsText;
    [SerializeField] private Button playButton;
    private PublishedGames game;
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
        playButton.onClick.AddListener(() => SceneLoader.Instance.LoadGameScene(game.Game_ID.ToString()));
    }
    
}
