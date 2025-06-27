using UnityEngine;
using TMPro;

public class MessagePanel : MonoBehaviour
{
    public static MessagePanel Instance;

    [SerializeField] private TMP_Text messageText;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Prevent duplicates
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        //gameObject.SetActive(false); // Hide by default
    }

    public void ShowMessage(string message)
    {
        if (messageText != null)
        {
            Debug.Log("Showing message: " + message);
            messageText.text = message;
            gameObject.SetActive(true);
            Debug.Log("Message panel active: " + gameObject.activeInHierarchy);
        }
        else
        {
            Debug.LogWarning("Message Text is not assigned in the MessagePanel.");
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
