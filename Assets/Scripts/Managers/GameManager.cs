using Models;
using Newtonsoft.Json;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public PublishedGames game { get; private set; }
    public GameSettings settings { get; private set; }
    public static GameManager Instance { get; private set; }
    public delegate void GameLoadedDelegate();
    public event GameLoadedDelegate OnGameLoaded;
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
            // GameID is retrieved successfully, use it as needed
            Debug.Log("Game ID: " + gameID);
            game = await SupabaseManager.Instance.GetGameFromDatabase(int.Parse(gameID));

            if (game != null)
            {
                settings = JsonConvert.DeserializeObject<GameSettings>(game.Game_Settings);
                Debug.Log("Player Health: " + settings.player_health);

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
