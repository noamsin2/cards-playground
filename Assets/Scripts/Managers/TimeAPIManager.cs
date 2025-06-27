using UnityEngine;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System;

public class TimeAPIManager : MonoBehaviour
{
    private static readonly string apiUrl = "https://timeapi.io/api/time/current/zone?timeZone=Israel"; // URL for UTC time from TimeAPI

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Method to fetch server time from TimeAPI
    public async Task<DateTime?> FetchServerTime()
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {

                // Fetch the response from the API
                string response = await client.GetStringAsync(apiUrl);

                // Parse the JSON response
                JObject jsonResponse = JObject.Parse(response);
                string utcTimeString = jsonResponse["dateTime"].ToString(); // Get the dateTime field from the response

                // Convert the UTC time to DateTime
                DateTime serverTime = DateTime.Parse(utcTimeString);
                Debug.Log($"Server Time (UTC): {serverTime}");
                return serverTime;
                // You can now use this server time in your game logic
            }
            catch (Exception ex)
            {
                // If there is an error, log it
                Debug.LogError($"An error occurred while fetching the server time: {ex.Message}");
                return null;
            }
        }
    }
}
