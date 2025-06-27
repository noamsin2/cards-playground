using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Models;
using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using static UnityEngine.GraphicsBuffer;

public class MatchManager : MonoBehaviour
{
    private string validateMoveUrl = "https://oaxeurahthkghramlwsf.functions.supabase.co/validate-move"; // Replace with your actual project reference URL
    public Match CurrentMatch { get; private set; }
    public static MatchManager Instance { get; private set; }
    [SerializeField] private EndMatchPanel endMatchPanel;
    [SerializeField] private MulliganPanel mulliganPanel;
    [SerializeField] private GameObject matchPanel;
    [SerializeField] private PlayPanel playPanel;
    [SerializeField] private GamePanel gamePanel;
    [SerializeField] private OptionsPanel optionsPanel;
    [SerializeField] private GameMenuPanel gameMenuPanel;
    [SerializeField] private TurnTimer turnTimer;

    private event Action OnDeckEmpty;
    private event Action OnDamagePlayer;
    
    private MatchPlayers player;
    private MatchPlayers opponent;
    private int maxHandSize;
    private string limitHandSize;
    private List<int> opponentCardsPlayed;
    private List<int> playerCardsPlayed;
    public List<int> hand {  get; private set; }
    private List<int> deck;
    public int opponentCardsCount {  get; private set; }
    public int opponentDeckCardsCount {  get; private set; }
    private int activePlayer;
    public bool isPlayingCard = false;
    public int maxHealth { get; private set; } = 0;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    private void Start()
    {
        
        SupabaseManager.Instance.onOpponentUpdate += UpdateOpponent;
        optionsPanel.OnSurrender += Surrender;
        turnTimer.OnTimerExpired += EndTurn;
        SupabaseManager.Instance.OnChangeTurn += OpponentEndedTurn;
       
    }
    public async void StartGame(Match match)
    {
        //var channelManager = FindFirstObjectByType<RealTimeChannelManager>();

        InitializeGameSettings();
        CurrentMatch = match;
        var playersResult = await SupabaseManager.Instance.GetMatchPlayers(match.Match_ID);
        playPanel.MatchFound();
        Invoke("TriggerFadeInMatch", 2f);
        player = playersResult.FirstOrDefault(mp => mp.User_ID == UserManager.Instance.userId);
        InitializeOpponent(playersResult.FirstOrDefault(mp => mp.User_ID != UserManager.Instance.userId));
        //await SupabaseManager.Instance.SubscribeToGameState(match.Match_ID, UserManager.Instance.userId);
        await SupabaseManager.Instance.SubscribeToOpponent(match.Match_ID, opponent.Player_Index);
        int myIndex = player?.Player_Index ?? -1;
        
        Debug.Log("Found Match");
        BeginMulliganPhase(myIndex);
    }
    private void InitializeGameSettings()
    {
        var settings = GameManager.Instance.settings;
        maxHandSize = int.Parse(settings.max_hand_size);
        limitHandSize = settings.limit_hand_size;
        OnDamagePlayer = null;
        OnDeckEmpty = null;
        if (settings.cards_win_condition)
            OnDeckEmpty += Lose;
        if (settings.health_win_condition)
        {
            maxHealth = int.Parse(settings.player_health);
            OnDamagePlayer += CheckHealth;

        }
    }
    private void InitializeOpponent(MatchPlayers opp)
    {

        opponent = opp;
        opponent.Current_Cards_In_Hand = new int[0];
        opponent.Current_Cards_In_Deck = new int[0];
        opponentCardsCount = 0;
        opponentDeckCardsCount = 0;
        opponent.Cards_Played = new int[0];
    }
    private void TriggerFadeInMatch()
    {
        matchPanel.SetActive(true);
        matchPanel.GetComponent<Animator>().SetTrigger("FadeIn_Trigger");
    }
    public void BeginMulliganPhase(int myIndex)
    {
        List<int> shuffledDeck = DecksManager.Instance.currentDeck.ShuffleDeck();
        mulliganPanel.ShowMulligan(shuffledDeck, myIndex); // reference via inspector
    }

