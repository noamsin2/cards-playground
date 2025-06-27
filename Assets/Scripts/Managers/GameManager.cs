using System;
using Models;
using Newtonsoft.Json;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public PublishedGames game { get; private set; }
    public GameSettings settings { get; private set; }
    public static GameManager Instance { get; private set; }
    public event Action OnGameLoaded;
    public int game_id { get; private set; }
    [SerializeField] private OptionsPanel optionsPanel; // Assign the Settings Panel in the Inspector

    private void Update()
    {
        // Check if ESC key is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            optionsPanel.ToggleOptionsPanel();
        }
    }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        
    }
    async void Start()
    {
        string gameID = PlayerPrefs.GetString("SelectedGameID", "default_value");

        // Check if gameID was successfully retrieved
        if (gameID != "default_value")
        {
            game_id = int.Parse(gameID);
            // GameID is retrieved successfully, use it as needed
            Debug.Log("Game ID: " + gameID);
            game = await SupabaseManager.Instance.GetGameFromDatabase(int.Parse(gameID));

            if (game != null)
            {
                settings = JsonConvert.DeserializeObject<GameSettings>(game.Game_Settings);
                
                await SupabaseManager.Instance.SubscribeToGameEvents(game_id);
                // Call the event when the game is fully loaded
                OnGameLoaded?.Invoke();
            }
            else
            {
                Debug.LogError("Failed to load game from database.");
            }
        }
        else
        {
            // Handle the case where no game ID is stored
            Debug.Log("No Game ID found in PlayerPrefs.");
        }
    }
}
