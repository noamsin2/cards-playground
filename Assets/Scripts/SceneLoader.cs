using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        LoadGameScene("6");
    }
    public void LoadGameScene(string gameId)
    {
        PlayerPrefs.SetString("SelectedGameID", gameId);
        //await Task.Run(() => CardsManager.Instance.LoadAllCards(gameId));
        SceneManager.LoadScene("Play Game Scene");
    }

    public void LoadMenuScene()
    {
        SceneManager.LoadScene("Create Game Scene");
    }
}