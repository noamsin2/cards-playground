using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameMenuPanel : MonoBehaviour
{
    [SerializeField] private Button exitButton;
    [SerializeField] private TMP_Text gameNameText;
    void Start()
    {
        exitButton.onClick.AddListener(() => SceneLoader.Instance.LoadMenuScene());
        GameManager.Instance.OnGameLoaded += UpdateUI;
    }
    private void UpdateUI()
    {
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

    private void OnDestroy()
    {
        // Unsubscribe from the event to avoid memory leaks
        GameManager.Instance.OnGameLoaded -= UpdateUI;
    }
}
