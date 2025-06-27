using Models;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.Reach;

public class MyGamesPanel : MonoBehaviour
{ 
    [SerializeField] private Transform contentPanel; // The Content area of your ScrollView (where the games will be listed)
    [SerializeField] private Transform deletedGamesContentPanel; // The Content area of your ScrollView (where the games will be listed)
    [SerializeField] private GameObject CreateGameErrorPanel;
    [SerializeField] private GameObject gameItemPrefab; // A prefab that represents a game item (button)
    [SerializeField] private Michsky.UI.Reach.PanelButton deletedGameItemPrefab; // A prefab that represents a deleted game item (button)
    [SerializeField] private CreateGamePanel createGamePanel;
    [SerializeField] private DeleteValidationPanel deleteValidationPanel;
    [SerializeField] private RestoreValidationPanel restoreValidationPanel;
    [SerializeField] private GameObject settingsPanel;
    private void OnEnable()
    {
        Debug.Log("ON ENABLE");
        DisplayGames();
    }

    public async void DisplayGames()
    {
        Debug.Log("DISPLAY GAMES");
        if (SupabaseManager.Instance != null)
        {
            List<CardGames> games = await SupabaseManager.Instance.RetrieveAllGames();
            ShowGames(games);
        }
        else
        {
            Debug.LogError("SupabaseManager.Instance is null.");
        }
    }
    public async void DisplayDeletedGames()
    {
        Debug.Log("DISPLAY GAMES");
        if (SupabaseManager.Instance != null)
        {
            List<CardGames> games = await SupabaseManager.Instance.RetrieveDeletedGames();
            ShowDeletedGames(games);
            
        }
        else
        {
            Debug.LogError("SupabaseManager.Instance is null.");
        }
    }
    private void ShowGames(List<CardGames> games)
    {
        // Clear the previous content
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        // Add each game to the UI
        foreach (var game in games)
        {
            // Instantiate the prefab for each game
            GameObject gameItem = Instantiate(gameItemPrefab, contentPanel);
            // Find the Text or Button inside the prefab (depending on your design)
            //TMP_Text gameNameText = gameItem.GetComponentInChildren<TMP_Text>(); // Assuming the prefab has a TMP_Text component
            PanelButton gameButton = gameItem.GetComponent<PanelButton>(); // If you want to make it clickable
            Button editButton = gameItem.transform.Find("Edit Btn").GetComponent<Button>(); // The edit button inside the prefab
            Button deleteButton = gameItem.transform.Find("Delete Btn").GetComponent<Button>(); // The edit button inside the prefab
                                                                                                     // Set the game name
            gameButton.buttonText = game.Name;
            gameButton.UpdateUI();
            // edit the game button
            editButton.onClick.AddListener(() => OpenEditPanel(game));
            // open delete game validation panel
            deleteButton.onClick.AddListener(() => deleteValidationPanel.InitializePanel(game));
            // If the item is clickable (like a button), you can attach a click handler
            gameButton.onClick.AddListener(() => OnGameClicked(game));
        }
    }
    public void ShowDeletedGames(List<CardGames> games)
    {
        // Clear the previous content
        foreach (Transform child in deletedGamesContentPanel)
        {
            Destroy(child.gameObject);
        }

        // Add each game to the UI
        foreach (var game in games)
        {
            // Instantiate the prefab for each game
            var gameItem = Instantiate(deletedGameItemPrefab, deletedGamesContentPanel);
            // Find the Text or Button inside the prefab (depending on your design)
            gameItem.buttonText = game.Name;
            Button restoreButton = gameItem.transform.Find("Restore Game Btn").GetComponent<Button>(); // The restore button inside the prefab
            gameItem.UpdateUI();

            // open delete game validation panel
            restoreButton.onClick.AddListener(() => restoreValidationPanel.InitializePanel(game));
        }
    }
    private void OnGameClicked(CardGames game)
    {
        Debug.Log($"Game {game.Name} clicked!");
        // Play the game
    }
    private void OpenEditPanel(CardGames game)
    {
        createGamePanel.InitializePanel(game);
        gameObject.SetActive(false);
        settingsPanel.SetActive(false);
    }
}
