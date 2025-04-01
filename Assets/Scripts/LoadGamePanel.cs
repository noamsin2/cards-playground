using UnityEngine;

public class LoadGamePanel : MonoBehaviour
{
    [SerializeField] private GameMenuPanel gameMenuPanel;

    async void Start()
    {
        await CardsManager.Instance.LoadAllCards(PlayerPrefs.GetString("SelectedGameID"));
        gameMenuPanel.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }
}
