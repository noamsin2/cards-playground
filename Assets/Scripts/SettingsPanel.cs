using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;
using static UnityEngine.Rendering.DebugUI;
using Michsky.UI.Reach;

public class SettingsPanel : MonoBehaviour
{
    [SerializeField] private MyGamesPanel myGamesPanel;
    [SerializeField] private GameObject additionalSettingsPanel;
    [SerializeField] private SettingsPanel settingsPanel;
    //[SerializeField] private GameObject settings;
    [SerializeField] private CreateGamePanel createGamePanel;
    [SerializeField] private HideValidationPanel hideValidationPanel;

    [SerializeField] private TMP_InputField projectName;
    [SerializeField] private GameObject hideButton;
    [Header("Winning Conditions")]
    [SerializeField] private Toggle healthWinCon;
    [SerializeField] private Toggle cardsWinCon;

    [Header("Rules")]
    [SerializeField] private TMP_InputField playerHealth;
    [SerializeField] public Michsky.UI.Reach.Dropdown deckSize;
    [SerializeField] public Michsky.UI.Reach.Dropdown initialHandSize;
    [SerializeField] public Michsky.UI.Reach.Dropdown maxHandSize;
    [SerializeField] private SliderManager turnLength;

    [Header("Error Message")]
    [SerializeField] private GameObject winConError;
    [SerializeField] private GameObject projectNameError;
    [SerializeField] private GameObject playerHealthError;

    [Header("Additional Rules")]
    [SerializeField] private Michsky.UI.Reach.Dropdown cardCopies;
    [SerializeField] private Michsky.UI.Reach.Dropdown limitHandSize;
    void Start()
    {
        healthWinCon.onValueChanged.AddListener(value => TogglePlayerHealthInput(value));
        healthWinCon.onValueChanged.AddListener(value => OnWinConToggle(value));
        cardsWinCon.onValueChanged.AddListener(value => OnWinConToggle(value));
        projectName.onValueChanged.AddListener(value => projectNameError.SetActive(false));
        playerHealth.onValueChanged.AddListener(value => OnPlayerHealthInput(value));
        //PopulateDropdowns();
    }

