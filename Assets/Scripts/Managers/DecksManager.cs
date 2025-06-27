using System;
using System.Collections.Generic;
using System.Linq;
using Models;
using UnityEngine;

public class DecksManager : MonoBehaviour
{
    public int MAX_DECKS = 5;
    public static DecksManager Instance;
    public List<DeckData> decks {  get; private set; }
    public DeckData currentDeck;
    public int cardCopies;
    public int maxDeckSize;
    public event Action OnDecksLoaded;
    public delegate void CardEventHandler(int? cardID, bool isCalledFromDeck);
    public event CardEventHandler OnCardAdded;
    public delegate void ErrorEventHandler(string error);
    public event ErrorEventHandler OnCardError;
    [SerializeField] private MessagePanel messagePanel;
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }
    private void Start()
    {
        
    }
    private void OnEnable()
    {
        GameManager.Instance.OnGameLoaded += InitializeDecksManager;
    }
    private void InitializeDecksManager()
    {
        cardCopies = int.Parse(GameManager.Instance.settings.card_copies);
        maxDeckSize = int.Parse(GameManager.Instance.settings.deck_size);
        GameManager.Instance.OnGameLoaded -= InitializeDecksManager;
        decks = new List<DeckData>();
        GetDecksFromDB();
        OnDecksLoaded?.Invoke();
        currentDeck = null;
    }
    private async void GetDecksFromDB()
    {
        List<string> affectedDeckNames = new List<string>();
     
        int user_id = UserManager.Instance.userId;
        int game_id = GameManager.Instance.game_id;
        Debug.Log($"user id: {user_id}, game_id: {game_id}");
        List<Decks> decks_DB = await SupabaseManager.Instance.GetDecksFromDB(user_id, game_id);
        List<PublishedCards> cardsInGame = await SupabaseManager.Instance.GetAllCardsInGame(game_id);
        var validCardIds = new HashSet<int>(cardsInGame.Select(card => card.Card_ID));
        Debug.Log(decks_DB.Count);
        foreach(Decks deck_db in decks_DB)
        {
            DeckData newDeck = new DeckData(deck_db.Name, maxDeckSize);
            newDeck.deckID = deck_db.Deck_ID;
            newDeck.maxDeckSize = maxDeckSize;
            bool cardRemoved = false;
            List<int> filteredCardIds = new List<int>();
            foreach (int id in deck_db.Card_IDs)
            {
                if (validCardIds.Contains(id))
                {
                    filteredCardIds.Add(id);
                    newDeck.AddCardToDeck(id);
                }
                else
                {
                    cardRemoved = true;
                }
            }
            if (cardRemoved)
            {
                deck_db.Card_IDs = filteredCardIds.ToArray();
                affectedDeckNames.Add(deck_db.Name);
                await SupabaseManager.Instance.UpdateDeckInDatabase(deck_db);
            }

            decks.Add(newDeck);
        }
        if (affectedDeckNames.Count > 0)
        {
            string deletedMessage = "Some cards were removed from the game and have been taken out of the following decks: ";
            deletedMessage += string.Join(", ", affectedDeckNames);
            Debug.Log(deletedMessage);
            messagePanel.ShowMessage(deletedMessage);
        }
    }
    public void AddCardToDeck(int cardID)
    {
        if (currentDeck != null)
        {
            string message = currentDeck.AddCardToDeck(cardID);
            if (message == "")
                OnCardAdded?.Invoke(cardID,false);
            else
                OnCardError?.Invoke(message);
            
        }
    }
    public void RemoveCardFromDeck(int cardID)
    {
        if (currentDeck != null)
        {
            currentDeck.RemoveCardFromDeck(cardID);
        }
    }
    public DeckData CreateNewDeck()
    {

        if(decks.Count < MAX_DECKS)
        {
            DeckData newDeck = new DeckData("New Deck", maxDeckSize);
            decks.Add(newDeck);
            AddDeckToDB(newDeck);
            Debug.Log("New deck added");
            return newDeck;
        }
        else
        {
            Debug.Log("Reached max deck count");
            return null;
        }
    }

    public void RemoveDeck(DeckData deckToRemove)
    {
        if (decks.Contains(deckToRemove))
        {
            SupabaseManager.Instance.DeleteDeckFromDB(deckToRemove);
            decks.Remove(deckToRemove);
            Debug.Log($"Deck {deckToRemove.deckName} removed");
        }
    }

    public async void AddDeckToDB(DeckData newDeck)
    {
        Decks deck = new Decks();
        deck.FK_Game_ID = GameManager.Instance.game_id;
        deck.FK_User_ID = UserManager.Instance.userId;
        deck.Name = newDeck.deckName;
        List<int> cardIdsList = new List<int>();
        foreach (KeyValuePair<int, int> kvp in newDeck.cardsCount)
        {
            Debug.Log($"Key: {kvp.Key}, Value: {kvp.Value}");
            for(int i = 0; i < kvp.Value; i++)
                cardIdsList.Add(kvp.Key);
        }
        deck.Card_IDs = cardIdsList.ToArray();
        Decks DBDeck = await SupabaseManager.Instance.AddDeckToDatabase(deck);
        newDeck.deckID = DBDeck.Deck_ID;
    }

    public async void UpdateDeckInDB(DeckData newDeck)
    {
        Decks deck = new Decks();
        deck.FK_Game_ID = GameManager.Instance.game_id;
        deck.FK_User_ID = UserManager.Instance.userId;
        deck.Name = newDeck.deckName;
        deck.Deck_ID = newDeck.deckID;
        List<int> cardIdsList = new List<int>();
        foreach (KeyValuePair<int, int> kvp in newDeck.cardsCount)
        {
            Debug.Log($"Key: {kvp.Key}, Value: {kvp.Value}");
            for (int i = 0; i < kvp.Value; i++)
                cardIdsList.Add(kvp.Key);
        }
        deck.Card_IDs = cardIdsList.ToArray();
        await SupabaseManager.Instance.UpdateDeckInDatabase(deck);
    }
    public void SaveDeck()
    {
        if(currentDeck != null)
        {
            UpdateDeckInDB(currentDeck);
            currentDeck = null;
        }

    }
}

