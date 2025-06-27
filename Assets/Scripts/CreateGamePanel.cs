using Michsky.UI.Reach;
using Models;
using TMPro;
using UnityEngine;

public class CreateGamePanel : MonoBehaviour
{
    public CardGames game { get; private set; }
    [SerializeField] private MyCardsPanel myCardsPanel;
    [SerializeField] private GameStatisticsPanel gameStatisticsPanel;
    [SerializeField] private GameObject gameID;
    [SerializeField] private GameObject gameName;
    [SerializeField] private GameObject publishButton;
    [SerializeField] private GameObject updateButton;
    [SerializeField] private ButtonManager statisticsButton;
    [SerializeField] private TMP_Text errorMessage;
    public void InitializePanel(CardGames game)
    {
        this.game = game;
        ShowPublishButton();
        gameObject.SetActive(true);
        gameID.GetComponent<TextMeshProUGUI>().text = game.Game_ID.ToString();
        gameName.GetComponent<TextMeshProUGUI>().text = game.Name.ToString();
        myCardsPanel.InitializePanel(game);
        //statisticsButton.onClick.AddListener(() => { gameStatisticsPanel.InitializePanel(game.Game_ID); }); 
        statisticsButton.gameObject.SetActive(true);
        statisticsButton.onClick.RemoveAllListeners();
        statisticsButton.onClick.AddListener(() =>
        {
            Debug.Log("Statistics button clicked");
            gameStatisticsPanel.InitializePanel(game.Game_ID);
        });
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
        await SupabaseManager.Instance.PublishGame(game);
    }
    private void ShowError()
    {
        errorMessage.gameObject.SetActive(true);
        errorMessage.GetComponent<Animator>().SetTrigger("ErrorTrigger");
    }
    public async void PlayGame()
    {
        if (game.Is_Published)
        {
            PublishedGames pubGame = await SupabaseManager.Instance.GetPublishedGame(game.Game_ID);
            if (pubGame.Updated_At == null)
            {
                Debug.Log("PlayGame called with ID: " + game.Game_ID);
                await SupabaseManager.Instance.LogGameLogin(UserManager.Instance.userId, game.Game_ID);
                SceneLoader.Instance.LoadGameScene(game.Game_ID.ToString());
            }
            else
            {
                // view an error message (game is being updated)
            }
        }
        else
        {
            ShowError();
        }
    }
}
