using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class EffectsPanel : MonoBehaviour
{
    [Header("Available Effects UI")]
    [SerializeField] private TMP_Dropdown dropdownAvailableActions;
    [SerializeField] private ScrollRect availableEffectsScrollView;
    [SerializeField] private GameObject effectTogglePrefab;
    [SerializeField] private TMP_Dropdown xDropdown;
    [Header("Existing Effects UI")]
    [SerializeField] private TMP_Dropdown dropdownExistingActions;
    [SerializeField] private ScrollRect existingEffectsScrollView;

    [Header("Buttons")]
    [SerializeField] private Button addButton;
    [SerializeField] private Button removeButton;
    private bool isInitialized = false;
    private Dictionary<string, List<string>> availableEffects;

    public Dictionary<string, List<EffectWithX>> existingEffects { get; private set; }

    public struct EffectWithX
    {
        public string Effect;
        public string X;

        public EffectWithX(string effect, string x)
        {
            Effect = effect;
            X = x;
        }

        public override string ToString()
        {
            return $"{Effect} (X: {X})";
        }
    }

    private void Start()
    {
        PopulateDropdown();
        addButton.onClick.AddListener(AddSelectedEffects);
        removeButton.onClick.AddListener(RemoveSelectedEffects);

        dropdownExistingActions.onValueChanged.AddListener(delegate { PopulateExistingEffects(); });
    }
    private void PopulateDropdown()
    {
        // Clear existing options
        xDropdown.ClearOptions();

        // Generate a list of numbers from 1 to 20
        List<string> options = new List<string>();
        for (int i = 1; i <= 20; i++)
        {
            options.Add(i.ToString());
        }

        // Add options to the dropdown
        xDropdown.AddOptions(options);
    }
    public void InitializePanel()
    {
        if (isInitialized) return;

        isInitialized = true;

        xDropdown.value = 0;
        availableEffects = new Dictionary<string, List<string>>()
        {
            { "On Play", new List<string> { "Draw X Cards", "Gain X Mana", "Deal X Damage" } },
            { "On Draw", new List<string> { "Heal X HP", "Add a Shield", "Reveal Top X Cards" } }
        };
        existingEffects = new Dictionary<string, List<EffectWithX>>()
        {
            { "On Play", new List<EffectWithX>() },
            { "On Draw", new List<EffectWithX>() }
        };
        PopulateDropdowns();
        PopulateAvailableEffects();
        PopulateExistingEffects();
    }
    private void PopulateDropdowns()
    {
        // Clear existing options and listeners
        dropdownAvailableActions.ClearOptions();
        dropdownAvailableActions.onValueChanged.RemoveAllListeners();

        dropdownExistingActions.ClearOptions();
        dropdownExistingActions.onValueChanged.RemoveAllListeners();

        // Add new options
        var actions = new List<string> { "On Play", "On Draw" };
        dropdownAvailableActions.AddOptions(actions);
        dropdownExistingActions.AddOptions(actions);

        // Re-add listeners
        dropdownAvailableActions.onValueChanged.AddListener(delegate { PopulateAvailableEffects(); });
        dropdownExistingActions.onValueChanged.AddListener(delegate { PopulateExistingEffects(); });
    }

    private void PopulateAvailableEffects()
    {
        foreach (Transform child in availableEffectsScrollView.content)
        {
            Destroy(child.gameObject);
        }

        string selectedAction = dropdownAvailableActions.options[dropdownAvailableActions.value].text;
        if (availableEffects.ContainsKey(selectedAction))
        {
            foreach (var effect in availableEffects[selectedAction])
            {
                GameObject newToggle = Instantiate(effectTogglePrefab, availableEffectsScrollView.content);
                newToggle.GetComponentInChildren<Text>().text = effect;

                var toggle = newToggle.GetComponent<Toggle>();
                toggle.onValueChanged.AddListener(isSelected =>
                {
                    if (isSelected)
                    {
                        Debug.Log($"Effect selected: {effect}");
                    }
                });
            }
        }
    }

    private void PopulateExistingEffects()
    {
        foreach (Transform child in existingEffectsScrollView.content)
        {
            Destroy(child.gameObject);
        }

        string selectedAction = dropdownExistingActions.options[dropdownExistingActions.value].text;

        if (existingEffects.ContainsKey(selectedAction))
        {
            foreach (var effectWithX in existingEffects[selectedAction])
            {
                GameObject newToggle = Instantiate(effectTogglePrefab, existingEffectsScrollView.content);
                newToggle.GetComponentInChildren<Text>().text = effectWithX.ToString();

                var toggle = newToggle.GetComponent<Toggle>();
                toggle.onValueChanged.AddListener(isSelected =>
                {
                    if (isSelected)
                    {
                        Debug.Log($"Effect selected for removal: {effectWithX.Effect} (X: {effectWithX.X})");
                    }
                });
            }
        }
    }


private void AddSelectedEffects()
    {
        string selectedAction = dropdownAvailableActions.options[dropdownAvailableActions.value].text;
        string selectedX = xDropdown.options[xDropdown.value].text; // Get the selected X value

        foreach (Transform child in availableEffectsScrollView.content)
        {
            var toggle = child.GetComponent<Toggle>();
            if (toggle.isOn)
            {
                string effect = child.GetComponentInChildren<Text>().text;

                var effectWithX = new EffectWithX(effect, selectedX);

                if (!existingEffects[selectedAction].Contains(effectWithX))
                {
                    existingEffects[selectedAction].Add(effectWithX);
                    availableEffects[selectedAction].Remove(effect);
                }
            }
        }

        PopulateAvailableEffects();
        PopulateExistingEffects();
    }


   private void RemoveSelectedEffects()
    {
        string selectedAction = dropdownExistingActions.options[dropdownExistingActions.value].text;

        var effectsToRemove = new List<EffectWithX>();

        foreach (Transform child in existingEffectsScrollView.content)
        {
            var toggle = child.GetComponent<Toggle>();
            if (toggle.isOn)
            {
                string effectText = child.GetComponentInChildren<Text>().text;

                var parts = effectText.Split(new[] { " (X: " }, System.StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    string effect = parts[0];
                    string xValue = parts[1].TrimEnd(')');
                    effectsToRemove.Add(new EffectWithX(effect, xValue));
                }
            }
        }

        foreach (var effectWithX in effectsToRemove)
        {
            if (existingEffects[selectedAction].Remove(effectWithX))
            {
                availableEffects[selectedAction].Add(effectWithX.Effect);
            }
        }

        PopulateAvailableEffects();
        PopulateExistingEffects();
    }
    public void PopulateEffectsFromDatabase(Dictionary<string, List<EffectWithX>> databaseEffects)
    {
        // Clear existing effects and repopulate
        
        InitializePanel();
        availableEffects = new Dictionary<string, List<string>>()
        {
        { "On Play", new List<string> { "Draw X Cards", "Gain X Mana", "Deal X Damage" } },
        { "On Draw", new List<string> { "Heal X HP", "Add a Shield", "Reveal Top X Cards" } }
        };
        existingEffects.Clear();

        foreach (var action in databaseEffects)
        {
            if (!existingEffects.ContainsKey(action.Key))
            {
                existingEffects[action.Key] = new List<EffectWithX>();
            }

            foreach (var effectWithX in action.Value)
            {
                // Add to existing effects
                existingEffects[action.Key].Add(effectWithX);

                // Remove from available effects if it exists there
                if (availableEffects.ContainsKey(action.Key))
                {
                    availableEffects[action.Key].Remove(effectWithX.Effect);
                }
            }
        }

        // Refresh the UI
        PopulateAvailableEffects();
        PopulateExistingEffects();
    }
    public async Task OnEditCard(int cardId)
    {
        var databaseEffects = await SupabaseManager.Instance.GetEffectsFromDatabase(cardId);

        if (databaseEffects != null)
        {
            PopulateEffectsFromDatabase(databaseEffects);
        }
        else
        {
            Debug.LogError("Failed to fetch effects from database.");
        }
    }
}
