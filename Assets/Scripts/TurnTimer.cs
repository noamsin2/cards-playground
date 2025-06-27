using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TurnTimer : MonoBehaviour
{
    [SerializeField] private float turnLength; // Duration of each turn (in seconds)
    [SerializeField] private TMP_Text timerText; // UI element to display the timer

    private float remainingTime;
    private bool isTimerRunning = false;

    public delegate void TimerExpiredAction();
    public event TimerExpiredAction OnTimerExpired;

    private void Start()
    {
        turnLength = GameManager.Instance.settings.turn_length;
        ResetTimer(); // Initialize timer
    }

    private void Update()
    {
        if (isTimerRunning)
        {
            remainingTime -= Time.deltaTime;

            if (remainingTime <= 0)
            {
                remainingTime = 0;
                isTimerRunning = false;
                timerText.text = "0:00";

                // Trigger timer expired actions
                OnTimerExpired?.Invoke();
            }
            else
            {
                // Update timer display
                timerText.text = $"{Mathf.FloorToInt(remainingTime / 60)}:{Mathf.FloorToInt(remainingTime % 60):00}";
            }
        }
    }

    public void StartTimer()
    {
        isTimerRunning = true;
    }

    public void ResetTimer()
    {
        remainingTime = turnLength;
        isTimerRunning = false;
        timerText.text = $"{Mathf.FloorToInt(remainingTime / 60)}:{Mathf.FloorToInt(remainingTime % 60):00}";
    }
}