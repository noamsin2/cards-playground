using Models;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Michsky.UI.Reach;
public class GameDetailsAdminPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text gameNameText;
    [SerializeField] private GameObject gameDescriptionText;
    [SerializeField] private GameObject settingsText;
    [SerializeField] private DeleteValidationPanel deleteValidationPanel;
    [SerializeField] private GameStatisticsPanel gameStatisticsPanel;
    [SerializeField] private Michsky.UI.Reach.ButtonManager deleteButton;
    [SerializeField] private Michsky.UI.Reach.ButtonManager statisticsButton;
    private CardGames game;

    private void Start()
    {
        gameDescriptionText.SetActive(false);
        settingsText.SetActive(false);
        gameNameText.gameObject.SetActive(false);
        deleteButton.gameObject.SetActive(false);
        statisticsButton.gameObject.SetActive(false);
    }
    public void ShowGameDetails(CardGames game)
    {
        this.game = game;
        gameNameText.text = game.Name;
        gameNameText.gameObject.SetActive(true);
        if (game.Description != null)
            gameDescriptionText.transform.Find("Game's Description").GetComponent<TMP_Text>().text = game.Description;
        else
            gameDescriptionText.transform.Find("Game's Description").GetComponent<TMP_Text>().text = "";

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

        statisticsButton.gameObject.SetActive(true);
        statisticsButton.onClick.RemoveAllListeners();
        statisticsButton.onClick.AddListener(() =>
        {
            Debug.Log("Statistics button clicked");
            gameStatisticsPanel.InitializePanel(game.Game_ID);
        });

        deleteButton.gameObject.SetActive(true);
        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(() =>
        {
            Debug.Log("Delete button clicked");
            DeleteGame(game);
        });
    }
    private void DeleteGame(CardGames game)
    {
        deleteValidationPanel.InitializePanel(game);
    }
}
