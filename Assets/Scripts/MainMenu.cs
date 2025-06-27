using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    const int CREATE_GAME_SCENE = 0;
    [SerializeField] private GameObject adminDashboardButton;
    private void Awake()
    {
        UserManager.Instance.onUserLogIn += DisplayAdminButton;
    }
    
    private void DisplayAdminButton()
    {
        UserManager.Instance.onUserLogIn -= DisplayAdminButton;
        if(UserManager.Instance.isAdmin == true)
            adminDashboardButton.SetActive(true);
        else 
            adminDashboardButton.SetActive(false);
    }

    public void CreateGame()
    {
        SceneManager.LoadSceneAsync(CREATE_GAME_SCENE);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        // Only included in the Unity Editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Included in builds
        Application.Quit();
#endif
    }
}
