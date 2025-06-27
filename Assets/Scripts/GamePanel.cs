using System.Collections;
using System.Collections.Generic;
using Models;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;
using UnityEngine.UI;

using Michsky.UI.Reach;
public class GamePanel : MonoBehaviour
{
    [Header("Panels")]
    private CanvasGroup gameCanvasGroup;
    [Header("Cards")]
    [SerializeField] private Transform playerHandArea; // Your cards area
    [SerializeField] private Transform opponentHandArea; // Opponent's cards area
    [SerializeField] private Transform playerDeck; // Your cards area
    [SerializeField] private Transform opponentDeck; // Opponent's cards area
    [SerializeField] private GameObject cardPrefab; // A prefab for card
    [SerializeField] private GameObject cardBackPrefab; // A prefab for card (back cover)
    [SerializeField] private TMP_Text playerHealth;
    [SerializeField] private TMP_Text opponentHealth;
    
    [Header("UI Elements")]
    [SerializeField] public Michsky.UI.Reach.ButtonManager endTurnButton;
    private ScrollRect handScrollRect;
    // Prefab for the tooltip UI element
    [SerializeField] private RectTransform cardCountTooltipInstance;     // Instance of the tooltip
    [SerializeField] private RectTransform cardPlaceholder;
    private TMP_Text tooltipText;
    private Vector3 mousePosition;
    private bool isHovering = false;
    [SerializeField] GameObject YourTurnVisual;
    private CanvasGroup yourTurnCanvas;
    private void Start()
    {
        handScrollRect = playerHandArea.parent.parent.GetComponent<ScrollRect>();
        Debug.Log("GamePanel Start() triggered.");
        endTurnButton.onClick.AddListener(EndTurn);
        tooltipText = cardCountTooltipInstance.GetComponentInChildren<TMP_Text>();
        //playerHealth.gameObject.SetActive(false);
        //opponentHealth.gameObject.SetActive(false);
        yourTurnCanvas = YourTurnVisual.GetComponent<CanvasGroup>();
        if (yourTurnCanvas == null)
        {
            yourTurnCanvas = YourTurnVisual.AddComponent<CanvasGroup>();
        }

        YourTurnVisual.SetActive(false);
        yourTurnCanvas.alpha = 0f;
    }
    private void Update()
    {
        mousePosition = Input.mousePosition;
        if (isHovering)
        {
            cardCountTooltipInstance.position = Input.mousePosition;
        }
        //float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        //if (scrollInput != 0)
        //{
        //    handScrollRect.horizontalNormalizedPosition = Mathf.Clamp(handScrollRect.horizontalNormalizedPosition + scrollInput * 0.1f, 0f, 1f);
        //}

    }
    private void AddEventToOpponentHand()
    {
        // Add an EventTrigger to the opponent's container
        EventTrigger trigger = opponentHandArea.parent.GetComponent<EventTrigger>() ?? opponentHandArea.parent.gameObject.AddComponent<EventTrigger>();

        // Define the Pointer Enter event
        EventTrigger.Entry pointerEnter = new EventTrigger.Entry();
        pointerEnter.eventID = EventTriggerType.PointerEnter;
        pointerEnter.callback.AddListener((eventData) => { SetHoverState(true);
            int cardCount = MatchManager.Instance.GetOpponentCardCount();
            if (tooltipText != null)
                tooltipText.text = $"Cards: {cardCount}";
        });

        // Define the Pointer Exit event
        EventTrigger.Entry pointerExit = new EventTrigger.Entry();
        pointerExit.eventID = EventTriggerType.PointerExit;
        pointerExit.callback.AddListener((eventData) => SetHoverState(false));

        // Add events to the trigger
        trigger.triggers.Add(pointerEnter);
        trigger.triggers.Add(pointerExit);
    }

