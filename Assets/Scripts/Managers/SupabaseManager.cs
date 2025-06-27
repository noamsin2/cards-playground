using Cysharp.Threading.Tasks;
using UnityEngine;
using Supabase;
using System;
using Client = Supabase.Client;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Models;
using System.Linq;
using NUnit.Framework;
using Steamworks;
using Microsoft.IdentityModel.Tokens;
using UnityEngine.Windows;
using static EffectsPanel;
using UnityEngine.Networking;
using Supabase.Interfaces;
using System.Net.Http;
using Supabase.Storage;
using System.Xml.Linq;
using UnityEngine.InputSystem;
using static Postgrest.Constants;
using UnityEngine.Rendering;
using Supabase.Realtime;
using Supabase.Realtime.PostgresChanges;
using static Supabase.Realtime.PostgresChanges.PostgresChangesOptions;
using Supabase.Gotrue;
using Newtonsoft.Json;
using Unity.VisualScripting;
using Newtonsoft.Json.Linq;
using Supabase.Realtime.Models;
using System.Threading.Channels;
using System.Threading;
using Postgrest;
public class SupabaseManager : MonoBehaviour
{
    public static SupabaseManager Instance { get; private set; }
    [SerializeField] private TimeAPIManager timeAPIManager;
    public Client client { get; private set; }


    public event Action<int> OnGameStart; // int = playerIndex
    public event Action<MatchPlayers> onOpponentUpdate;
    public event Action<int> OnChangeTurn;
    public event Action OnLogout;
    public event Action<int> OnUpdate;
    //private string supabaseUrl;
    //private string apiKey;
    public const string CARD_BUCKET = "card-bucket";
    public const string PUBLISHED_CARD_BUCKET = "published-card-bucket";
    //public CardDisplayManager cardDisplayManager;
    private async void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            // Load Supabase credentials from environment variables or hardcode for testing

            TextAsset configText = Resources.Load<TextAsset>("supabase_config");
            if (configText == null)
            {
                Debug.LogError("Missing supabase_config.json in Resources folder!");
                return;
            }
            SupabaseConfig config = JsonUtility.FromJson<SupabaseConfig>(configText.text);
            
