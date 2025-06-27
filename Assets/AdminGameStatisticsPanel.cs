using TMPro;
using UnityEngine;

public class AdminGameStatisticsPanel : MonoBehaviour
{
    private long? totalGames;
    private long? totalPlayers;
    private long? totalGameCreators;
    private long? totalUsers;
    private long? totalDecks;
    private long? totalCards;

    [SerializeField] private TMP_Text totalGamesText;
    [SerializeField] private TMP_Text totalPlayersText;
    [SerializeField] private TMP_Text totalCreatorsText;
    [SerializeField] private TMP_Text totalUsersText;
    [SerializeField] private TMP_Text totalDecksText;
    [SerializeField] private TMP_Text totalCardsText;

    public async void InitializePanel()
    {
        gameObject.SetActive(true);
        if (UserManager.Instance.isAdmin == false)
            return;
        totalGames = await SupabaseManager.Instance.GetCardGamesCount();
        totalGamesText.text = totalGames.ToString();

        totalPlayers = await SupabaseManager.Instance.GetTotalPlayers();
        totalPlayersText.text = totalPlayers.ToString();

        totalGameCreators = await SupabaseManager.Instance.GetTotalGameCreators();
        totalCreatorsText.text = totalGameCreators.ToString();

        totalUsers = await SupabaseManager.Instance.GetTotalUsers();
        totalUsersText.text = totalUsers.ToString();

        totalDecks = await SupabaseManager.Instance.GetTotalDecks();
        totalDecksText.text = totalDecks.ToString();

        totalCards = await SupabaseManager.Instance.GetTotalCards();
        totalCardsText.text = totalCards.ToString();
    }
}