    // called from edit game window
    public void InitializeSettings()
    {
        CardGames game = createGamePanel.game;
        if (game.Is_Published == true)
        {
            hideButton.GetComponent<UnityEngine.UI.Button>().onClick.RemoveAllListeners();
            hideButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => hideValidationPanel.InitializePanel(game));
            hideButton.gameObject.SetActive(true);
        }
        else
            hideButton.gameObject.SetActive(false);
    }
    public void OnHideButton()
    {
        hideButton.gameObject.SetActive(false);
    }
    private void OnPlayerHealthInput(string value_str)
    {
        if (!value_str.IsNullOrEmpty())
        {
            int value = int.Parse(value_str);
            if (value < 1 || value > 99999)
            {
                if (value == 0)
                    playerHealth.text = "1";
                else
                    playerHealth.text = "99999";

                playerHealthError.GetComponent<TextMeshProUGUI>().text = "(number must be between 1 and 100,000)";
                playerHealthError.SetActive(true);
            }
            else
            {
                playerHealthError.SetActive(false);
            }
        }
    }
    private void OnWinConToggle(bool value)
    {
        if (value)
        {
            winConError.SetActive(false);
        }
    }
    private void TogglePlayerHealthInput(bool value)
    {
        playerHealth.interactable = value;
        
    }
    public void PopulateDropdowns()
    {
        Debug.Log("POPULATE DROPDOWN");
        if (deckSize != null)
            PopulateDropdown(deckSize, 2, 50, false);
        if (initialHandSize != null)
            PopulateDropdown(initialHandSize, 1, 25, false);
        if (maxHandSize != null)
            PopulateDropdown(maxHandSize, 1, 50, false);
    }
    public void PopulateAdditionalDropdowns()
    {

        if (cardCopies != null)
            PopulateDropdown(cardCopies, 1, 10, true);
        if (limitHandSize != null)
            PopulateDropdown(limitHandSize, 1, 50, true);
    }
    private void PopulateDropdown(Michsky.UI.Reach.Dropdown dropdown, int minSize = 1, int maxSize = 5, bool noOption = true)
    {
        Debug.Log("POPULATE DROPDOWN FOR " + dropdown.name);
        dropdown.items.Clear();
        if (noOption)
        {
            dropdown.CreateNewItem("No", false);
        }
        for (int i = minSize; i <= maxSize; i++)
        {
            dropdown.CreateNewItem(i.ToString(), false);
        }
        dropdown.Initialize();
        
    }

    public void InitializeGameSettings()
    {
        if (!ValidateInput())
            return;
        additionalSettingsPanel.SetActive(true);
        //PopulateAdditionalDropdowns();
        //myGamesPanel.gameObject.SetActive(false);
        //settings.gameObject.SetActive(false);

    }
    private bool ValidateInput()
    {
        bool flag = true;
        if (!healthWinCon.isOn && !cardsWinCon.isOn)
        {
            flag = false;
            winConError.SetActive(true);
        }
        if (projectName.text.IsNullOrEmpty())
        {
            flag = false;
            projectNameError.SetActive(true);
        }
        if (healthWinCon.isOn)
        { // If health win con is activated and text is empty
            if (playerHealth.text.IsNullOrEmpty())
            {
                flag = false;
                playerHealthError.GetComponent<TextMeshProUGUI>().text = "(must enter a number)";
                playerHealthError.SetActive(true);
            }
        }

        return flag;
    }
    public void EditGameSettings()
    {
        if (!ValidateInput())
            return;
        CardGames game = createGamePanel.game;

        var gameSettings = new Dictionary<string, object>
        {
            { "health_win_condition", healthWinCon.isOn }, { "cards_win_condition", cardsWinCon.isOn },
            { "player_health", playerHealth.text},
            { "deck_size", settingsPanel.deckSize.items[deckSize.index].itemName },
            { "initial_hand_size", settingsPanel.initialHandSize.items[initialHandSize.index].itemName },
            { "max_hand_size", settingsPanel.maxHandSize.items[maxHandSize.index].itemName },
            { "turn_length", (int)settingsPanel.turnLength.mainSlider.value},
            { "card_copies", cardCopies.items[cardCopies.index].itemName },
            { "limit_hand_size", limitHandSize.items[limitHandSize.index].itemName }
        };
        string jsonSettings = JsonConvert.SerializeObject(gameSettings);

        Debug.Log(jsonSettings);
        EditGameSettingsInDatabase(game.Game_ID, projectName.text, jsonSettings);
        gameObject.SetActive(false);

    }
    private async Task EditGameSettingsInDatabase(int gameId, string name, string jsonSettings)
    {
        CardGames game = await SupabaseManager.Instance.EditGameSettingsInDatabase(gameId, name, jsonSettings);
        createGamePanel.game.Game_Settings = game.Game_Settings;
    }
    public void InitializeAdditionalSettings()
    {
        Debug.Log("ADDITIONAL SETTINGS");
        if (settingsPanel == null)
        {
            Debug.LogError("settingsPanel is null!");
            return;
        }

        string GetDropdownValue(Michsky.UI.Reach.Dropdown dropdown)
        {
            if (dropdown == null)
            {
                Debug.LogWarning("Dropdown is null");
                return "null";
            }

            if (dropdown.items == null || dropdown.items.Count <= dropdown.index)
            {
                Debug.LogWarning($"Dropdown '{dropdown.name}' has invalid items or index");
                return "invalid";
            }

            return dropdown.items[dropdown.index].itemName;
        }
         var gameSettings = new Dictionary<string, object>
        {
            { "health_win_condition", healthWinCon?.isOn ?? false },
            { "cards_win_condition", cardsWinCon?.isOn ?? false },
            { "player_health", playerHealth?.text ?? "0" },
            { "deck_size", GetDropdownValue(settingsPanel.deckSize) },
            { "initial_hand_size", GetDropdownValue(settingsPanel.initialHandSize) },
            { "max_hand_size", GetDropdownValue(settingsPanel.maxHandSize) },
            { "turn_length", (int)settingsPanel.turnLength.mainSlider.value},
            { "card_copies", GetDropdownValue(cardCopies) },
            { "limit_hand_size", GetDropdownValue(limitHandSize) }
        };
        // Convert dictionary to JSON
        string jsonSettings = JsonConvert.SerializeObject(gameSettings);

        Debug.Log(jsonSettings);
        InitializeSettingsInDatabase(jsonSettings);
    }
    // Creates the game in the database
    public async Task InitializeSettingsInDatabase(string jsonSettings)
    {
        Debug.Log("ADDITIONAL SETTINGS2");
        var newGame = await SupabaseManager.Instance.AddGameToDatabase(projectName.text, jsonSettings);

        if (newGame != null)
        {
            Debug.Log("Game added successfully.");
        }
        else
        {
            Debug.LogError("Failed to add game.");
            return;
        }
        createGamePanel.InitializePanel(newGame);
        gameObject.SetActive(false);
        myGamesPanel.DisplayGames();
    }

    public void PopulateUI()
    {
        gameObject.SetActive(true);
        PopulateDropdowns();
        PopulateAdditionalDropdowns();
        StartCoroutine(DelayedPopulateSettings());
       
    }
    private IEnumerator DelayedPopulateSettings()
    {
        
        yield return null;
        yield return new WaitUntil(() =>
           deckSize.items.Count > 0 &&
           initialHandSize.items.Count > 0 &&
           maxHandSize.items.Count > 0 &&
           cardCopies.items.Count > 0 &&
           limitHandSize.items.Count > 0
       );

        Debug.Log("DELAYED POPULATE");
        CardGames game = createGamePanel.game;
        var settings = JObject.Parse(game.Game_Settings);
        projectName.text = game.Name;
        healthWinCon.isOn = settings["health_win_condition"].Value<bool>();
        cardsWinCon.isOn = settings["cards_win_condition"].Value<bool>();

        playerHealth.text = settings["player_health"].ToString();

        SetDropdownValue(deckSize, settings["deck_size"].ToString());
        SetDropdownValue(initialHandSize, settings["initial_hand_size"].ToString());
        SetDropdownValue(maxHandSize, settings["max_hand_size"].ToString());

        turnLength.mainSlider.value = settings["turn_length"].Value<float>();

        SetDropdownValue(cardCopies, settings["card_copies"].ToString());
        SetDropdownValue(limitHandSize, settings["limit_hand_size"].ToString());
    }
    private void SetDropdownValue(Michsky.UI.Reach.Dropdown dropdown, string value)
    {
        Debug.Log($"Setting dropdown value: {value}");

        int index = dropdown.items.FindIndex(item => item.itemName == value);
        if (index != -1)
        {
            dropdown.index = index;
            dropdown.SetDropdownIndex(index);
            Debug.Log($"Dropdown value set to: {dropdown.items[index].itemName}");
        }
        else
        {
            Debug.LogError($"Value '{value}' not found in dropdown items.");
        }
    }
    public void RevertChanges()
    {
        var settings = JObject.Parse(createGamePanel.game.Game_Settings);
        SetDropdownValue(settingsPanel.cardCopies, settings["card_copies"].ToString());
        SetDropdownValue(settingsPanel.limitHandSize, settings["limit_hand_size"].ToString());
    }
}
