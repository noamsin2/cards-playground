using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    const int CREATE_GAME_SCENE = 0;
    public void CreateGame()
    {
        SceneManager.LoadSceneAsync(CREATE_GAME_SCENE);
    }

    public void Quit()
    {
        // Check if we're running in the Unity Editor
        if (Application.isEditor)
        {
            // Stop playing the scene in the editor (simulates quitting)
            EditorApplication.isPlaying = false;
        }
        else
        {
            // Quit the game in a built version
            Application.Quit();
        }
    }
}