    private void AddEventToDecks()
    {
        // Add an EventTrigger to the opponent's container
        EventTrigger playerTrigger = playerDeck.GetComponent<EventTrigger>() ?? playerDeck.gameObject.AddComponent<EventTrigger>();

        // Define the Pointer Enter event
        EventTrigger.Entry pointerEnter = new EventTrigger.Entry();
        pointerEnter.eventID = EventTriggerType.PointerEnter;
        pointerEnter.callback.AddListener((eventData) => {
            int cardCount = MatchManager.Instance.GetPlayerDeckCount();
            if (tooltipText != null)
                tooltipText.text = $"Cards: {cardCount}";
            SetHoverState(true);
        });

        // Define the Pointer Exit event
        EventTrigger.Entry pointerExit = new EventTrigger.Entry();
        pointerExit.eventID = EventTriggerType.PointerExit;
        pointerExit.callback.AddListener((eventData) => SetHoverState(false));

        // Add events to the trigger
        playerTrigger.triggers.Add(pointerEnter);
        playerTrigger.triggers.Add(pointerExit);

        EventTrigger opponentTrigger = opponentDeck.GetComponent<EventTrigger>() ?? opponentDeck.gameObject.AddComponent<EventTrigger>();

        // Define the Pointer Enter event
        EventTrigger.Entry pointerEnter2 = new EventTrigger.Entry();
        pointerEnter2.eventID = EventTriggerType.PointerEnter;
        pointerEnter2.callback.AddListener((eventData) => {
            int cardCount = MatchManager.Instance.GetOpponentDeckCount();
            if (tooltipText != null)
                tooltipText.text = $"Cards: {cardCount}";
            SetHoverState(true);
        });

        // Define the Pointer Exit event
        EventTrigger.Entry pointerExit2 = new EventTrigger.Entry();
        pointerExit2.eventID = EventTriggerType.PointerExit;
        pointerExit2.callback.AddListener((eventData) => SetHoverState(false));

        // Add events to the trigger
        opponentTrigger.triggers.Add(pointerEnter2);
        opponentTrigger.triggers.Add(pointerExit2);
    }

    public void SetHoverState(bool state)
    {
        isHovering = state;
        cardCountTooltipInstance.gameObject.SetActive(state);
    }

public void InitializeGamePanel()
    {
    
        gameObject.SetActive(true);
        var maxHealth = MatchManager.Instance.maxHealth;
        if (maxHealth != 0)
        {
            //playerHealth.gameObject.SetActive(true);
            //opponentHealth.gameObject.SetActive(true);
            playerHealth.text = $"Health: { maxHealth.ToString() }";
            opponentHealth.text = $"Health: {maxHealth.ToString()}";

        }
        else
        {
            playerHealth.text = "";
            opponentHealth.text = "";
        }
        gameCanvasGroup = gameObject.GetComponent<CanvasGroup>();
        gameCanvasGroup.alpha = 0; // Verify alpha is set to 0
        AddEventToOpponentHand();
        AddEventToDecks();
        // Fade in game panel
        StartCoroutine(FadeInGamePanel());

        // Show your cards and opponent's cards (with back cover)
        ShowCardsForPlayer();
       
        // Setup game board for each player
        SetupGameBoard();
    }

    private IEnumerator FadeInGamePanel()
    {
        // Fade in effect for game panel
      

        float fadeDuration = 0.5f;
        
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            gameCanvasGroup.alpha = t / fadeDuration;
          
            yield return null;
        }

