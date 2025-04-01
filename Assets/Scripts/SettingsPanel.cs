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

public class SettingsPanel : MonoBehaviour
{
    [SerializeField] private MyGamesPanel myGamesPanel;
    [SerializeField] private GameObject additionalSettingsPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private CreateGamePanel createGamePanel;
    [SerializeField] private HideValidationPanel hideValidationPanel;

    [SerializeField] private TMP_InputField projectName;
    [SerializeField] private GameObject hideButton;
    [Header("Winning Conditions")]
    [SerializeField] private Toggle healthWinCon;
    [SerializeField] private Toggle cardsWinCon;

    [Header("Rules")]
    [SerializeField] private TMP_InputField playerHealth;
    [SerializeField] public TMP_Dropdown deckSize;
    [SerializeField] public TMP_Dropdown initialHandSize;
    [SerializeField] public TMP_Dropdown maxHandSize;
    [SerializeField] private Slider turnLength;

    [Header("Error Message")]
    [SerializeField] private GameObject winConError;
    [SerializeField] private GameObject projectNameError;
    [SerializeField] private GameObject playerHealthError;

    [Header("Additional Rules")]
    [SerializeField] private TMP_Dropdown cardCopies;
    [SerializeField] private TMP_Dropdown limitHandSize;
    void Start()
    {
        healthWinCon.onValueChanged.AddListener(value => TogglePlayerHealthInput(value));
        healthWinCon.onValueChanged.AddListener(value => OnWinConToggle(value));
        cardsWinCon.onValueChanged.AddListener(value => OnWinConToggle(value));
        projectName.onValueChanged.AddListener(value => projectNameError.SetActive(false));
        playerHealth.onValueChanged.AddListener(value => OnPlayerHealthInput(value));
        PopulateDropdowns();
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
        PopulateDropdown(deckSize, 2, 50, true);
        PopulateDropdown(initialHandSize, 1, 25, true);
        PopulateDropdown(maxHandSize, 1, 50, true);
        PopulateDropdown(cardCopies, 1, 10, false);
        PopulateDropdown(limitHandSize, 1, 50, false);
    }
    private void PopulateDropdown(TMP_Dropdown dropdown, int minSize = 1, int maxSize = 5, bool clearOptions = true)
    {
        if (clearOptions)
        {
            dropdown.ClearOptions();
        }
        List<string> options = new List<string>();
        for (int i = minSize; i <= maxSize; i++)
        {
            options.Add(i.ToString());
        }

        dropdown.AddOptions(options);
    }

    public void InitializeGameSettings()
    {
        if (!ValidateInput())
            return;
        additionalSettingsPanel.SetActive(true);
        myGamesPanel.gameObject.SetActive(false);
        settingsPanel.SetActive(false);

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
            { "deck_size", deckSize.options[deckSize.value].text },
            { "initial_hand_size", initialHandSize.options[initialHandSize.value].text },
            { "max_hand_size", maxHandSize.options[maxHandSize.value].text },
            { "turn_length", (int)turnLength.value },
            { "card_copies", cardCopies.options[cardCopies.value].text },
            { "limit_hand_size", limitHandSize.options[limitHandSize.value].text }
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
        var gameSettings = new Dictionary<string, object>
        {
            { "health_win_condition", healthWinCon.isOn }, { "cards_win_condition", cardsWinCon.isOn },
            { "player_health", playerHealth.text},
            { "deck_size", deckSize.options[deckSize.value].text },
            { "initial_hand_size", initialHandSize.options[initialHandSize.value].text },
            { "max_hand_size", maxHandSize.options[maxHandSize.value].text },
            { "turn_length", (int)turnLength.value },
            { "card_copies", cardCopies.options[cardCopies.value].text },
            { "limit_hand_size", limitHandSize.options[limitHandSize.value].text }
        };
        Debug.Log("deck_size: " + deckSize.options[deckSize.value].text);
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
    }

    public void PopulateUI()
    {
        gameObject.SetActive(true);
        PopulateDropdowns();

        StartCoroutine(DelayedPopulateSettings());
       
    }
    private IEnumerator DelayedPopulateSettings()
    {
        
        yield return null;

        CardGames game = createGamePanel.game;
        var settings = JObject.Parse(game.Game_Settings);
        projectName.text = game.Name;
        healthWinCon.isOn = settings["health_win_condition"].Value<bool>();
        cardsWinCon.isOn = settings["cards_win_condition"].Value<bool>();

        playerHealth.text = settings["player_health"].ToString();

        SetDropdownValue(deckSize, settings["deck_size"].ToString());
        SetDropdownValue(initialHandSize, settings["initial_hand_size"].ToString());
        SetDropdownValue(maxHandSize, settings["max_hand_size"].ToString());

        turnLength.value = settings["turn_length"].Value<float>();

        SetDropdownValue(cardCopies, settings["card_copies"].ToString());
        SetDropdownValue(limitHandSize, settings["limit_hand_size"].ToString());
    }
    private void SetDropdownValue(TMP_Dropdown dropdown, string value)
    {
        Debug.Log($"Setting dropdown value: {value}");

        int index = dropdown.options.FindIndex(option => option.text == value);
        if (index != -1)
        {
            dropdown.value = index;
            dropdown.RefreshShownValue();
        }
        else
        {
            Debug.LogError($"Value '{value}' not found in dropdown options.");
        }
        Debug.Log(dropdown.value);
    }
    public void RevertChanges()
    {
        var settings = JObject.Parse(createGamePanel.game.Game_Settings);
        SetDropdownValue(cardCopies, settings["card_copies"].ToString());
        SetDropdownValue(limitHandSize, settings["limit_hand_size"].ToString());
    }
}
