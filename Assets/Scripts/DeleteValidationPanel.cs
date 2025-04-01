using Models;
using TMPro;
using UnityEngine;

public class DeleteValidationPanel : MonoBehaviour
{
    private Cards card;
    private CardGames cardGame;
    [SerializeField] new private TextMeshProUGUI name;
    [SerializeField] private MyCardsPanel myCardsPanel;
    [SerializeField] private MyGamesPanel myGamesPanel;
    [SerializeField] private GameObject loadingCircle;
    public void InitializePanel(Cards card)
    {
        this.card = card;
        name.text += card.Name + "?";
        gameObject.SetActive(true);
    }
    
    public void InitializePanel(CardGames cardGame)
    {
        this.cardGame = cardGame;
        name.text += cardGame.Name + "?";
        gameObject.SetActive(true);
    }
    public async void DeleteCard()
    {
        if (card != null)
        {
            await SupabaseManager.Instance.DeleteCard(card);
            myCardsPanel.DisplayCards();
            card = null;
            name.text = "Delete ";
        }
    }
    public void OnNoClick()
    {
        card = null;
        cardGame = null;
        name.text = "Delete ";
    }
    public async void DeleteGame()
    {
        if (cardGame != null)
        {
            // change isDeleted to true in database
            await SupabaseManager.Instance.SetGameToDeleted(cardGame);
            // refresh the game list
            myGamesPanel.DisplayGames();
            gameObject.SetActive(false);
            loadingCircle.SetActive(false);
            cardGame = null;
            name.text = "Delete ";
        }
    }
}
