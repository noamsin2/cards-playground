using Models;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using TMPro;

public class GameStatisticsPanel : MonoBehaviour
{
    private int? matchesPlayed;
    private int totalLogins = 0;
    private int totalDecks = 0;
    private int largestWinCount = 0;
    private int? gameId;
    [SerializeField] private TMP_Text matchesPlayedText;
    [SerializeField] private TMP_Text totalLoginsText;
    [SerializeField] private TMP_Text totalDecksText;
    [SerializeField] private TMP_Text largestWinCountText;

   
    public async void InitializePanel(int gameId)
    {
        if (this.gameId == null || this.gameId != null && this.gameId != gameId)
        {
            this.gameId = gameId;
           
            List<GamePlayers> gamePlayers = await SupabaseManager.Instance.GetGamePlayersByGameID(gameId);

            largestWinCount = GetHighestWinCount(gamePlayers);

            totalLogins = CountPlayersLoggedInYesterday(gamePlayers);

            matchesPlayed = await SupabaseManager.Instance.GetGamesPlayed(gameId);

            totalDecks = await SupabaseManager.Instance.CountDecksByGameId(gameId);


            if (matchesPlayed != null)
            {
                matchesPlayedText.text = matchesPlayed.ToString();
            }
            else
                matchesPlayedText.text = "0";

            totalLoginsText.text = totalLogins.ToString();
            totalDecksText.text = totalDecks.ToString();
            largestWinCountText.text = largestWinCount.ToString();
        }
        gameObject.SetActive(true);
    }

    public int CountPlayersLoggedInYesterday(List<GamePlayers> players)
    {
        if (players == null || players.Count == 0)
            return 0;

        DateTime todayUtc = DateTime.UtcNow.Date;
        DateTime yesterdayStart = todayUtc.AddDays(-1);
        DateTime yesterdayEnd = todayUtc;

        return players.Count(p =>
            p.Last_Logged_In.HasValue &&
            p.Last_Logged_In.Value >= yesterdayStart &&
            p.Last_Logged_In.Value < yesterdayEnd
        );
    }
    public int GetHighestWinCount(List<GamePlayers> players)
    {
        if (players == null || players.Count == 0)
            return 0;

        return players.Max(p => p.Win_Count);
    }

}