        gameCanvasGroup.alpha = 1;
      

    }

   
    public void ShowCardsForOpponent()
    {
        int cardCount = MatchManager.Instance.GetOpponentCardCount();
        Debug.Log(cardCount);
        foreach (Transform child in opponentHandArea)
            Destroy(child.gameObject);
        for (int i = 0; i < cardCount; i++)
        {
            Instantiate(cardBackPrefab, opponentHandArea);
        }
    }

    public void ShowCardsForPlayer()
    {
        int cardCount = MatchManager.Instance.GetPlayerCardCount();

        // Clear any existing cards in the player's hand area
        foreach (Transform child in playerHandArea)
            Destroy(child.gameObject);

        List<int> hand = MatchManager.Instance.hand;
        foreach (var cardID in hand)
        {
            // Instantiate the card and set it as a child of the player's hand area
            var cardItem = Instantiate(cardPrefab, playerHandArea);

            // Find the "Card Description" child inside "Description Background"
            Transform description = cardItem.transform.Find("Description Background/Card Description");
            description.GetComponent<TMP_Text>().text = CardsManager.Instance.GetCardDescriptionByID(cardID);

            TMP_Text cardText = cardItem.transform.Find("Name Background/Card Name")?.GetComponent<TMP_Text>();
            cardText.text = CardsManager.Instance.GetCardNameByID(cardID);

            Image cardImage = cardItem.GetComponentInChildren<Image>();
            cardImage.sprite = CardsManager.Instance.GetCardImageByID(cardID); // Display card image

            // Get or add an EventTrigger component to handle hover events
            EventTrigger trigger = cardItem.GetComponent<EventTrigger>() ?? cardItem.gameObject.AddComponent<EventTrigger>();

            // Define "Pointer Enter" action (hover effect + show description)
            EventTrigger.Entry pointerEnter = new EventTrigger.Entry();
            pointerEnter.eventID = EventTriggerType.PointerEnter;
            pointerEnter.callback.AddListener((eventData) =>
            {
                cardItem.transform.localPosition += new Vector3(0, 220, 0); // Move card up
                if (description != null) LeanTween.alphaCanvas(description.GetComponent<CanvasGroup>(), 1, 0.3f); // Smooth fade in
            });

            Button cardPlayButton = cardItem.GetComponentInChildren<Button>();
            cardPlayButton.onClick.AddListener(() =>  MatchManager.Instance.PlayCard(cardID, cardItem));


            // Define "Pointer Exit" action (reset effect + hide description)
            EventTrigger.Entry pointerExit = new EventTrigger.Entry();
            pointerExit.eventID = EventTriggerType.PointerExit;
            pointerExit.callback.AddListener((eventData) =>
            {
                if (cardItem.GetComponent<RectTransform>().anchoredPosition.y > 0)
                {
                    cardItem.transform.localPosition -= new Vector3(0, 220, 0); // Move card back down
                    if (description != null) LeanTween.alphaCanvas(description.GetComponent<CanvasGroup>(), 0, 0.3f); // Smooth fade out
                }
            });


            // Add both actions to the EventTrigger
            trigger.triggers.Add(pointerEnter);
            trigger.triggers.Add(pointerExit);
        }

        Debug.Log("Cards displayed with hover effects and description visibility.");
    }
    public void PlayerDrawCardUI(int cardID)
    {
        GameObject cardItem = Instantiate(cardPrefab, playerHandArea);
        RectTransform cardRect = cardItem.GetComponent<RectTransform>();
        //cardRect.anchoredPosition = playerHandArea.GetComponent<RectTransform>().anchoredPosition;
        LayoutRebuilder.ForceRebuildLayoutImmediate(playerHandArea.GetComponent<RectTransform>());
        cardItem.SetActive(false); // Hide the real card during animation

        Transform description = cardItem.transform.Find("Description Background/Card Description");
        description.GetComponent<TMP_Text>().text = CardsManager.Instance.GetCardDescriptionByID(cardID);

        TMP_Text cardText = cardItem.transform.Find("Name Background/Card Name")?.GetComponent<TMP_Text>();
        cardText.text = CardsManager.Instance.GetCardNameByID(cardID);

        Image cardImage = cardItem.GetComponentInChildren<Image>();
        cardImage.sprite = CardsManager.Instance.GetCardImageByID(cardID); // Display card image

        // Get or add an EventTrigger component to handle hover events
        EventTrigger trigger = cardItem.GetComponent<EventTrigger>() ?? cardItem.gameObject.AddComponent<EventTrigger>();

        // Define "Pointer Enter" action (hover effect + show description)
        EventTrigger.Entry pointerEnter = new EventTrigger.Entry();
        pointerEnter.eventID = EventTriggerType.PointerEnter;
        pointerEnter.callback.AddListener((eventData) =>
        {
            cardItem.transform.localPosition += new Vector3(0, 220, 0); // Move card up
            if (description != null) LeanTween.alphaCanvas(description.GetComponent<CanvasGroup>(), 1, 0.3f); // Smooth fade in
        });

        Button cardPlayButton = cardItem.GetComponentInChildren<Button>();
        cardPlayButton.onClick.AddListener(() => MatchManager.Instance.PlayCard(cardID, cardItem));


        // Define "Pointer Exit" action (reset effect + hide description)
        EventTrigger.Entry pointerExit = new EventTrigger.Entry();
        pointerExit.eventID = EventTriggerType.PointerExit;
        pointerExit.callback.AddListener((eventData) =>
        {
            if (cardItem.GetComponent<RectTransform>().anchoredPosition.y > 0)
            {
                cardItem.transform.localPosition -= new Vector3(0, 220, 0); // Move card back down
                if (description != null) LeanTween.alphaCanvas(description.GetComponent<CanvasGroup>(), 0, 0.3f); // Smooth fade out
            }
        });
        GameObject deckCard = Instantiate(cardItem, playerDeck);
        deckCard.SetActive(true); // If cardItem was inactive

        RectTransform deckCardRect = deckCard.GetComponent<RectTransform>();
        deckCardRect.anchoredPosition = playerDeck.GetComponent<RectTransform>().anchoredPosition;

        // Add both actions to the EventTrigger
        trigger.triggers.Add(pointerEnter);
        trigger.triggers.Add(pointerExit);
        Vector2 targetPosition = cardPlaceholder.anchoredPosition;


        Debug.Log($"📍 Corrected Target World Position: {targetPosition}");
        StartCoroutine(AnimateCardToContainer(deckCard, targetPosition, 0.5f, cardItem));



    }

    private IEnumerator AnimateCardToContainer(GameObject deckCard, Vector2 targetPosition, float duration, GameObject handCard)
    {
        //targetPosition.x = -targetPosition.x;
        RectTransform rectTransform = deckCard.GetComponent<RectTransform>();

        // Log the initial position (deck)
        Vector2 startPos = rectTransform.anchoredPosition;

        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;

            // Log position at each frame
            yield return null;
        }
        Destroy(deckCard);
        // Log the final position
        rectTransform.anchoredPosition = targetPosition;
        handCard.SetActive(true);
    }
    public void OpponentDrawCardUI()
    {
        Debug.Log("OPPONENT DRAW CARD UI");
        GameObject cardItem = Instantiate(cardBackPrefab, opponentHandArea);
        RectTransform cardRect = cardItem.GetComponent<RectTransform>();
        //cardRect.anchoredPosition = playerHandArea.GetComponent<RectTransform>().anchoredPosition;
        LayoutRebuilder.ForceRebuildLayoutImmediate(playerHandArea.GetComponent<RectTransform>());
        cardItem.SetActive(false); // Hide the real card during animation

        GameObject deckCard = Instantiate(cardItem, opponentDeck);
        deckCard.SetActive(true); // If cardItem was inactive

        RectTransform deckCardRect = deckCard.GetComponent<RectTransform>();
        deckCardRect.anchoredPosition = opponentDeck.GetComponent<RectTransform>().anchoredPosition;


        Vector2 targetPosition = opponentHandArea.GetComponent<RectTransform>().anchoredPosition;

        Debug.Log($"📍 Corrected Target World Position: {targetPosition}");
        StartCoroutine(AnimateCardToContainer(deckCard, targetPosition, 0.5f, cardItem));



    }
    public IEnumerator ShowPlayedCardsWithDelay(IEnumerable<int> cardIDs, bool isOpponent)
    {
        foreach (var cardID in cardIDs)
        {
            yield return StartCoroutine(ShowCardPlayed(cardID));
            if(isOpponent)
                CardsManager.Instance.PlayCardEffect(cardID, true);
        }
        MatchManager.Instance.isPlayingCard = false;
    }
    private IEnumerator ShowCardPlayed(int cardID)
    {
        GameObject playedCardUI = Instantiate(cardPrefab, gameObject.transform);
        RectTransform cardRect = playedCardUI.GetComponent<RectTransform>();

        // Set anchor to middle of the screen
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);

        // Position it exactly at the center (0,0 relative to screen center)
        cardRect.anchoredPosition = Vector2.zero;

        // Assign the card's data (name, image, etc.)
        Transform description = playedCardUI.transform.Find("Description Background/Card Description");
        description.GetComponent<TMP_Text>().text = CardsManager.Instance.GetCardDescriptionByID(cardID);

        TMP_Text cardText = playedCardUI.transform.Find("Name Background/Card Name")?.GetComponent<TMP_Text>();
        cardText.text = CardsManager.Instance.GetCardNameByID(cardID);

        Image cardImage = playedCardUI.GetComponentInChildren<Image>();
        cardImage.sprite = CardsManager.Instance.GetCardImageByID(cardID); // Display card image
        // Optional: Animate it into place
        yield return StartCoroutine(AnimateCardAppearance(cardRect));
        yield return new WaitForSeconds(3); // Optional delay between animations
        yield return StartCoroutine(FadeOutAndDestroy(playedCardUI));
        
    }
    private IEnumerator AnimateCardAppearance(RectTransform cardRect)
    {
        cardRect.localScale = Vector3.zero; // Start small
        float duration = 0.5f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            cardRect.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cardRect.localScale = Vector3.one; // Ensure final size
    }
    private IEnumerator FadeOutAndDestroy(GameObject card)
    {
        // Check if the card has a CanvasGroup, if not, add one
        CanvasGroup canvasGroup = card.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = card.AddComponent<CanvasGroup>(); // Attach new CanvasGroup
        }

        float duration = 1f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(1, 0, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(card);
    }
    private void SetupGameBoard()
    {
        // Example: Setup game board where each player has their side
        // Adjust based on your actual game logic and board setup

        // Here you could implement a more detailed layout
        // For example, you could set positions or background elements for each player’s side
    }

    public void EndTurn()
    {
        // Example: Implement your end turn logic here
        MatchManager.Instance.EndTurn();
    }
    public IEnumerator ShowYourTurnVisual()
    {
        YourTurnVisual.SetActive(true);
        float currentTime = 0f;
        float fadeDuration = 0.5f;
        float displayDuration = 0.5f;
        // Fade In
        while (currentTime < fadeDuration)
        {
            yourTurnCanvas.alpha = Mathf.Lerp(0f, 1f, currentTime / fadeDuration);
            currentTime += Time.deltaTime;
            yield return null;
        }
        yourTurnCanvas.alpha = 1f;

        // Wait before fading out
        yield return new WaitForSeconds(displayDuration);

        // Fade Out
        currentTime = 0f;
        while (currentTime < fadeDuration)
        {
            yourTurnCanvas.alpha = Mathf.Lerp(1f, 0f, currentTime / fadeDuration);
            currentTime += Time.deltaTime;
            yield return null;
        }
        yourTurnCanvas.alpha = 0f;

        YourTurnVisual.SetActive(false);

    }
    public void UpdatePlayerHealth(int newHealth)
    {
        playerHealth.text = $"Health: {newHealth.ToString()}";
    }
    public void UpdateOpponentHealth(int newHealth)
    {
        opponentHealth.text = $"Health: {newHealth.ToString()}";
    }
}