    // This method will be used to validate moves.
    public IEnumerator ValidateMove(string matchId, string userId, string moveDataJson)
    {
        // Prepare the data to send to the Supabase Edge Function.
        var json = JsonUtility.ToJson(new
        {
            match_id = matchId,
            user_id = userId,
            move = moveDataJson
        });

        // Create a UnityWebRequest for sending the POST request.
        UnityWebRequest request = new UnityWebRequest(validateMoveUrl, "POST");

        // Convert the data to byte array.
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        // Set the request handlers (upload and download).
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        // Set the request headers.
        request.SetRequestHeader("Content-Type", "application/json");

        // Send the request and wait for a response.
        yield return request.SendWebRequest();

        // Handle the result of the request.
        if (request.result == UnityWebRequest.Result.Success)
        {
            // Log the successful validation.
            Debug.Log("Move validated: " + request.downloadHandler.text);
        }
        else
        {
            // Log any errors that occurred.
            Debug.LogError("Validation failed: " + request.error);
        }
    }

    // You can call this method when a player makes a move in your game:
    public void OnPlayerMove(string matchId, string userId, string moveDataJson)
    {
        // Start the validation coroutine.
        StartCoroutine(ValidateMove(matchId, userId, moveDataJson));
    }
    public async void ProceedToGame(List<int> deck, List<int> hand, int activePlayerIndex)
    {
        Debug.Log("proceed to game");
        this.deck = deck;
        this.hand = hand;
        InitializePlayer();
        await SupabaseManager.Instance.UpdatePlayerInMatch(player);
        await SupabaseManager.Instance.SubscribeToGameState(CurrentMatch.Match_ID);
        activePlayer = activePlayerIndex;
        StartTurn();

    }
    private void InitializePlayer()
    {
        player.Current_Cards_In_Deck = deck.ToArray();
        player.Current_Cards_In_Hand = hand.ToArray();
        player.HasMulliganed = true;
        player.Cards_Played = new int[0];
        if(maxHealth != 0)
            player.Current_Health = int.Parse(GameManager.Instance.settings.player_health);

    }
    private void Surrender()
    {
        if (CurrentMatch != null)
        {
            Lose();
        }
    }
    private async void Lose()
    {
        
        await SupabaseManager.Instance.UpdatePlayerWinLoseCount(UserManager.Instance.userId, GameManager.Instance.game_id,false);

        // show lose screen
        endMatchPanel.OnPlayerContinued += ResetUI;
        endMatchPanel.OnMatchEnded(false);
        EndMatch();
    }
    private async void Win()
    {
        int gameId = GameManager.Instance.game_id;
        await SupabaseManager.Instance.UpdatePlayerWinLoseCount(UserManager.Instance.userId, gameId, true);
        await SupabaseManager.Instance.IncreaseGamesPlayed(gameId);
        // Wait 5 seconds because it takes 5 seconds for the card to disappear
        await Task.Delay(4500);
        // show win screen
        endMatchPanel.OnPlayerContinued += ResetUI;
        endMatchPanel.OnMatchEnded(true);

        EndMatch();
    }
    private void ResetUI()
    {
        endMatchPanel.OnPlayerContinued -= ResetUI;
        gamePanel.gameObject.SetActive(false);
        matchPanel.SetActive(false);
        playPanel.gameObject.SetActive(false);
        gameMenuPanel.gameObject.SetActive(true);
    }
    private void EndMatch()
    {
        // delete match from database

        var channelManager = FindFirstObjectByType<RealTimeChannelManager>();
        channelManager.UnsubscribeFromChannel("matches");
        channelManager.UnsubscribeFromChannel("match_players");
        CurrentMatch = null;
        player = null;
        opponent = null;
        hand = null;
        deck = null;
    }
    private void StartTurn()
    {
        turnTimer.ResetTimer();
        turnTimer.StartTimer();
        gamePanel.endTurnButton.isInteractable = (activePlayer == player.Player_Index) ? true : false;
        if (activePlayer != player.Player_Index)
        {
            OpponentDrawCard();
            return;
        }
        StartCoroutine(gamePanel.ShowYourTurnVisual());
        DrawCardFromDeck();

    }
    public void EndTurn()
    {
        if (activePlayer == player.Player_Index)
        {
            activePlayer = (activePlayer == 1) ? 0 : 1; // Switch to the other player
            SupabaseManager.Instance.ChangeTurnInMatch(CurrentMatch.Match_ID, activePlayer);
            
            StartTurn();
        } 
    }

    private void OpponentEndedTurn(int activePlayerIndex)
    {
        if (activePlayerIndex == player.Player_Index)
        {
            activePlayer = (activePlayer == 1) ? 0 : 1; // Switch to the other player
            
            StartTurn();
        }
    }

