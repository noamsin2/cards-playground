using UnityEngine;
using UnityEngine.UI;
using Supabase;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

public class BrowseGamesPanel : MonoBehaviour
{
    [SerializeField] private GameObject gameButtonPrefab; // Prefab for each game entry
    [SerializeField] private Transform contentParent; // Parent of the ScrollView content
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private GameDetailsPanel gameDetailsPanel;
    private int currentPage = 0;
    private bool isLoading = false;

    void Start()
    {
        LoadGames();
        scrollRect.onValueChanged.AddListener(OnScroll);
    }

    async void LoadGames()
    {
        if (isLoading) return;
        isLoading = true;

        var response = await SupabaseManager.Instance.LoadGames(currentPage);

        foreach (var game in response)
        {
            GameObject button = Instantiate(gameButtonPrefab, contentParent);
            button.GetComponentInChildren<TMPro.TMP_Text>().text = game.Name;
            button.GetComponent<Button>().onClick.AddListener(() => gameDetailsPanel.ShowGameDetails(game));
        }

        currentPage++;
        isLoading = false;
    }

    void OnScroll(Vector2 scrollPosition)
    {
        if (scrollPosition.y <= 0.1f && !isLoading) // Near bottom
        {
            LoadGames();
        }
    }
}