            // Initialize Supabase client
            var options = new Supabase.SupabaseOptions
            {
                AutoConnectRealtime = true,
            };
            client = new Supabase.Client(config.url, config.key, options);
            await client.InitializeAsync();
            Debug.Log("Supabase client initialized!");
        }
        else
        {
            Destroy(gameObject);
        }

        //await DeleteAllFilesInBucket(CARD_BUCKET);
    }

    // <-----------------------------------Users------------------------------------------>
    public async Task<Users> EnsureSteamUserExists(string steamId)
    {
        try
        {

            var response = await client
             .From<Users>()
             .Filter("steam_id", Postgrest.Constants.Operator.Equals, steamId)
             .Get();


            if (response.Models.Count > 0)
            {
                Debug.Log("Steam user already exists.");
                return response.Models[0];
            }

            // Create a new user
            var newUser = new Users { Steam_ID = steamId };
            var insertResponse = await client
                .From<Users>()
                .Insert(newUser);

            if (insertResponse.ResponseMessage.IsSuccessStatusCode && insertResponse.Models.Count > 0)
            {
                Debug.Log("New Steam user created successfully!");
                return insertResponse.Models[0];
            }
            else
            {
                Debug.LogError($"Failed to create Steam user: {insertResponse.ResponseMessage.ReasonPhrase}");
                return null;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Exception while checking/creating Steam user: {e.Message}");
            return null;
        }

    }

    // <-----------------------------------Card Game------------------------------------------>
    public async Task<CardGames> AddGameToDatabase(string name, string jsonSettings)
    {
        try
        {
            int userId = UserManager.Instance.GetUserId();
            var newGame = new CardGames
            {
                Name = name,
                FK_User_ID = userId,
                Game_Settings = jsonSettings
            };
            var response = await client.From<CardGames>().Insert(newGame);
            return response.Models[0];
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Couldn't make a game: {e.Message}");
            return null;
        }
    }
    public async Task<CardGames> EditGameSettingsInDatabase(int gameId, string name, string jsonSettings)
    {
        try
        {
            var updateResponse = await client
                    .From<CardGames>()
                    .Where(cg => cg.Game_ID == gameId)
                    .Set(cg => cg.Game_Settings, jsonSettings)
                    .Update();

            return updateResponse.Models[0];
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Couldn't make a game: {e.Message}");
            return null;
        }
    }
    public async Task<List<CardGames>> RetrieveAllGames()
    {
        try
        {
            // Perform the database query and get the response
            int userId = UserManager.Instance.GetUserId();
            Debug.Log(userId);
            var response = await client.From<CardGames>()
                .Filter("fk_user_id", Postgrest.Constants.Operator.Equals, userId)
                .Filter("is_deleted", Postgrest.Constants.Operator.Equals, "FALSE")
                .Get();

            // Check if the response is successful and return the models (the list of cards)
            if (response.ResponseMessage.IsSuccessStatusCode)
            {
                Debug.Log("retrieved list: " + response.Models.Count);
                return response.Models.ToList();  // Convert to List<CardGames>
            }
            else
            {
                Debug.LogError($"Error fetching games: {response.ResponseMessage.ReasonPhrase}");
                return new List<CardGames>();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Exception while fetching games: {e.Message}");
            return new List<CardGames>();
        }
    }
    public async Task<List<CardGames>> RetrieveDeletedGames()
    {
        try
        {
            // Perform the database query and get the response
            int userId = UserManager.Instance.GetUserId();
            Debug.Log(userId);
            var response = await client.From<CardGames>()
                .Filter("fk_user_id", Postgrest.Constants.Operator.Equals, userId)
                .Filter("is_deleted", Postgrest.Constants.Operator.Equals, "TRUE")
                .Get();

            // Check if the response is successful and return the models (the list of cards)
            if (response.ResponseMessage.IsSuccessStatusCode)
            {
                Debug.Log("retrieved list: " + response.Models.Count);
                return response.Models.ToList();  // Convert to List<CardGames>
            }
            else
            {
                Debug.LogError($"Error fetching games: {response.ResponseMessage.ReasonPhrase}");
                return new List<CardGames>();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Exception while fetching games: {e.Message}");
            return new List<CardGames>();
        }
    }
    public async Task<bool> SetGameToDeleted(CardGames cardGame)
    {
        try
        {
            if (cardGame != null)
            {
                var updateResponse = await client
                    .From<CardGames>()
                    .Where(cg => cg.Game_ID == cardGame.Game_ID)
                    .Set(cg => cg.Is_Deleted, true)
                    .Set(cg => cg.Is_Published, false)
                    .Update();

                if (updateResponse.Models.Count == 0)
                {
                    Debug.LogError("Failed to update the game in the database.");
                    return false;
                }
                var publishedGameResponse = await client
                .From<PublishedGames>()
                .Where(pg => pg.Game_ID == cardGame.Game_ID)
                .Get();

                if (publishedGameResponse.Models.Count > 0)
                {
                    // Set Is_Published to false in PublishedGames
                    var updatePublished = await client
                        .From<PublishedGames>()
                        .Where(pg => pg.Game_ID == cardGame.Game_ID)
                        .Set(pg => pg.Is_Published, false)
                        .Update();

                    if (updatePublished.Models.Count == 0)
                    {
                        Debug.LogWarning("Published game found but failed to update Is_Published to false.");
                    }
                    else
                    {
                        Debug.Log("Published game Is_Published set to false.");
                    }
                    Debug.Log("Game 'is_deleted' updated to true (Deleted_At handled by DB).");
                    return true;
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"An error occurred while editing the game: {ex.Message}");
            return false;
        }

        return true;
    }
    public async Task DeleteGameFromDB(CardGames cardGame)
    {
        try
        {
            if (cardGame != null)
            {
                await client.From<CardGames>()
                    .Where(cg => cg.Game_ID == cardGame.Game_ID)
                    .Delete();

             
                Debug.Log("Game was deleted.");
            }

        }
        catch (System.Exception ex)
        {
            Debug.LogError($"An error occurred while editing the game: {ex.Message}");
        }
    }
    public async Task<bool> RestoreGame(CardGames cardGame)
    {
        try
        {
            if (cardGame != null)
            {
                var updateResponse = await client
                    .From<CardGames>()
                    .Where(cg => cg.Game_ID == cardGame.Game_ID)
                    .Set(cg => cg.Is_Deleted, false)
                    .Set(cg => cg.Deleted_At, (DateTime?)null)
                    .Update();

                if (updateResponse.Models.Count == 0)
                {
                    Debug.LogError("Failed to update the game in the database.");
                    return false;
                }

                Debug.Log("Game 'is_deleted' updated to false.");
                return true;
            }

        }
        catch (System.Exception ex)
        {
            Debug.LogError($"An error occurred while editing the game: {ex.Message}");
            return false;
        }
        //if there's nothing to change
        return true;
    }
    class GameBroadcast : BaseBroadcast
    {
        [JsonProperty("game_id")]
        public int GameId { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }
    }
    public async Task SubscribeToGameEvents(int gameId)
    {
        var channel = client.Realtime.Channel("logout");

        var broadcast = channel.Register<GameBroadcast>();

        broadcast.AddBroadcastEventHandler((sender, baseBroadcast) =>
        {
            var raw = broadcast.Current();

            // Convert the whole broadcast to a JSON string
            var json = JsonConvert.SerializeObject(raw);

            // Extract the payload dictionary and deserialize it
            if (raw.Payload is Dictionary<string, object> payloadDict)
            {
                var payloadJson = JsonConvert.SerializeObject(payloadDict);
                var data = JsonConvert.DeserializeObject<GameBroadcast>(payloadJson);
                Debug.Log(data.ToString());
                if (data.GameId == gameId)
                {
                    if (data.Reason == "logout")
                    {
                        Debug.Log($"Received logout broadcast: {data.Reason}");
                        OnLogout?.Invoke();
                    }
                    else if (data.Reason == "updated")
                    {
                        Debug.Log($"Received updated broadcast: {data.Reason}");
                        OnUpdate?.Invoke(gameId);
                    }
                }
            }
            else
            {
                Debug.LogWarning("Unexpected payload format.");
            }
        });

        await channel.Subscribe();


    }
    public async Task<DateTime?> GetPublishedGameUpdatedAt(int gameId)
    {
        var updateResponse = await client
                    .From<PublishedGames>()
                    .Where(pg => pg.Game_ID == gameId).Get();
        if (updateResponse.Models.Count > 0)
        {
            return updateResponse.Models[0].Updated_At;
        }
        return null;
    }
    public async Task<bool> SendGameBroadcast(int gameId, string reason, int timeoutMs = 2000)
    {
        var channel = client.Realtime.Channel("logout");

        await channel.Subscribe();
        var wrappedPayload = new
        {
            Payload = new
            {
                game_id = gameId,
                reason = reason
            }
        };
        var payload = new
        {
            game_id = gameId,
            reason = reason
        };

        var sendTask = channel.Send(
            Supabase.Realtime.Constants.ChannelEventName.Broadcast,
            "logout",
            wrappedPayload,
            timeoutMs
        );

        var timeoutTask = Task.Delay(timeoutMs);

        var completedTask = await Task.WhenAny(sendTask, timeoutTask);

        if (completedTask == sendTask)
        {
            // The sendTask completed first
            try
            {
                bool success = await sendTask;  // Await again to propagate exceptions if any
                Debug.Log("Broadcast sent? " + success);
                return success;
            }
            catch (Exception ex)
            {
                Debug.LogError("Exception sending broadcast: " + ex);
                return false;
            }
        }
        else
        {
            // Timeout happened
            Debug.LogError("Broadcast send timed out.");
            return false;
        }
    }
   
    public void UnsubscribeFromGameEvents()
    {
        var channel = client.Realtime.Channel("logout");
        client.Realtime.Remove(channel);
    }
    public async Task<bool> PublishGame(CardGames cardGame)
    {
        try
        {
            if (cardGame == null)
            {
                Debug.LogError("Game not found.");
                return false;
            }

            // STEP 1: Fetch current publish state from DB
            var currentState = await client
                .From<CardGames>()
                .Where(cg => cg.Game_ID == cardGame.Game_ID)
                .Single();

            //bool wasAlreadyPublished = currentState?.Is_Published == true;

            // STEP 2: Set Is_Published to true (if not already)
            var updateResponse = await client
                .From<CardGames>()
                .Where(cg => cg.Game_ID == cardGame.Game_ID)
                .Set(cg => cg.Is_Published, true)
                .Update();

            //if (wasAlreadyPublished)
            //{
            //    // STEP 3: Schedule delayed update via Edge Function
            //    Debug.Log("Game was already published. Updating timestamp only.");

            //    var updatePublishedGame = await client
            //        .From<PublishedGames>()
            //        .Where(pg => pg.Game_ID == cardGame.Game_ID)
            //        .Set(pg => pg.Is_Published, true)
            //        .Update();

            //    if (updatePublishedGame.Models.Count == 0)
            //    {
            //        Debug.LogError("Failed to update published game's timestamp.");
            //        return false;
            //    }

            //    return true;
            //}

            // STEP 4: Proceed with initial publish
            var publishedGame = await client
                .From<PublishedGames>()
                .Where(cg => cg.Game_ID == cardGame.Game_ID).Get();

            int gamesPlayed = publishedGame?.Models.FirstOrDefault()?.Games_Played ?? 0;

            var publishGameResponse = await client.From<PublishedGames>().Upsert(new PublishedGames
            {
                Game_ID = cardGame.Game_ID,
                Name = cardGame.Name,
                Description = cardGame.Description,
                FK_User_ID = cardGame.FK_User_ID,
                Is_Deleted = cardGame.Is_Deleted,
                Deleted_At = cardGame.Deleted_At,
                Game_Settings = cardGame.Game_Settings,
                Games_Played = gamesPlayed,
                Is_Published = true
            });

            if (publishGameResponse.Models.Count == 0)
            {
                Debug.LogError("Failed to publish game.");
                return false;
            }

            var cards = await client.From<Cards>().Where(c => c.FK_Game_ID == cardGame.Game_ID).Get();

            foreach (var card in cards.Models)
            {
                // If the image has changed update it in storage
                if (card.Is_Image_Changed == true)
                {
                    Debug.Log($"CARD {card.Name} was changed");
                    string fileExtension = Path.GetExtension(card.Image_URL);
                    byte[] downloadedFile = await DownloadFileFromSupabase(CARD_BUCKET, card.Image_URL);

                    string fileNameWithExtension = card.Image_URL;
                    if (downloadedFile != null)
                    {
                        await UploadFileToSupabase(PUBLISHED_CARD_BUCKET, fileNameWithExtension, downloadedFile);
                    }
                    else
                    {
                        Debug.LogError("Failed to download an image while publishing\n");
                    }
                }
                card.Is_Image_Changed = false;
                await client.From<Cards>().Update(card);
                await client.From<PublishedCards>().Upsert(new PublishedCards
                {
                    Card_ID = card.Card_ID,
                    FK_Game_ID = card.FK_Game_ID,
                    Name = card.Name,
                    Image_URL = card.Image_URL
                });

                var cardEffects = await client.From<CardEffects>()
                    .Where(ce => ce.FK_Card_ID == card.Card_ID)
                    .Get();

                foreach (var effect in cardEffects.Models)
                {
                    await client.From<PublishedCardEffects>().Upsert(new PublishedCardEffects
                    {
                        Effect = effect.Effect,
                        FK_Card_ID = effect.FK_Card_ID,
                        Action = effect.Action,
                        X = effect.X
                    });
                }
                
            }
            // Remove cards that were deleted
            var publishedCards = await client.From<PublishedCards>()
            .Where(pc => pc.FK_Game_ID == cardGame.Game_ID)
            .Get();
            var currentCardIds = new HashSet<int>(cards.Models.Select(c => c.Card_ID));
            foreach (var publishedCard in publishedCards.Models)
            {
                if (!currentCardIds.Contains(publishedCard.Card_ID))
                {
                    string filePath = publishedCard.Image_URL;
                    // Delete the published card from DB
                    await client.From<PublishedCards>()
                        .Where(pc => pc.Card_ID == publishedCard.Card_ID)
                        .Delete();

                    // Also delete any effects tied to it
                    //await client.From<PublishedCardEffects>()
                    //    .Where(pce => pce.FK_Card_ID == publishedCard.Card_ID)
                    //    .Delete();

                    // Delete the file from Supabase Storage
                    var deleteResult = await DeleteCardFromStorage(filePath, PUBLISHED_CARD_BUCKET);
                    if (!deleteResult)
                    {
                        Debug.LogWarning($"Failed to delete image: {publishedCard.Image_URL}");
                    }
                }
            }


            await SendGameBroadcast(cardGame.Game_ID, "updated");
            Debug.Log("Game, cards, and card effects successfully published.");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error publishing game: {ex.Message}");
            await client
                .From<CardGames>()
                .Where(cg => cg.Game_ID == cardGame.Game_ID)
                .Set(cg => cg.Is_Published, false)
                .Update();
            await client.From<PublishedGames>().Where(pcg => pcg.Game_ID == cardGame.Game_ID).Delete();
            return false;
        }
    }

    public async Task<PublishedGames> GetPublishedGame(int gameId)
    {
        try
        {
            var publishedGame = await client
               .From<PublishedGames>()
               .Where(cg => cg.Game_ID == gameId).Get();
            if (publishedGame.Models.Count > 0)
                return publishedGame.Models[0];
        }
        catch (Exception ex)
        {
            Debug.Log($"Couldn't find the published game: {ex.Message}");
        }
        return null;
    }
    public async Task<bool> UnpublishGame(CardGames cardGame)
    {
        try
        {
            if (cardGame != null)
            {
                var updateResponse = await client
                    .From<CardGames>()
                    .Where(cg => cg.Game_ID == cardGame.Game_ID)
                    .Set(cg => cg.Is_Published, false)
                    .Update();
                var updateResponse2 = await client
                    .From<PublishedGames>()
                    .Where(pg => pg.Game_ID == cardGame.Game_ID)
                    .Set(pg => pg.Is_Published, false)
                    .Update();

                if (updateResponse.Models.Count == 0 || updateResponse2.Models.Count == 0)
                {
                    Debug.LogError("Failed to update the game in the database.");
                    return false;
                }
                await SendGameBroadcast(cardGame.Game_ID, "logout");
                Debug.Log("Game 'is_published' updated to false.");
                return true;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"An error occurred while editing the game: {ex.Message}");
            return false;
        }
        //if there's nothing to change
        return true;
    }

    public async Task<long> GetCardGamesCount()
    {
        try
        {
            // Call the RPC and get the count directly
            long count = await client.Rpc<long>("get_card_games_count", null);
            Debug.Log($"Count of rows in card_games: {count}");
            return count;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error calling RPC: {e.Message}");
        }
        return -1;
    }
    public async Task<long> GetTotalPlayers()
    {
        try
        {
            long uniqueUserCount = await client.Rpc<long>("get_unique_user_count", null);
            Debug.Log($"Count of unique fk_user_id in game_players: {uniqueUserCount}");
            return uniqueUserCount;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error calling RPC: {e.Message}");
        }
        return -1;
    } 
    public async Task<long> GetTotalUsers()
    {
        try
        {
            long userCount = await client.Rpc<long>("get_users_count", null);
            Debug.Log($"Users count: {userCount}");
            return userCount;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error calling RPC: {e.Message}");
        }
        return -1;
    }
    public async Task<long> GetTotalGameCreators()
    {
        try
        {
            long uniqueUserCount = await client.Rpc<long>("get_unique_fk_user_id_count_in_card_games", null);
            Debug.Log($"Count of unique fk_user_id in card_games: {uniqueUserCount}");
            return uniqueUserCount;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error calling RPC: {e.Message}");
        }
        return -1;
    }
    public async Task<long> GetTotalDecks()
    {
        try
        {
            long decksCount = await client.Rpc<long>("get_decks_count", null);
            Debug.Log($"Decks count: {decksCount}");
            return decksCount;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error calling RPC: {e.Message}");
        }
        return -1;
    }
    public async Task<long> GetTotalCards()
    {
        try
        {
            long cardsCount = await client.Rpc<long>("get_cards_count", null);
            Debug.Log($"Cards count: {cardsCount}");
            return cardsCount;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error calling RPC: {e.Message}");
        }
        return -1;
    }
    // <-----------------------------------CARDS------------------------------------------>
    public async Task UploadCard(string filePath, string name, int gameId, Dictionary<string, List<EffectWithX>> cardEffects)
    {
        string fileUrl = "";
        Cards card = null;
        if (string.IsNullOrEmpty(filePath))
        {
            Debug.LogError("File path is null or empty!");
            return;
        }
        string fileName = Path.GetFileName(filePath);
        string fileExtension = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(fileExtension))
        {
            // If no extension, you can assign one or handle the error.
            fileExtension = ".jpg";  // Default extension or handle accordingly
        }
        try
        {

            // Add card to the database
            
            card = await AddCardToDatabase(name, gameId);
            string fileNameWithExtension = $"game_{gameId}/{card.Card_ID}{fileExtension}";
            Debug.Log("Uploading with path: " + fileNameWithExtension);

            if (card == null)
            {
                Debug.LogWarning("Database insertion failed.");
            }
            else
            {
                fileUrl = await AddCardToStorage(filePath, fileNameWithExtension, CARD_BUCKET);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("An error occurred while uploading the file: " + e.Message);
        }
        if (fileUrl.IsNullOrEmpty() && card != null)
        {
            await DeleteCardFromDatabase(card.Card_ID);
        }
        else if (!fileUrl.IsNullOrEmpty() && card != null)
        {
            card.Image_URL = fileUrl;
            try
            {
                var update = await client
                  .From<Cards>()
                  .Where(c => c.Card_ID == card.Card_ID)
                  .Set(c => c.Image_URL, fileUrl)
                  .Update();

                await AddCardEffectsToDatabase(cardEffects, card.Card_ID);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to update card with file URL: {e.Message}");
                await DeleteCardFromDatabase(card.Card_ID);
                await DeleteCardFromStorage(fileUrl,CARD_BUCKET);
            }

        }
    }

    // might need to give an option to overwrite a file
    private async Task<string> AddCardToStorage(string filePath, string file_name, string bucket)
    {
        //filePath is the local file path and file_name is the card_id with the extention for eg: 1.jpg

        byte[] fileBytes;
        try
        {
            fileBytes = System.IO.File.ReadAllBytes(filePath);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to read file: " + e.Message);
            return "";
        }
        try
        {

            string fileUrl = await client.Storage
                .From(bucket)
                .Upload(fileBytes, file_name);

            if (string.IsNullOrEmpty(fileUrl))
            {
                Debug.LogError("File upload failed: No URL returned.");
                return "";
            }
            Debug.Log($"File uploaded successfully! URL: {fileUrl}");
            Debug.Log($"File '{file_name}' uploaded successfully and database updated.");
            string result = fileUrl.Replace(bucket + "/", "");
            return result;

        }
        catch (Exception e)
        {
            Debug.LogError("An error occurred while uploading the file: " + e.Message);
            return "";
        }
    }
    private async Task<bool> DeleteCardFromStorage(string filePath, string bucket)
    {
        if(await DeleteFileFromStorage(filePath, bucket))
        {
            return true;
        }
        return false;
    }
    private async Task<Cards> AddCardToDatabase(string name, int fk_game_id)
    {
        try
        {
            var cardToInsert = new Cards
            {
                Name = name,
                FK_Game_ID = fk_game_id,
                Is_Image_Changed = true
            };

            var response = await client.From<Cards>()
                .Insert(cardToInsert);
            return response.Models[0];
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to add card to database: {e.Message}");
            return null;
        }
    }
    public async Task<bool> EditCardOnStorage(Cards card, string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            Debug.LogError("File path is null or empty!");
            return false;
        }
        if (!System.IO.File.Exists(filePath))
        {
            Debug.LogError($"Local file does not exist at path: {filePath}");
            return false;
        }
        string folderPrefix = $"game_{card.FK_Game_ID}/";
        var files = await client.Storage.From(CARD_BUCKET).List(folderPrefix);

        bool fileExists = files.Any(file => file.Name == Path.GetFileName(card.Image_URL));
        Debug.Log("Image_URL: " + card.Image_URL);
      
        string fileName = Path.GetFileName(filePath);
        string fileExtension = Path.GetExtension(fileName);
        
        if (!fileExists)
        {
            Debug.LogError($"File '{card.Image_URL}' does not exist in Supabase storage.");
            return false;
        }

        if (string.IsNullOrEmpty(fileExtension))
        {
            return false;
        }
        try
        {
            //string fileNameWithExtension = $"game_{card.FK_Game_ID}/{card.Card_ID}{fileExtension}";

            if (await DeleteCardFromStorage(card.Image_URL, CARD_BUCKET))
            {
                string fileUrl = await AddCardToStorage(filePath, card.Image_URL, CARD_BUCKET);
                //card.Is_Image_Changed = true;
                //if (!string.IsNullOrEmpty(fileUrl))
                //{
                //    await EditCardOnDatabase(card, card.Name, fileUrl, true);
                //}
                
            }
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("An error occurred while uploading the file: " + e.Message);
            return false;
        }

    }
    public async Task<bool> EditCardOnDatabase(Cards card, string newName, string newImageUrl, bool isImageUpdated)
    {
        
        try
        {
            if (card.Name != newName || isImageUpdated)
            {
                Debug.Log("isImageUpdated: "+isImageUpdated);
                var updateResponse = await client
                    .From<Cards>()
                    .Where(c => c.Card_ID == card.Card_ID)
                    .Set(c => c.Name, newName)
                    .Set(c => c.Is_Image_Changed, isImageUpdated)
                    .Update();

                if (updateResponse.Models.Count == 0)
                {
                    Debug.LogError("Failed to update card's name in the database.");
                    return false;
                }
                
                Debug.Log("Card details updated in the database.");
                return true;
            }
            if (card.Image_URL != newImageUrl)
            {
                var updateResponse = await client
                    .From<Cards>()
                    .Where(c => c.Card_ID == card.Card_ID)
                    .Set(c => c.Image_URL, newImageUrl)
                    .Update();

                if (updateResponse.Models.Count == 0)
                {
                    Debug.LogError("Failed to update card's image url in the database.");
                    return false;
                }

                Debug.Log("Card details updated in the database.");
                return true;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"An error occurred while editing the card: {ex.Message}");
            return false;
        }
        //if there's nothing to change
        return true;
    }
    public async Task<bool> DeleteCard(Cards card)
    {
        bool isDeleted = await DeleteCardFromDatabase(card.Card_ID);
        if (isDeleted)
        {
            if (await DeleteFileFromStorage(card.Image_URL, CARD_BUCKET))
            {
                return true;
            }

        }
        return false;
    }
    private async Task<bool> DeleteCardFromDatabase(int card_id)
    {
        try
        {
            await client.From<Cards>().Filter("card_id", Postgrest.Constants.Operator.Equals, card_id).Delete();
            Debug.Log("Card deleted successfully.");
            return true;

        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to delete a card from database: {e.Message}");
            return false;
        }
    }
    
    
    public async Task<List<Cards>> GetCardsFromDatabase(CardGames game)
    {
        try
        {
            // Perform the database query and get the response
            var response = await client.From<Cards>().Filter("fk_game_id", Postgrest.Constants.Operator.Equals, game.Game_ID).Get();
            print("retrieved cards list:" + response.Models.Count());
            // Check if the response is successful and return the models (the list of cards)
            if (response.ResponseMessage.IsSuccessStatusCode)
            {
                return response.Models.ToList();  // Convert to List<Cards>
            }
            else
            {
                Debug.LogError($"Error fetching card data: {response.ResponseMessage.ReasonPhrase}");
                return null;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Exception while fetching card data: {e.Message}");
            return null;
        }
    }
    public string GetCardUrl(string bucket,string filePath)
    {
        return GetFileUrl(bucket, filePath);
    }


    public async Task DeleteAllFilesInBucket(string bucketName)
    {
        try
        {
            // Retrieve all files from the bucket
            var files = await client.Storage
                .From(bucketName)
                .List();

            if (files == null || files.Count == 0)
            {
                Debug.Log($"No files found in the bucket '{bucketName}'.");
                return;
            }

            // Collect all file names
            List<string> fileNames = new List<string>();
            foreach (var file in files)
            {
                fileNames.Add(file.Name);
            }

            // Delete all files
            var result = await client.Storage
                .From(bucketName)
                .Remove(fileNames);

            if (result != null && result.Count > 0)
            {
                Debug.Log($"Successfully deleted {result.Count} files from the bucket '{bucketName}'.");
            }
            else
            {
                Debug.LogError("Failed to delete some or all files.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"An error occurred while deleting files: {e.Message}");
        }
    }

    private async Task<string> UploadFileToSupabase(string bucketName, string filePath, byte[] fileData)
    {
        try
        {
            // Check if the file already exists in the bucket
            string prefix = Path.GetDirectoryName(filePath)?.Replace("\\", "/") ?? "";
            // List files in that folder (this acts as the prefix)
            var existingFiles = await client.Storage.From(bucketName).List(prefix);
            // Check if the file already exists
            bool fileExists = existingFiles.Any(f => f.Name == Path.GetFileName(filePath));
            Debug.Log($"Attempting to delete: bucket = {bucketName}, path = {filePath}");

            Debug.Log(Path.GetFileName(filePath));
            if (fileExists)
            {
                Debug.Log($"File {filePath} already exists. Deleting...");
                await DeleteCardFromStorage(filePath, bucketName);
                //await client.Storage.From(bucketName).Remove(new List<string> { filePath });
            }

            // Upload the new file
            await client.Storage.From(bucketName).Upload(fileData, filePath);

            // Get and return the public URL of the uploaded file
            return client.Storage.From(bucketName).GetPublicUrl(filePath);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error uploading file: {ex.Message}");
            return null;
        }
    }

    // <-----------------------------------CARD EFFECTS------------------------------------------>

    private async Task AddCardEffectsToDatabase(Dictionary<string, List<EffectWithX>> cardEffects, int cardId)
    {
        if (cardEffects == null || cardEffects.Count == 0)
        {
            Debug.LogError("No effects to upload.");
            return;
        }

        var effectsPayload = new List<CardEffects>();

        foreach (var kvp in cardEffects)
        {
            string action = kvp.Key;
            List<EffectWithX> effectsList = kvp.Value;

            Debug.Log($"Action Key: {action}");
            foreach (var effectWithX in effectsList)
            {
                Debug.Log($"Effect: {effectWithX.Effect}, X: {effectWithX.X}");
            }

            // Skip invalid or empty action keys
            if (string.IsNullOrEmpty(action))
            {
                Debug.LogError("Invalid action key. Skipping...");
                continue;
            }

            if (effectsList == null || effectsList.Count == 0)
            {
                Debug.LogWarning($"No effects found for action: {action}. Skipping...");
                continue;
            }

            foreach (var effectWithX in effectsList)
            {
                if (string.IsNullOrEmpty(effectWithX.Effect) || string.IsNullOrEmpty(effectWithX.X))
                {
                    Debug.LogError($"Invalid effect or X for action '{action}'. Skipping...");
                    continue;
                }

                effectsPayload.Add(new CardEffects
                {
                    FK_Card_ID = cardId,
                    Effect = effectWithX.Effect,
                    Action = action,
                    X = int.Parse(effectWithX.X)
                });
            }
        }

        if (effectsPayload.Count == 0)
        {
            Debug.LogError("No valid effects to insert into the database.");
            return;
        }

        try
        {
            foreach (var payload in effectsPayload)
            {
                Debug.Log($"Prepared Payload: FK_Card_ID={payload.FK_Card_ID}, Action={payload.Action}, Effect={payload.Effect}, X={payload.X}");

                Debug.Log($"Uploading {effectsPayload.Count} card effects...");

                var response = await client.From<CardEffects>().Insert(payload);

                if (response.ResponseMessage.IsSuccessStatusCode)
                {
                    Debug.Log("Card effects uploaded successfully.");
                }
                else
                {
                    Debug.LogError($"Failed to upload card effects: {response.ResponseMessage.ReasonPhrase}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error while uploading card effects: {e.Message}");
        }
    }
    public async Task EditCardEffectsInDatabase(Dictionary<string, List<EffectsPanel.EffectWithX>> updatedEffects, int cardId)
    {
        try
        {
            // Remove old effects for the card
            await client.From<CardEffects>().Where(c => c.FK_Card_ID == cardId).Delete();
            await AddCardEffectsToDatabase(updatedEffects, cardId);
           
            Debug.Log("Effects updated in database successfully.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error updating effects in database: {ex.Message}");
        }
    }


    public async Task<Dictionary<string, List<EffectWithX>>> GetEffectsFromDatabase(int cardId)
    {
        try
        {

            var response = await client
           .From<CardEffects>()
           .Filter("fk_card_id", Postgrest.Constants.Operator.Equals, cardId)
           .Get();

            if (response.Models == null || response.Models.Count == 0)
            {
                Debug.LogWarning($"No effects found for card ID {cardId} in the database.");
                return null;
            }

            var effectDictionary = response.Models
                .GroupBy(e => e.Action)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(e => new EffectWithX(e.Effect, e.X.ToString())).ToList()
                );

            return effectDictionary;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error fetching effects for card ID {cardId} from database: {ex.Message}");
            return null;
        }
    }
    // <-----------------------------------STORAGE------------------------------------------>
    private string GetFileUrl(string bucketName, string filePath)
    {
        try
        {
            var storage = client.Storage;
            var bucket = storage.From(bucketName);

            string publicUrl = bucket.GetPublicUrl(filePath);
            //Debug.Log(filePath);
            //Debug.Log(publicUrl);
            return publicUrl;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error retrieving file URL: {e.Message}");
            return null;
        }
    }

    private async Task<bool> DeleteFileFromStorage(string fileUrl, string bucketName)
    {
        try
        {
            Debug.Log($"Deleting from bucket: {bucketName}, path: {fileUrl}");

            //string card_id_str = card_id.ToString();
            var result = await client.Storage
            .From(bucketName)
            .Remove(new List<string> { fileUrl });

            if (result != null)
            {
                Debug.Log($"File '{fileUrl}' successfully deleted.");
                return true;
            }
            else
            {
                Debug.LogError($"Failed to delete file '{fileUrl}'. The file might not exist or the operation failed.");
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"An error occurred while deleting the file: {e.Message}");
            return false;
        }
    }
    // <-----------------------------------GAMES BROWSE------------------------------------------>
    public async Task<List<PublishedGames>> LoadGames(int currentPage)
    {
        var response = await client.From<PublishedGames>()
           .Where(pg => pg.Is_Published == true)
           .Order("games_played", Ordering.Descending)
           .Range(currentPage * 10, (currentPage + 1) * 10 - 1)
           .Get();
        return response.Models;
    }
    public async Task<List<CardGames>> LoadGamesAdmin(int currentPage)
    {
        var response = await client.From<CardGames>()
           .Order("name", Ordering.Ascending)
           .Range(currentPage * 10, (currentPage + 1) * 10 - 1)
           .Get();
        return response.Models;
    }
    // <-----------------------------------GAMES PLAY------------------------------------------>
    public async Task<PublishedGames> GetGameFromDatabase(int gameID)
    {
        var response = await client.From<PublishedGames>().Where(pcg => pcg.Game_ID == gameID).Get();
        return response.Models[0];
    }
    public async Task<List<PublishedCards>> GetAllCardsInGame(int gameID)
    {
        var response = await client.From<PublishedCards>()
            .Filter("fk_game_id", Postgrest.Constants.Operator.Equals, gameID)
            .Get();
        
        return response.Models;
    }
    public async Task<List<PublishedCardEffects>> GetAllCardEffects(int cardID)
    {
        var response = await client.From<PublishedCardEffects>()
            .Filter("fk_card_id", Postgrest.Constants.Operator.Equals, cardID)
            .Get();

        return response.Models;
    }
    //public async Task<Decks> AddDeckToDatabase(Decks deck)
    //{
    //    if (deck.Deck_ID == 0)
    //    {
    //        deck.Deck_ID = null;  // Let PostgreSQL generate it
    //    }
    //    try
    //    {
    //        var response = await client
    //            .From<Decks>()
    //            .Upsert(deck);

    //        Debug.Log($"Deck {deck.Deck_ID} added/updated successfully.");
    //        return response.Models[0];
    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.LogError($"Error upserting deck: {ex.Message}");
    //        return null;
    //    }
    //}
    public async Task<Decks> AddDeckToDatabase(Decks deck)
    {
        try
        {
            var response = await client
                .From<Decks>()
                .Insert(new Decks
                {
                    Name = deck.Name,
                    FK_Game_ID = deck.FK_Game_ID,
                    FK_User_ID = deck.FK_User_ID,
                    Card_IDs = deck.Card_IDs,
                });

            Debug.Log($"Deck {response.Models[0].Deck_ID} added/updated successfully.");
            return response.Models[0];
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error inserting deck: {ex.Message}");
            return null;
        }
    }
    public async Task UpdateDeckInDatabase(Decks deck)
    {
        try
        {
            var response = await client
                .From<Decks>()
                .Update(deck);

            Debug.Log($"Deck {deck.Deck_ID} updated successfully.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error updating deck: {ex.Message}");
        }
    }
    public async void DeleteDeckFromDB(DeckData deckToRemove)
    {
        try
        {
            await client
            .From<Decks>()
            .Where(d => d.Deck_ID == deckToRemove.deckID)
            .Delete();

            Debug.Log($"Deck {deckToRemove.deckID} removed successfully.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error deleting deck: {ex.Message}");
        }
    }
    public async Task<List<Decks>> GetDecksFromDB(int user_id,int game_id)
    {
        var response = await client
       .From<Decks>()
       .Where(d => d.FK_User_ID == user_id && d.FK_Game_ID == game_id)
       .Get();
        Debug.Log($"Retrieved Decks: {response.Models.Count}");
        return response.Models;  // Returns a list of Decks
    }
    // ---------- MATCHMAKING ----------
    public async Task<bool> CheckForMatch(int currentUserId, int requiredPlayers = 2)
    {
        var queueResponse = await client
            .From<MatchmakingQueue>()
            .Order("queued_at", Postgrest.Constants.Ordering.Ascending)
            .Limit(requiredPlayers - 1)
            .Get();

        var queuePlayers = queueResponse.Models;

        // Check if player is already in the queue
        if (queuePlayers.Any(p => p.User_ID == currentUserId))
        {
            Debug.Log("Already in queue.");
            return false;
        }

        if (queuePlayers.Count == requiredPlayers - 1)
        {
            // Attempt to delete all opponents atomically
            try
            {
                var allPlayers = queuePlayers.Select(q => q.User_ID).ToList();
                allPlayers.Add(currentUserId); // include self

                // Delete all opponents from queue
                foreach (var p in queuePlayers)
                {
                    await client
                        .From<MatchmakingQueue>()
                        .Where(q => q.User_ID == p.User_ID)
                        .Delete();
                }
                SubscribeToMatchStart(currentUserId);
                await CreateMatch(allPlayers);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error during matchmaking: {ex.Message}");
                await AddToQueue(currentUserId);
                return false;
            }
        }
        else
        {
            // Not enough players yet
            await AddToQueue(currentUserId);
            return false;
        }
    }
    private async Task AddToQueue(int userId)
    {
        try
        {
            SubscribeToMatchStart(userId);
            await client
                .From<MatchmakingQueue>()
                .Insert(new MatchmakingQueue { User_ID = userId });
            Debug.Log("Added to queue.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to add to queue: {ex.Message}");
        }
    }
    public async Task CreateMatch(List<int> playerUserIds)
    {
        if (playerUserIds == null || playerUserIds.Count == 0)
        {
            Debug.LogError("No players provided to create match.");
            return;
        }

        // Step 1: Create a new match with default state
        var match = new Match
        {
            Game_State = new Dictionary<string, object>(), // or any default JSON game state
            Current_Turn_Index = 0,
            Created_At = DateTime.UtcNow
        };

        var matchResponse = await client.From<Match>().Insert(match);
        var createdMatch = matchResponse.Models.FirstOrDefault();

        if (createdMatch == null)
        {
            Debug.LogError("Failed to create match.");
            return;
        }
        GameManager gameManager = GameManager.Instance;
        int health = 0;
        if(gameManager.settings.health_win_condition == true)
            health = int.Parse(gameManager.settings.player_health);

        // Step 2: Assign each player an index and insert into match_players table
        var matchPlayers = playerUserIds.Select((userId, index) => new MatchPlayers
        {
            Match_ID = createdMatch.Match_ID,
            User_ID = userId,
            Player_Index = index,
            Current_Health = health
        }).ToList();

        await client.From<MatchPlayers>().Insert(matchPlayers);

        Debug.Log($"Created match {createdMatch.Match_ID} with {playerUserIds.Count} players.");
        // start the match for the player that wasnt in queue but matched with other players
    }
    

    public async void SubscribeToMatchStart(int userId)
    {
        var channelManager = FindFirstObjectByType<RealTimeChannelManager>();
        await channelManager.SubscribeToChannel("match_players", ListenType.Inserts,async change =>
        {
            var insertedRow = change.Model<MatchPlayers>();
            if (insertedRow == null)
            {
                Debug.LogWarning("Inserted row is null. Dumping raw payload for inspection:");
                Debug.Log(change?.Payload?.ToString());
                return;
            }

            if (insertedRow.User_ID == userId)
            {
                Debug.Log("You have been assigned to a match!");

                var matchResult = await client
                    .From<Match>()
                    .Where(m => m.Match_ID == insertedRow.Match_ID)
                    .Get();

                if (matchResult.Models.Count > 0)
                {
                    var match = matchResult.Models[0];
                    UnityMainThreadDispatcher.Instance.Enqueue(() =>
                    {
                        channelManager.UnsubscribeFromChannel("match_players");
                        MatchManager.Instance.StartGame(match);
                    });
                }
                else
                {
                    Debug.LogWarning("Match not found with ID: " + insertedRow.Match_ID);
                }
            }
        });
        Debug.Log("SubscribeToMatchStart");
    }
    public async Task<List<MatchPlayers>> GetMatchPlayers(Guid match_ID)
    {
        var playersResult = await client
        .From<MatchPlayers>()
        .Where(mp => mp.Match_ID == match_ID)
        .Get();

        return playersResult.Models;
    }
    // ---------- GAME ----------

    public async Task<List<GamePlayers>> GetGamePlayersByGameID(int gameId)
    {
        try
        {
            var result = await client
                .From<GamePlayers>()
                .Where(gp => gp.FK_Game_ID == gameId)
                .Get();

            return result.Models ?? new List<GamePlayers>();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error retrieving game players by GameID {gameId}: {ex.Message}");
            return new List<GamePlayers>();
        }
    }
    public async Task<int?> GetGamesPlayed(int gameId)
    {
        try
        {
            var response = await client
            .From<PublishedGames>()
            .Where(pg => pg.Game_ID == gameId)
            .Select("games_played")
            .Single();
            return response?.Games_Played;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error fetching games played for gameId {gameId}: {ex.Message}");
            return null;
        }
    }
    public async Task<int> CountDecksByGameId(int gameId)
    {
        try
        {
            var response = await client
                .From<Decks>()
                .Where(d => d.FK_Game_ID == gameId)
                .Get();

            return response.Models.Count;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error counting decks for gameId {gameId}: {ex.Message}");
            return 0;
        }
    }
    public async Task LogGameLogin(int userId, int gameId)
    {
        try
        {
            // Check if row exists
            var existingEntry = await client
                .From<GamePlayers>()
                .Where(gp => gp.FK_User_ID == userId && gp.FK_Game_ID == gameId)
                .Get();

            if (existingEntry.Models.Count > 0)
            {
                // Row exists – trigger DB update (last_logged_in set by trigger)
                var existing = existingEntry.Models[0];

                var updateResponse = await client
                    .From<GamePlayers>()
                    .Where(gp => gp.FK_User_ID == existing.FK_User_ID && gp.FK_Game_ID == existing.FK_Game_ID)
                    .Set(gl => gl.Win_Count, existing.Win_Count) // Dummy update to trigger the trigger
                    .Update();

                Debug.Log("Updated last_logged_in via DB.");
            }
            else
            {
                // Row doesn't exist – insert new (DB sets last_logged_in automatically)
                var newEntry = new GamePlayers
                {
                    FK_User_ID = userId,
                    FK_Game_ID = gameId,
                    Win_Count = 0,
                    Lose_Count = 0
                    // Do NOT set Last_Logged_In
                };

                var insertResponse = await client
                    .From<GamePlayers>()
                    .Insert(newEntry);

                Debug.Log("Inserted new game login entry (DB set last_logged_in).");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error logging game login: {ex.Message}");
        }
    }
    public async Task IncreaseGamesPlayed(int gameId)
    {
        try
        {
            var existingEntry = await client
                .From<PublishedGames>()
                .Where(pg => pg.Game_ID == gameId)
                .Get();

            var game = existingEntry.Models.FirstOrDefault();
            if (game != null)
            {
                // Update the appropriate count
                game.Games_Played += 1;

                var updateResponse = await client
                    .From<PublishedGames>()
                    .Where(pg => pg.Game_ID == gameId)
                    .Set(gp => gp.Games_Played, game.Games_Played)
                    .Update();

                Debug.Log($"Updated {game.Games_Played}");
            }
            else
            {
                Debug.LogWarning("Player entry not found.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error updating win/lose count: {ex.Message}");
        }
    }
    public async Task UpdatePlayerWinLoseCount(int userId, int gameId,bool isWin)
    {
        try
        {
            var existingEntry = await client
                .From<GamePlayers>()
                .Where(gp => gp.FK_User_ID == userId && gp.FK_Game_ID == gameId)
                .Get();

            var player = existingEntry.Models.FirstOrDefault();
            if (player != null)
            {
                // Update the appropriate count
                if (isWin)
                    player.Win_Count += 1;
                else
                    player.Lose_Count += 1;

                var updateResponse = await client
                    .From<GamePlayers>()
                    .Where(gp => gp.FK_User_ID == userId && gp.FK_Game_ID == gameId)
                    .Set(gl => gl.Win_Count, player.Win_Count)
                    .Set(gl => gl.Lose_Count, player.Lose_Count)
                    .Update();

                Debug.Log($"Updated {(isWin ? "win" : "lose")} count for player.");
            }
            else
            {
                Debug.LogWarning("Player entry not found.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error updating win/lose count: {ex.Message}");
        }
    }
    public async void ChangeTurnInMatch(Guid match_ID, int activePlayer)
    {
        var playersResult = await client
        .From<Match>()
        .Where(m => m.Match_ID == match_ID)
        .Set(m => m.Current_Turn_Index, activePlayer)
        .Update();

    }
    public async Task SubscribeToGameState(Guid currentMatchID)
    {
        // Get reference to RealTimeChannelManager
        var channelManager = FindFirstObjectByType<RealTimeChannelManager>();

        if (channelManager == null)
        {
            Debug.LogError("RealTimeChannelManager not found in the scene!");
            return;
        }

        // Subscribe to the "matches" channel using RealTimeChannelManager
        await channelManager.SubscribeToChannel("matches", ListenType.Updates, (change) =>
        {
            // This is where the handler logic goes
            Debug.Log("Match table update received");

            var match = change.Model<Match>();

            if (match != null)
            {

                Debug.Log(match.Match_ID);
                if (match.Match_ID == currentMatchID)
                {
                    Debug.Log(match.Match_ID);
                    if (match.Game_State != null &&
                        match.Game_State.ContainsKey("phase") &&
                        match.Game_State["phase"].ToString() == "started")
                    {
                        Debug.Log("Changed turn");

                        UnityMainThreadDispatcher.Instance.Enqueue(() =>
                        {
                                OnChangeTurn?.Invoke(match.Current_Turn_Index);
                        });
                        
                    }
                }
            }
        });

    }
    public async Task PollForUpdates(Guid matchID)
    {
        while (true)
        {
            var response = await client
                .From<Match>()
                .Where(m => m.Match_ID == matchID)
                .Single();

            if (response != null && response.Game_State.ContainsKey("phase") &&
                response.Game_State["phase"].ToString() == "started")
            {
                Debug.Log("Game started!");

                if (response.Game_State.ContainsKey("active_player_id"))
                {
                    int activePlayerId = Convert.ToInt32(response.Game_State["active_player_id"]);
                    UnityMainThreadDispatcher.Instance.Enqueue(() =>
                    {
                        OnGameStart?.Invoke(activePlayerId);
                    });
                    break; // Exit the loop once the game starts
                }
            }

            await Task.Delay(2000); // Poll every 3 seconds
        }
    }
    public async Task SubscribeToOpponent(Guid currentMatchID, int playerIndex)
    {
        
        // Subscribe to updates on the MatchPlayers table for real-time changes
         await FindFirstObjectByType<RealTimeChannelManager>().SubscribeToChannel("match_players", ListenType.Updates ,change =>
         {
             var opponent = change.Model<MatchPlayers>();
             if (opponent != null && opponent.Match_ID == currentMatchID && opponent.Player_Index == playerIndex)
             {
                 Debug.Log("📡 Opponent update received");
                 UnityMainThreadDispatcher.Instance.Enqueue(() =>
                 {
                     onOpponentUpdate?.Invoke(opponent);
                 });
                 
             }
         });
    }
    public async Task UpdatePlayerInMatch(MatchPlayers player)
    {
        var response = await client
         .From<MatchPlayers>()
         .Upsert(player);

        if (response.Models != null && response.Models.Count > 0)
        {
            Debug.Log("Player row inserted or updated via upsert.");
        }
    }
    // <-----------------------------------Utility------------------------------------------>
    public async Task<byte[]> DownloadFileFromSupabase(string bucketName, string filePath)
    {
        try
        {
            // Get the file reference from the Supabase storage
            var file = await client.Storage.From(bucketName).Download(filePath, null);

            if (file != null)
            {
                return file;
            }
            else
            {
                Console.WriteLine("File not found.");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading file: {ex.Message}");
            return null;
        }
    }


}