    private async void UpdateOpponent(MatchPlayers updatedOpponent)
    {
        if (opponent == null) { return; }
        Debug.Log("OPPONENT INDEX: " + updatedOpponent.Player_Index);
        if (updatedOpponent == null)
            Debug.Log("UPDATED OPPONEENT NULL");
        if (updatedOpponent.Current_Cards_In_Hand == null)
            return;
        int maxRetries = 20; // Prevent infinite loops
        int retryCount = 0;

        bool changedFlag = false;
        int newHandCardCount = opponentCardsCount;
        int newDeckCardCount = opponentDeckCardsCount;
        int cardsPlayedCount = opponent.Cards_Played.Count();
        int newHealth = opponent.Current_Health;
        Debug.Log("BEFORE WHILE");
        while (!changedFlag && retryCount < maxRetries) {
            Debug.Log("WHILE");
            //Debug.Log($"Comparing Hand Cards: Old={string.Join(",", opponent.Current_Cards_In_Hand)} vs New={string.Join(",", updatedOpponent.Current_Cards_In_Hand)}");
            //Debug.Log($"Comparing Deck Cards: Old={string.Join(",", opponent.Current_Cards_In_Deck)} vs New={string.Join(",", updatedOpponent.Current_Cards_In_Deck)}");
            //Debug.Log($"Comparing Health: Old={opponent.Current_Health} vs New={updatedOpponent.Current_Health}");
            if ((updatedOpponent.Current_Cards_In_Hand != null && !opponent.Current_Cards_In_Hand.SequenceEqual(updatedOpponent.Current_Cards_In_Hand)) ||
                (updatedOpponent.Current_Cards_In_Deck != null && !opponent.Current_Cards_In_Deck.SequenceEqual(updatedOpponent.Current_Cards_In_Deck)) ||
                (opponent.Current_Health != updatedOpponent.Current_Health) ||
                (updatedOpponent.Cards_Played != null && cardsPlayedCount != updatedOpponent.Cards_Played.Count()))
            {
                Debug.Log("🔄 Change Detected!");
                changedFlag = true;
                newHealth = updatedOpponent.Current_Health;
                newHandCardCount = updatedOpponent.Current_Cards_In_Hand.Length;
                newDeckCardCount = updatedOpponent.Current_Cards_In_Deck.Length;
                cardsPlayedCount = updatedOpponent.Cards_Played.Count();
            }
            else
            {
                Debug.Log("Delay 500");
                await Task.Delay(500);
                retryCount++;

            }
        }
        Debug.Log("AFTER WHILE");
        if (!changedFlag)
        {
            Debug.LogError("⏳ Data did not change after max retries.");
            return;
        }

        foreach (var id in updatedOpponent.Current_Cards_In_Hand)
            Debug.Log(id);

        if (opponentCardsCount != newHandCardCount)
        {
            opponent.Current_Cards_In_Hand = updatedOpponent.Current_Cards_In_Hand.ToArray(); // Creates a new independent array
            opponentCardsCount = newHandCardCount;
            if (newDeckCardCount != opponentDeckCardsCount - 1 )
            {
                gamePanel.ShowCardsForOpponent();
                //OpponentDrawCard();
            }
        }
        if (opponentDeckCardsCount != newDeckCardCount)
        {
            opponentDeckCardsCount = newDeckCardCount;
            //update opponent's deck on UI
        }
        if(GameManager.Instance.settings.health_win_condition == true &&  opponent.Current_Health != updatedOpponent.Current_Health)
        {
            opponent.Current_Health = newHealth;
            //update opponent's health on UI
        }
        if (cardsPlayedCount != opponent.Cards_Played.Count())
        {
            var opponentCardCounts = opponent.Cards_Played.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());
            var updatedCardCounts = updatedOpponent.Cards_Played.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());

            var differences = new List<int>();


