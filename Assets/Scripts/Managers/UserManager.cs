using Steamworks;
using UnityEngine;
using Supabase;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.UI;
using System;

public class UserManager : MonoBehaviour
{
    public static UserManager Instance { get; private set; }
    public int userId {  get; private set; }
    [SerializeField] private GameObject LoginErrorPanel;
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private RawImage avatarImage;
    public Action onUserLogIn;
    public bool isAdmin { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Application.runInBackground = true;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public async void SimulateLogin()
    {
        
        if (SteamManager.Initialized)
        {
            string steamId = "1";
            Debug.Log("Logged in with Steam ID: " + steamId);

            // Ensure the user exists in the Supabase database
            var user = await SupabaseManager.Instance.EnsureSteamUserExists(steamId);
            
            if (user == null)
            {
                ShowError($"Failed to login. Please try restarting the game or check your connection. steamid:{steamId}");
                return;
            }
            userId = user.User_ID;
            isAdmin = user.Is_Admin;
            onUserLogIn?.Invoke();
            UpdateSteamProfile();
        } 
        else
        {
            ShowError("Steam is not initialized. Make sure you are running the game through Steam. (CHANGED)");
        }
    }
    async void Start()
    {
        if (SteamManager.Initialized)
        {
            string steamId = SteamUser.GetSteamID().ToString();
            Debug.Log("Logged in with Steam ID: " + steamId);

            // Ensure the user exists in the Supabase database
            var user = await SupabaseManager.Instance.EnsureSteamUserExists(steamId);
            
            if (user == null)
            {
                ShowError($"Failed to login. Please try restarting the game or check your connection. steamId: {steamId}");
                return;
            }
            userId = user.User_ID;
            isAdmin = user.Is_Admin;
            onUserLogIn?.Invoke();
            UpdateSteamProfile();
        }
        else
        {
            ShowError($"Steam is not initialized. Make sure you are running the game through Steam.(CHANGED)");
        }
    }

    private void ShowError(string message)
    {
        Debug.LogError(message);
        if (LoginErrorPanel != null)
        {
            LoginErrorPanel.SetActive(true);

            TMP_Text errorText = LoginErrorPanel.GetComponentInChildren<TMP_Text>();
            if (errorText != null)
            {
                errorText.text = message;
            }
        }
    }
    public int GetUserId()
    {
        return userId;
    }
    private void UpdateSteamProfile()
    {
        // Get the Steam username
        string username = SteamFriends.GetPersonaName();
        if (usernameText != null)
        {
            usernameText.text = username;
        }

        // Get the Steam avatar
        int avatarInt = SteamFriends.GetLargeFriendAvatar(SteamUser.GetSteamID());
        if (avatarInt != -1)
        {
            LoadAvatar(avatarInt);
        }
        else
        {
            Debug.LogWarning("Failed to get Steam avatar.");
        }
    }

    private void LoadAvatar(int avatarInt)
    {
        uint width, height;
        bool success = SteamUtils.GetImageSize(avatarInt, out width, out height);

        if (!success || width == 0 || height == 0)
        {
            Debug.LogWarning("Invalid avatar image size.");
            return;
        }

        byte[] imageData = new byte[4 * width * height];
        success = SteamUtils.GetImageRGBA(avatarInt, imageData, 4 * (int)width * (int)height);

        if (!success)
        {
            Debug.LogWarning("Failed to get avatar image data.");
            return;
        }

        Texture2D texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);

        Color32[] pixels = new Color32[width * height];
        for (uint y = 0; y < height; y++)
        {
            for (uint x = 0; x < width; x++)
            {
                int index = (int)((y * width) + x);
                int flippedIndex = (int)(((height - 1 - y) * width) + x); // Flip vertically

                byte r = imageData[index * 4 + 0];
                byte g = imageData[index * 4 + 1];
                byte b = imageData[index * 4 + 2];
                byte a = imageData[index * 4 + 3];

                pixels[flippedIndex] = new Color32(r, g, b, a);
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();

        if (avatarImage != null)
        {
            avatarImage.texture = texture;
        }
    }
}
