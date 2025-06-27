using UnityEngine;
using UnityEngine.UI;
using Supabase;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.InputSystem;
using Michsky.UI.Reach;
public class ManageGamesPanel : MonoBehaviour
{
    [SerializeField] private Michsky.UI.Reach.PanelButton gameButtonPrefab; // Prefab for each game entry
    [SerializeField] private Transform contentParent; // Parent of the ScrollView content
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private GameDetailsAdminPanel gameDetailsPanel;
    
    private int currentPage = 0;
    private bool isLoading = false;

    void Start()
    {
        LoadGames(true);
        scrollRect.onValueChanged.AddListener(OnScroll);
    }

    public async void LoadGames(bool isReset)
    {
        if (UserManager.Instance.isAdmin == false)
            return;
        Debug.Log("LOADGAMES");
        if (isLoading) return;
        isLoading = true;
        if (isReset)
        {
            currentPage = 0;
            foreach (Transform child in contentParent)
            {
                Destroy(child.gameObject);
            }
        }
        Debug.Log("isLoading false");
        var response = await SupabaseManager.Instance.LoadGamesAdmin(currentPage);

        foreach (var game in response)
        {
            Debug.Log("FOUND GAME: " + game.Name);
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
            LoadGames(false);
        }
    }

}
