using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.DebugUI;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;
    void Awake()
    {
        // Check if an instance already exists
        if (Instance == null)
        {
            // If not, set this as the instance and don't destroy it on load
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // If an instance already exists, destroy this one
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        //for testing - remove later
        //LoadGameScene("6");
    }
    public void LoadGameScene(string gameId)
    {
        PlayerPrefs.SetString("SelectedGameID", gameId);
        //await Task.Run(() => CardsManager.Instance.LoadAllCards(gameId));
        SceneManager.LoadScene("Play Game Scene");
    }

    public void LoadMenuScene()
    {
        PlayerPrefs.DeleteKey("SelectedGameID");
        SceneManager.LoadScene("Create Game Scene");
    }
    public void LoadMenuScene(string message)
    {
        Debug.Log("Loading menu scene and preparing to show message...");

        PlayerPrefs.DeleteKey("SelectedGameID");

        // Define the handler so it can be unsubscribed properly
        UnityEngine.Events.UnityAction<Scene, LoadSceneMode> handler = null;

        handler = (Scene scene, LoadSceneMode mode) =>
        {
            if (MessagePanel.Instance != null)
            {
                MessagePanel.Instance.ShowMessage(message);
            }
            else
            {
                Debug.LogWarning("MessagePanel.Instance is null!");
            }

            // Unsubscribe from event
            SceneManager.sceneLoaded -= handler;
            Debug.Log("Unsubscribed from sceneLoaded event.");
        };

        SceneManager.sceneLoaded += handler;
        Debug.Log("Subscribed to sceneLoaded event, loading scene...");

        SceneManager.LoadScene("Create Game Scene");
    }
}