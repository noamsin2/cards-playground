using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using Michsky.UI.Reach;
using UnityEngine.UIElements;
public class EffectsPanel : MonoBehaviour
{
    [Header("Available Effects UI")]
    [SerializeField] private Michsky.UI.Reach.Dropdown dropdownAvailableActions;
    [SerializeField] private ScrollRect availableEffectsScrollView;
    [SerializeField] private GameObject effectTogglePrefab;
    [SerializeField] private Michsky.UI.Reach.Dropdown xDropdown;
    [Header("Existing Effects UI")]
    [SerializeField] private Michsky.UI.Reach.Dropdown dropdownExistingActions;
    [SerializeField] private ScrollRect existingEffectsScrollView;

    [Header("Buttons")]
    [SerializeField] private ButtonManager addButton;
    [SerializeField] private ButtonManager removeButton;
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
        xDropdown.items.Clear();

        // Generate a list of numbers from 1 to 20
        List<string> options = new List<string>();
        for (int i = 1; i <= 20; i++)
        {
            xDropdown.CreateNewItem(i.ToString());
        }
    }
    public void InitializePanel()
    {
        //if (isInitialized) return;

        //isInitialized = true;

        //xDropdown.value = 0;
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
        dropdownAvailableActions.items.Clear();
        dropdownAvailableActions.onValueChanged.RemoveAllListeners();

        dropdownExistingActions.items.Clear();
        dropdownExistingActions.onValueChanged.RemoveAllListeners();

        // Add new options
        var actions = new List<string> { "On Play", "On Draw" };
        foreach (var action in actions) {
            dropdownAvailableActions.CreateNewItem(action);
            dropdownExistingActions.CreateNewItem(action);
        }
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

        string selectedAction = dropdownAvailableActions.items[dropdownAvailableActions.selectedItemIndex].itemName;
        Debug.Log("AVAILABLE SELECTED ACTION: " + selectedAction);
        if (availableEffects.ContainsKey(selectedAction))
        {
            foreach (var effect in availableEffects[selectedAction])
            {
                Debug.Log("available effect:" + effect);
                GameObject newToggle = Instantiate(effectTogglePrefab, availableEffectsScrollView.content);
                newToggle.GetComponentInChildren<TMP_Text>().text = effect;

                var toggle = newToggle.GetComponent<UnityEngine.UI.Toggle>();
                toggle.isOn = false;
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

        string selectedAction = dropdownExistingActions.items[dropdownExistingActions.selectedItemIndex].itemName;
        Debug.Log("AVAILABLE SELECTED ACTION: " + selectedAction);
        if (existingEffects.ContainsKey(selectedAction))
        {
            foreach (var effectWithX in existingEffects[selectedAction])
            {
                GameObject newToggle = Instantiate(effectTogglePrefab, existingEffectsScrollView.content);
                newToggle.GetComponentInChildren<TMP_Text>().text = effectWithX.ToString();

                var toggle = newToggle.GetComponent<UnityEngine.UI.Toggle>();
                toggle.isOn = false;
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
        string selectedAction = dropdownAvailableActions.items[dropdownAvailableActions.selectedItemIndex].itemName;
        string selectedX = xDropdown.items[xDropdown.selectedItemIndex].itemName;
        Debug.Log($"SELECTED ACTION {selectedAction}");
        Debug.Log($"SELECTED VALUE {selectedX}");
        foreach (Transform child in availableEffectsScrollView.content)
        {
            // Adjust based on your actual prefab structure
            var toggleUI = child.GetComponent<UnityEngine.UI.Toggle>();
            if (toggleUI != null && toggleUI.isOn)
            {
                foreach (Transform t in toggleUI.transform)
                {
                    Debug.Log("Toggle child: " + t.name);
                }
                Debug.Log("NOT NULL");
                // Assumes TMP_Text is a direct child or sibling of the ToggleUI
                TMP_Text effectText = toggleUI.GetComponentInChildren<TMP_Text>();
                if (effectText != null)
                {
                    string effect = effectText.text;
                    Debug.Log("effect:" + effect);
                    var effectWithX = new EffectWithX(effect, selectedX);

                    if (!existingEffects[selectedAction].Contains(effectWithX))
                    {
                        existingEffects[selectedAction].Add(effectWithX);
                        availableEffects[selectedAction].Remove(effect);
                    }
                }
            }
        }

        PopulateAvailableEffects();
        PopulateExistingEffects();
    }
    private void RemoveSelectedEffects()
    {
        string selectedAction = dropdownExistingActions.items[dropdownExistingActions.selectedItemIndex].itemName;

        var effectsToRemove = new List<EffectWithX>();

        foreach (Transform child in existingEffectsScrollView.content)
        {
            var toggle = child.GetComponent<UnityEngine.UI.Toggle>();
            if (toggle.isOn)
            {
                string effectText = child.GetComponentInChildren<TMP_Text>().text;

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
