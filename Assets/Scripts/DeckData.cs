using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System;
using System.Linq;

[System.Serializable]
public class DeckData
{
    public string deckName;
    public int deckID;
    public Dictionary<int, int> cardsCount; // Dictionary of <ID, cardCount>
    public int maxDeckSize;
    public int cardsInDeck;
    
    public DeckData(string name, int maxDeckSize)
    {
        deckName = name;
        deckID = 0;
        this.maxDeckSize = maxDeckSize;
        cardsInDeck = 0;
        cardsCount = new Dictionary<int, int>();
    }
    public string AddCardToDeck(int cardID)
    {
        if (cardsInDeck < maxDeckSize)
        {
            if (cardsCount.ContainsKey(cardID) && cardsCount[cardID] < DecksManager.Instance.cardCopies)
                cardsCount[cardID]++;
            else if (!cardsCount.ContainsKey(cardID))
                cardsCount[cardID] = 1;
            else
            {
                Debug.Log("card reached max copies");
                return "Card reached max copies, cannot add more of the same card";
            }
            cardsInDeck++;
            Debug.Log($"Card {cardID} added to deck.");
        }
        else
        {
            Debug.Log("DECK IS FULL");
            return "Deck is full, cannot add more cards";
        }
        return "";
    }
    public void RemoveCardFromDeck(int cardID)
    {
        if (cardsCount.ContainsKey(cardID))
            cardsCount[cardID]--;
        if (cardsCount[cardID] == 0)
            cardsCount.Remove(cardID);
        cardsInDeck--;
        Debug.Log($"Card {cardID} removed from deck.");
    }
    public List<int> ShuffleDeck()
    {
        List<int> allCards = new List<int>();

        // Flatten the deck into a list of card IDs
        foreach (var entry in cardsCount)
        {
            for (int i = 0; i < entry.Value; i++)
            {
                allCards.Add(entry.Key);
            }
        }

        // Shuffle using Fisher-Yates
        System.Random rng = new System.Random();
        int n = allCards.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            int temp = allCards[k];
            allCards[k] = allCards[n];
            allCards[n] = temp;
        }

        return allCards;
    }
    

}
