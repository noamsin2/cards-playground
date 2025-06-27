using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Models;
using UnityEngine;
using UnityEngine.Networking;

public class CardsManager : MonoBehaviour
{
    public static CardsManager Instance { get; private set; }

    [SerializeField] public Dictionary<int, CardData> allCards;  // Holds all cards
    public event Action OnCardsLoaded;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    //private async void Start()
    //{
    //    await LoadAllCards(PlayerPrefs.GetString("SelectedGameID"));
    //}
    public async Task LoadAllCards(string gameId)
    {
        allCards = new Dictionary<int, CardData>();
        List<PublishedCards> cards = await SupabaseManager.Instance.GetAllCardsInGame(int.Parse(gameId));

        foreach (var card in cards)
        {
            List<PublishedCardEffects> effects = await SupabaseManager.Instance.GetAllCardEffects(card.Card_ID);
            List<CardEffect> noramlizedEffect = NoramlizeCardEffects(effects);
            string description = MakeCardDescription(noramlizedEffect);
            var tmpCard = new CardData(card.Name, description, null, noramlizedEffect);
            allCards[card.Card_ID] = tmpCard;
            int cardIndex = allCards.Count - 1;
            StartCoroutine(LoadImage(card.Image_URL, card.Card_ID));
        }

        await Task.Yield(); // Let all async operations finish
        OnCardsLoaded?.Invoke();
    }
    private List<CardEffect> NoramlizeCardEffects(List<PublishedCardEffects> effects)
    {
        List<CardEffect> normalizedEffects = new List<CardEffect>();
        foreach (var effect in effects)
        {
            CardEffect newEffect = new CardEffect
            {
                action = effect.Action,
                effect = effect.Effect,
                value = effect.X
            };
            normalizedEffects.Add(newEffect);
        }
        return normalizedEffects;
    }
    private string MakeCardDescription(List<CardEffect> effects)
    {
        // Dictionary to store actions and their corresponding effects
        Dictionary<string, List<string>> actionEffectsMap = new Dictionary<string, List<string>>();

        // Iterate over all card effects and group them by action
        foreach (var effect in effects)
        {
            if (!actionEffectsMap.ContainsKey(effect.action))
            {
                actionEffectsMap[effect.action] = new List<string>();
            }

            // Add the effect to the corresponding action
            actionEffectsMap[effect.action].Add(effect.effect.Replace("X", effect.value.ToString()));
        }

        // Now build the description string
        List<string> descriptionParts = new List<string>();

        foreach (var action in actionEffectsMap)
        {
            // Combine all effects for this action
            string combinedEffects = string.Join(" and ", action.Value);

            // Add the action with its combined effects to the description
            descriptionParts.Add($"{action.Key}: {combinedEffects}");
        }

        // Join all parts into one final description
        return string.Join("\n", descriptionParts);
    }

    private IEnumerator LoadImage(string url, int cardId)
    {
        //Debug.Log($"LoadImage started for card id {cardId}, URL: {url}");
        if (string.IsNullOrEmpty(url)) yield break;

        url = SupabaseManager.Instance.GetCardUrl(SupabaseManager.PUBLISHED_CARD_BUCKET ,url);
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            Sprite newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            // add the image to the last item on the list because we just added it
            allCards[cardId].cardImage = newSprite;
        }
        else
        {
            Debug.LogError($"Failed to load image: {url}");
        }
    }
    public string GetCardNameByID(int cardID)
    {
        return allCards[cardID].cardName;
    }
    public string GetCardDescriptionByID(int cardID)
    {
        return allCards[cardID].cardDescription;
    }
    public Sprite GetCardImageByID(int cardID)
    {
        return allCards[cardID].cardImage;
    }
    public List<CardEffect> GetCardEffectsByID(int cardID)
    {
        return allCards[cardID].effects;
    }
    public void PlayCardEffect(int cardID,bool isOpponent)
    {
        CardEffectProcessor effectProcessor = FindFirstObjectByType<CardEffectProcessor>();
        var card = allCards[cardID];
        Debug.Log(isOpponent);
        foreach (CardEffect effect in card.effects)
        {
            if (effect.action == "On Play")
            {
                effectProcessor.ApplyEffect(effect, isOpponent);
            }
        }

        Debug.Log($"?? {card.cardName} played!");
    }

}
