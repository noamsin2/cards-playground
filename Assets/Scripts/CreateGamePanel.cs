using Models;
using TMPro;
using UnityEngine;

public class CreateGamePanel : MonoBehaviour
{
    public CardGames game { get; private set; }
    [SerializeField] private MyCardsPanel myCardsPanel;
    [SerializeField] private GameObject gameID;
    [SerializeField] private GameObject gameName;
    [SerializeField] private GameObject publishButton;
    [SerializeField] private GameObject updateButton;
    public void InitializePanel(CardGames game)
    {
        this.game = game;
        ShowPublishButton();
        gameObject.SetActive(true);
        gameID.GetComponent<TextMeshProUGUI>().text = game.Game_ID.ToString();
        gameName.GetComponent<TextMeshProUGUI>().text = game.Name.ToString();
        myCardsPanel.InitializePanel(game);
    }
    private void ShowPublishButton()
    {
        publishButton.SetActive(false);
        updateButton.SetActive(false);
        if (game.Is_Published == false)
        {
            publishButton.SetActive(true);
        }
        else
            updateButton.SetActive(true);
    }
    public async void PublishGame()
    {
        bool isUpdated = await SupabaseManager.Instance.PublishGame(game);
        if (isUpdated)
        {
            game.Is_Published = true;
            publishButton.SetActive(false);
            updateButton.SetActive(true);
        }
    }
    public void OnHideButton()
    {
        publishButton.SetActive(true);
        updateButton.SetActive(false);
    }
    public async void UpdateGame()
    {
        // should schedule a game update here
        //
        //

        await SupabaseManager.Instance.PublishGame(game);
    }
}
