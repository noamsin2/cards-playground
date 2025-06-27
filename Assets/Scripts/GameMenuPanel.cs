using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.Reach;
using System;
public class GameMenuPanel : MonoBehaviour
{
    [SerializeField] private Michsky.UI.Reach.ButtonManager exitButton;
    [SerializeField] private TMP_Text gameNameText;
    [SerializeField] private GameObject updateMessage;
    private TMP_Text timerText;
    private DateTime? logoutTime = null;

    void Update()
    {
        if (logoutTime != null)
        {
            TimeSpan timeLeft = logoutTime.Value - DateTime.Now;
            if (timeLeft.TotalSeconds <= 0)
            {
                timerText.text = "Logging out...";
                // Optionally trigger logout now
            }
            else
            {
                timerText.text = string.Format(
                    "{0:HH:mm}. Time left: {1:D2}:{2:D2}:{3:D2}",
                    logoutTime,
                    timeLeft.Hours,
                    timeLeft.Minutes,
                    timeLeft.Seconds
                );
            }
        }
    }
    void Start()
    {
        timerText = updateMessage.transform.Find("Time To Update").GetComponent<TMP_Text>();
        exitButton.onClick.AddListener(() => HandleLogout());
        GameManager.Instance.OnGameLoaded += UpdateUI;
        SupabaseManager.Instance.OnLogout += HandleLogout;
        SupabaseManager.Instance.OnUpdate += HandleUpdateMessage;
        UpdateUI();
    }
    private void UpdateUI()
    {
        GameManager.Instance.OnGameLoaded -= UpdateUI;
        // Ensure that the gameNameText only gets updated once the game data is available
        if (GameManager.Instance.game != null)
        {
            gameNameText.text = GameManager.Instance.game.Name;
        }
        else
        {
            Debug.LogError("Game data is null!");
        }
    }
    private void HandleLogout()
    {
        SupabaseManager.Instance.OnLogout -= HandleLogout;
        SupabaseManager.Instance.UnsubscribeFromGameEvents();
        UnityMainThreadDispatcher.Instance.Enqueue(() =>
        {
            string message = "The game has been updated and you've been logged out.";
            SceneLoader.Instance.LoadMenuScene(message);
        });
    }
    private void HandleUpdateMessage(int gameId)
    {
        SupabaseManager.Instance.OnUpdate -= HandleUpdateMessage;
        UnityMainThreadDispatcher.Instance.Enqueue(() =>
        {
            ShowError();
            GetLogoutTime(gameId);
        });
        
    }
    private async void GetLogoutTime(int gameId)
    {
        DateTime? updatedAt = await SupabaseManager.Instance.GetPublishedGameUpdatedAt(gameId);
        DateTime now = DateTime.UtcNow; // Or DateTime.Now if you're using local time


        if (updatedAt.HasValue)
        {
            // Add 1 hour grace period
            DateTime minLogoutTime = updatedAt.Value.AddHours(1);
            logoutTime = new DateTime(
            minLogoutTime.Year,
            minLogoutTime.Month,
            minLogoutTime.Day,
            minLogoutTime.Hour,
            0, 0
        ).AddHours(minLogoutTime.Minute > 0 || minLogoutTime.Second > 0 ? 1 : 0);
        }
        else
        {
            Debug.LogError("updatedAt is null, cannot calculate logout time.");
        }
        // Round up to the next whole hour (next cron run after grace period)
        
    }
    private void ShowError()
    {
        updateMessage.SetActive(true);
    }
    private void OnDestroy()
    {
        // Unsubscribe from the event to avoid memory leaks
        GameManager.Instance.OnGameLoaded -= UpdateUI;
    }
}