            foreach (var kvp in updatedCardCounts)
            {
                int cardID = kvp.Key;
                int updatedCount = kvp.Value;
                int previousCount = opponentCardCounts.ContainsKey(cardID) ? opponentCardCounts[cardID] : 0;

                // If more instances exist in updatedOpponent.Cards_Played, add the extra ones
                for (int i = 0; i < updatedCount - previousCount; i++)
                {
                    differences.Add(cardID);
                }
            }
            if (differences.Count() > 0)
            {
                isPlayingCard = true;
                //show card played UI
                StartCoroutine(gamePanel.ShowPlayedCardsWithDelay(differences, true));
                opponent.Cards_Played = updatedOpponent.Cards_Played.ToArray();
            }
        }
    }

    public int GetPlayerCardCount()
    {
        return player.Current_Cards_In_Hand.Count();
    }
    public int GetOpponentDeckCount()
    {
        return opponentDeckCardsCount;
    }
    public int GetPlayerDeckCount()
    {
        return player.Current_Cards_In_Deck.Count();
    }
    public int GetOpponentCardCount()
    {
        return opponentCardsCount;
    }
    public void DamagePlayer(int healthDecrease,string target)
    {
        if (target == "player")
            player.Current_Health -= healthDecrease;
        else if (target == "opponent")
            opponent.Current_Health -= healthDecrease;
        OnDamagePlayer?.Invoke();
    }
    private void CheckHealth()
    {
        gamePanel.UpdatePlayerHealth(player.Current_Health);
        gamePanel.UpdateOpponentHealth(opponent.Current_Health);
        if (player.Current_Health <= 0)
        {
            Lose();
        }
        else if(opponent.Current_Health <= 0)
        {
            Win();
        }
    }
    public void HealPlayer(int healthIncrease, string target)
    {
        if (target == "player")
            player.Current_Health += healthIncrease;
        else if (target == "opponent")
            opponent.Current_Health += healthIncrease;
    }
    public void DrawCards(int amountToDraw)
    {
        for (int i = 0; i < amountToDraw; i++)
        {
            DrawCardFromDeck(); // Assume this removes from deck & returns the card
        }
    }
    private async void DrawCardFromDeck()
    {
        if (player.Current_Cards_In_Deck.Length == 0)
        {
            Debug.LogWarning("⚠️ No cards left in the deck!");
            OnDeckEmpty?.Invoke();
            return; // Indicating no cards to draw
        }

        int drawnCard = player.Current_Cards_In_Deck[0]; // Get the first card
        int[] newDeck = new int[player.Current_Cards_In_Deck.Length - 1];
        Array.Copy(player.Current_Cards_In_Deck, 1, newDeck, 0, newDeck.Length); // Shift elements left

        player.Current_Cards_In_Deck = newDeck; // Update the reference to new deck
        Debug.Log($"🃏 Card {drawnCard} drawn from the deck. Remaining cards: {player.Current_Cards_In_Deck.Length}");

        AddCardToHand(drawnCard);
        gamePanel.PlayerDrawCardUI(drawnCard);
        await SupabaseManager.Instance.UpdatePlayerInMatch(player);
    }
    public void OpponentDrawCard()
    {
        gamePanel.OpponentDrawCardUI();
    }
    private void AddCardToHand(int drawnCard)
    {
        int handSize = GetPlayerCardCount();
        int parsedLimit;

        if (handSize >= maxHandSize)
        {
            if (limitHandSize == "No" || int.TryParse(limitHandSize, out parsedLimit) && parsedLimit <= handSize)
            {
                Debug.Log($"Hand exceeds max size! Discarding {drawnCard} drawn cards.");
                return;
            }
        }
        AddNumberToHandArray(drawnCard);
    }
    private void AddNumberToHandArray(int drawnCard)
    {
        int[] newHand = new int[player.Current_Cards_In_Hand.Length + 1];

        // Copy existing cards into the new array
        Array.Copy(player.Current_Cards_In_Hand, newHand, player.Current_Cards_In_Hand.Length);

        // Add the drawn card to the last slot
        newHand[newHand.Length - 1] = drawnCard;

        // Update the player's hand reference
        player.Current_Cards_In_Hand = newHand;

        Debug.Log($"🃏 Card {drawnCard} added to hand. Hand size: {newHand.Length}");

    }
    public async void PlayCard(int cardID, GameObject cardItem)
    {
        if (isPlayingCard)
            return;
        if (isMyTurn() && player.Current_Cards_In_Hand.Contains(cardID))
        {
            isPlayingCard = true;
            

            // Properly remove the played card from hand
            List<int> tempHand = player.Current_Cards_In_Hand.ToList(); // Convert array to list
            tempHand.Remove(cardID); // Removes only the first occurrence
            player.Current_Cards_In_Hand = tempHand.ToArray(); // Convert back to array
            // Append played card to the played cards list
            player.Cards_Played = player.Cards_Played.Append(cardID).ToArray();
            await SupabaseManager.Instance.UpdatePlayerInMatch(player);

            Destroy(cardItem);
            StartCoroutine(gamePanel.ShowPlayedCardsWithDelay(new List<int> { cardID }, false));
            CardsManager.Instance.PlayCardEffect(cardID, false);
            
        }
    }
    private bool isMyTurn()
    {
        return activePlayer == player.Player_Index;
    }
}
