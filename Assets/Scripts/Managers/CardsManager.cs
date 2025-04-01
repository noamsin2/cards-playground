using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Models;
using UnityEngine;
using UnityEngine.Networking;

public class CardsManager : MonoBehaviour
{
    public static CardsManager Instance { get; private set; }

    public List<CardData> allCards;  // Holds all cards

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
        allCards = new List<CardData>();
        List<Cards> cards = await SupabaseManager.Instance.GetAllCardsInGame(int.Parse(gameId));

        foreach (var card in cards)
        {
            List<CardEffects> effects = await SupabaseManager.Instance.GetAllCardEffects(card.Card_ID);
            List<CardEffect> noramlizedEffect = NoramlizeCardEffects(effects);
            string description = MakeCardDescription(noramlizedEffect);
            var tmpCard = new CardData(card.Name, description, null, noramlizedEffect);
            allCards.Add(tmpCard);
            int cardIndex = allCards.Count - 1;
            Debug.Log($"Starting Coroutine for Card: {card.Name}, URL: {card.Image_URL}");
            StartCoroutine(LoadImage(card.Image_URL, cardIndex));
            Debug.Log($"Ending Coroutine for Card: {card.Name}, URL: {card.Image_URL}");
        }
    }
    private List<CardEffect> NoramlizeCardEffects(List<CardEffects> effects)
    {
        List<CardEffect> normalizedEffects = new List<CardEffect>();
        foreach (var effect in effects)
        {
            CardEffect newEffect = new CardEffect
            {
                action = effect.Action,
                effect = effect.Effect
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
            actionEffectsMap[effect.action].Add(effect.effect);
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

    private IEnumerator LoadImage(string url, int cardIndex)
    {
        Debug.Log($"LoadImage started for card index {cardIndex}, URL: {url}");
        if (string.IsNullOrEmpty(url)) yield break;

        url = SupabaseManager.Instance.GetCardUrl(url);
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            Sprite newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            // add the image to the last item on the list because we just added it
            allCards[cardIndex].cardImage = newSprite;
        }
        else
        {
            Debug.LogError($"Failed to load image: {url}");
        }
    }
}
