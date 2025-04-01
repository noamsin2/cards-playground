using Steamworks;
using UnityEngine;
using Supabase;
using System.Threading.Tasks;

public class UserManager : MonoBehaviour
{
    public static UserManager Instance { get; private set; }
    private int userId;
    [SerializeField] private GameObject LoginErrorPanel;

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
    async void Start()
    {

        if (SteamManager.Initialized)
        {
            string steamId = SteamUser.GetSteamID().ToString();
            Debug.Log("Logged in with Steam ID: " + steamId);

            // Ensure the user exists in the Supabase database
            userId = await SupabaseManager.Instance.EnsureSteamUserExists(steamId);
            if(userId == -1)
            {
                LoginErrorPanel.SetActive(true);
            }
        }
        else
        {
            Debug.LogError("Steam is not initialized.");
            LoginErrorPanel.SetActive(true);
        }
    }
    public int GetUserId()
    {
        return userId;
    }
}
