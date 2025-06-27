using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Models;
using Newtonsoft.Json;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class MulliganPanel : MonoBehaviour
{
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private GamePanel gamePanel;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private Button confirmButton;
    [SerializeField] private List<int> initialHand;
    [SerializeField] private List<int> deck;
    [SerializeField] private HashSet<int> selectedForReplacement = new();
    private CardsManager cardManager;
    private CanvasGroup mulliganCanvasGroup;
    private void Awake()
    {
        mulliganCanvasGroup = GetComponent<CanvasGroup>();
        if (mulliganCanvasGroup == null)
            Debug.LogError("CanvasGroup missing from MulliganPanel!");
    }
    public void ShowMulligan(List<int> fullDeck, int myIndex)
    {
        SupabaseManager.Instance.OnGameStart += HandleMulligan;
        int initialHandSize = int.Parse(GameManager.Instance.settings.initial_hand_size);
       
        // Extract initial hand from deck
        initialHand = fullDeck.Take(initialHandSize).ToList();
        fullDeck.RemoveRange(0, initialHandSize); // modify the deck
  
        selectedForReplacement.Clear();
        Debug.Log($"GameObject active: {gameObject.activeInHierarchy}");
        Debug.Log($"Script enabled: {enabled}");
        Debug.Log($"CanvasGroup: {mulliganCanvasGroup}");
        Invoke("TriggerFadeIn", 2f);

        // Clear any existing cards
        foreach (Transform child in cardContainer)
            Destroy(child.gameObject);

        cardManager = CardsManager.Instance;
        // Create card UI
        for (int i = 0; i < initialHand.Count; i++)
        {
            int index = i; // Capture the index for the closure
            int cardId = initialHand[i];
            GameObject card = Instantiate(cardPrefab, cardContainer);
            
            var name = card.transform.Find("Name Background/Card Name")?.GetComponent<TMP_Text>();
            if (name != null) name.text = cardManager.GetCardNameByID(cardId);

            var description = card.transform.Find("Description Background/Card Description")?.GetComponent<TMP_Text>();
            if (description != null) description.text = cardManager.GetCardDescriptionByID(cardId);

            var image = card.GetComponentInChildren<Image>();
            if (image != null) image.sprite = cardManager.GetCardImageByID(cardId);


            var highlight = card.transform.Find("Selected For Replacement")?.gameObject;
            if (highlight != null) highlight.SetActive(false);

            var button = card.GetComponentInChildren<Button>();
            if (button == null)
            {
                Debug.LogError($"Button not found in card {card.name}");
            }
            button.onClick.AddListener(() =>
            {
                if (selectedForReplacement.Contains(index))
                {
                    selectedForReplacement.Remove(index);
                    if (highlight != null) highlight.SetActive(false);
                }
                else
                {
                    selectedForReplacement.Add(index);
                    if (highlight != null) highlight.SetActive(true);
                }
            });
        }

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() =>
        {
            confirmButton.gameObject.SetActive(false);
            ConfirmMulligan(fullDeck); // pass remaining deck
        });
    }
    public List<int> Mulligan(List<int> fullDeck, List<int> indexesToReplace)
    {
        // Sort indexes descending so we can safely remove from initialHand
        indexesToReplace.Sort((a, b) => b.CompareTo(a));

        List<int> cardsToReplace = new List<int>();
        List<int> cardsToKeep = new List<int>();

        for (int i = 0; i < initialHand.Count; i++)
        {
            if (indexesToReplace.Contains(i))
            {
                cardsToReplace.Add(initialHand[i]);
            }
            else
            {
                cardsToKeep.Add(initialHand[i]);
            }
        }

        // Put replaced cards back into deck
        foreach (var cardID in cardsToReplace)
            fullDeck.Add(cardID);

        // Shuffle
        System.Random rng = new();
        int n = fullDeck.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (fullDeck[n], fullDeck[k]) = (fullDeck[k], fullDeck[n]);
        }

        // Draw new cards
        List<int> replacementCards = fullDeck.Take(indexesToReplace.Count).ToList();
        fullDeck.RemoveRange(0, indexesToReplace.Count);

        List<int> finalHand = new List<int>(initialHand); // Start with old hand
        for (int i = 0; i < indexesToReplace.Count; i++)
        {
            finalHand[indexesToReplace[i]] = replacementCards[i]; // Replace at the correct index
        }

        deck = fullDeck;
        return finalHand;
    }


    private IEnumerator AnimateMulliganSwap(List<int> newHand)
    {
        for (int i = 0; i < cardContainer.childCount; i++)
        {
            Transform cardTransform = cardContainer.GetChild(i);
            GameObject cardGO = cardTransform.gameObject;

            // Only animate cards that were selected by index
            if (!selectedForReplacement.Contains(i))
                continue;

            CanvasGroup cg = cardGO.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = cardGO.AddComponent<CanvasGroup>();

            // Fade out
            for (float t = 0; t <= 1; t += Time.deltaTime * 3)
            {
                cg.alpha = 1 - t;
                yield return null;
            }

            int newCardId = newHand[i]; // Updated card at same index

            var name = cardGO.transform.Find("Name Background/Card Name")?.GetComponent<TMP_Text>();
            var description = cardGO.transform.Find("Description Background/Card Description")?.GetComponent<TMP_Text>();
            var image = cardGO.GetComponentInChildren<Image>();
            var highlight = cardGO.transform.Find("Selected For Replacement")?.gameObject;

            if (name != null) name.text = cardManager.GetCardNameByID(newCardId);
            if (description != null) description.text = cardManager.GetCardDescriptionByID(newCardId);
            if (image != null) image.sprite = cardManager.GetCardImageByID(newCardId);
            if (highlight != null) highlight.SetActive(false);

            // Update internal initialHand state
            initialHand[i] = newCardId;

            // Fade in
            for (float t = 0; t <= 1; t += Time.deltaTime * 3)
            {
                cg.alpha = t;
                yield return null;
            }
        }

        selectedForReplacement.Clear(); // Reset for next mulligan
    }




    private IEnumerator AnimateCardOut(GameObject card)
    {
        float duration = 0.3f;
        Vector3 originalScale = card.transform.localScale;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float scale = Mathf.Lerp(1f, 0f, t / duration);
            card.transform.localScale = originalScale * scale;
            yield return null;
        }
        card.transform.localScale = Vector3.zero;
    }

    private IEnumerator AnimateCardIn(GameObject card)
    {
        float duration = 0.3f;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float scale = Mathf.Lerp(0f, 1f, t / duration);
            card.transform.localScale = Vector3.one * scale;
            yield return null;
        }
        card.transform.localScale = Vector3.one;
    }

    private async void ConfirmMulligan(List<int> remainingDeck)
    {
        var handClone = new List<int>(initialHand); // current logical hand
        var newHand = Mulligan(remainingDeck,selectedForReplacement.ToList());
        StartCoroutine(AnimateMulliganSwap(newHand));
        StartCoroutine(CallMulliganFunction(MatchManager.Instance.CurrentMatch.Match_ID,UserManager.Instance.userId));
        await SupabaseManager.Instance.PollForUpdates(MatchManager.Instance.CurrentMatch.Match_ID);
    }

    private void HandleMulligan(int playerIndex)
    {
        SupabaseManager.Instance.OnGameStart -= HandleMulligan;
        Debug.Log("handle mulligan function");
        var channelManager = FindFirstObjectByType<RealTimeChannelManager>();
        
        StartCoroutine(FadeOutAndStartGame(playerIndex));
    }
    void TriggerFadeIn()
    {
        confirmButton.gameObject.SetActive(true);
        StartCoroutine(FadeIn());
    }
    private IEnumerator FadeIn()
    {
        mulliganCanvasGroup.alpha = 0;
        mulliganCanvasGroup.interactable = true;
        mulliganCanvasGroup.blocksRaycasts = true;

        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            mulliganCanvasGroup.alpha = elapsed / duration;
            yield return null;
        }

        mulliganCanvasGroup.alpha = 1;
    }
    private IEnumerator FadeOutAndStartGame(int playerIndex)
    {
        float duration = 0.5f;
        float elapsed = 0f;
        var mulliganCanvasGroup = gameObject.GetComponent<CanvasGroup>();
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            mulliganCanvasGroup.alpha = 1 - (elapsed / duration);
            yield return null;
        }

        mulliganCanvasGroup.alpha = 0;
        mulliganCanvasGroup.interactable = false;
        mulliganCanvasGroup.blocksRaycasts = false;

        // Proceed to start the game
        MatchManager.Instance.ProceedToGame(deck, initialHand, playerIndex);
        gamePanel.InitializeGamePanel();
    }
    IEnumerator CallMulliganFunction(Guid matchID, int userID)
    {
        string url = "https://oaxeurahthkghramlwsf.supabase.co/functions/v1/check-mulligan";
        var requestBody = new
        {
            match_id = matchID.ToString(),
            user_id = userID
        };

        string json = JsonConvert.SerializeObject(requestBody);

        using UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        // request.method = UnityWebRequest.kHttpVerbPOST;
        var key = EnvReader.GetEnvVariable("SUPABASE_SERVICE_ROLE_KEY");
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {key}");


        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Mulligan function failed: " + request.error);
        }
        else
        {
            Debug.Log($"Response: {request.responseCode} - {request.downloadHandler.text}");

        }
    }

}
