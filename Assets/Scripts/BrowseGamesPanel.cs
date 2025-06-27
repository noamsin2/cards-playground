using UnityEngine;
using UnityEngine.UI;
using Supabase;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.InputSystem;
using Michsky.UI.Reach;
public class BrowseGamesPanel : MonoBehaviour
{
    [SerializeField] private Michsky.UI.Reach.PanelButton gameButtonPrefab; // Prefab for each game entry
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
            Michsky.UI.Reach.PanelButton button = Instantiate(gameButtonPrefab, contentParent);
            button.buttonText = game.Name;
            button.onClick.AddListener(() => gameDetailsPanel.ShowGameDetails(game));
            button.UpdateUI();
        }

        currentPage++;
        isLoading = false;
    }

    void OnScroll(Vector2 scrollPosition)
    {
        if (scrollPosition.y <= 0.01f && !isLoading)
        {
            LoadGames();
        }
    }

}
