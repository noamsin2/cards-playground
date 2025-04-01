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

public class SupabaseManager : MonoBehaviour
{
    public static SupabaseManager Instance { get; private set; }
    [SerializeField] private TimeAPIManager timeAPIManager;
    private Client client;
    //private string supabaseUrl;
    //private string apiKey;
    const string CARD_BUCKET = "card-bucket";
    const string PUBLISHED_CARD_BUCKET = "published-card-bucket";
    //public CardDisplayManager cardDisplayManager;
    private async void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            // Load Supabase credentials from environment variables or hardcode for testing
            string supabaseUrl = EnvReader.GetEnvVariable("SUPABASE_URL");
            string apiKey = EnvReader.GetEnvVariable("SUPABASE_API_KEY");
            Debug.Log(supabaseUrl);

            // Initialize Supabase client
            var options = new Supabase.SupabaseOptions
            {
                AutoConnectRealtime = true
            };
            client = new Client(supabaseUrl, apiKey, options);
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
    public async Task<int> EnsureSteamUserExists(string steamId)
    {
        try
        {
            var response = await client.From<Users>()
                .Filter("steam_id", Postgrest.Constants.Operator.Equals, steamId)
                .Get();

            if (response.Models.Count == 0)
            {
                // User does not exist, create a new one
                var newUser = new Users
                {
                    Steam_ID = steamId
                };

                var insertResponse = await client.From<Users>().Insert(newUser);

                if (insertResponse.ResponseMessage.IsSuccessStatusCode && insertResponse.Models.Count > 0)
                {
                    var createdUser = insertResponse.Models[0]; // Access the first inserted user
                    Debug.Log("New Steam user created successfully!");
                    return createdUser.User_ID; // Return the ID of the created user
                }
                else
                {
                    Debug.LogError($"Failed to create Steam user: {insertResponse.ResponseMessage.ReasonPhrase}");
                    return -1;
                }
            }
            else
            {
                Debug.Log("Steam user already exists.");
                var user = response.Models[0];
                return user.User_ID;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Exception while checking Steam user: {e.Message}");
            return -1;
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
    public async Task<CardGames> EditGameSettingsInDatabase(int gameId,string name, string jsonSettings)
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
                DateTime? serverTime = await timeAPIManager.FetchServerTime();
                if (serverTime == null)
                {
                    Debug.LogError("Failed to fetch the server time.");
                    return false;
                }
                var updateResponse = await client
                    .From<CardGames>()
                    .Where(cg => cg.Game_ID == cardGame.Game_ID)
                    .Set(cg => cg.Is_Deleted, true)
                    .Set(cg => cg.Deleted_At, serverTime)
                    .Set(cg => cg.Is_Published, false)
                    .Update();

                if (updateResponse.Models.Count == 0)
                {
                    Debug.LogError("Failed to update the game in the database.");
                    return false;
                }

                Debug.Log("Game 'is_deleted' updated to true.");
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
    public async Task<bool> PublishGame(CardGames cardGame)
    {
        try
        {
            if (cardGame == null)
            {
                Debug.LogError("Game not found.");
                return false;
            }
            var updateResponse = await client
                .From<CardGames>()
                .Where(cg => cg.Game_ID == cardGame.Game_ID)
                .Set(cg => cg.Is_Published, true)
                .Update();

            var publishedGame = await client
                .From<PublishedGames>()
                .Where(cg => cg.Game_ID == cardGame.Game_ID).Get();

            int gamesPlayed = 0;
            if(publishedGame != null)
            {
                gamesPlayed = publishedGame.Models[0].Games_Played;
            }
            // Copy game to published_games
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

            // Copy cards and card effects to published_cards

            foreach (var card in cards.Models)
            {
                // If the image is changed
                if(card.Is_Image_Changed == true)
                {
                    
                    string fileExtension = Path.GetExtension(card.Image_URL);
                  
                    byte[] downloadedFile = await DownloadFileFromSupabase(CARD_BUCKET, card.Image_URL);
                    if (downloadedFile != null)
                    {
                        // Upload and overwrite the file if it exists
                        await UploadFileToSupabase(PUBLISHED_CARD_BUCKET, card.FK_Game_ID + fileExtension, downloadedFile);
                    }
                    else
                    {
                        Debug.LogError("Failed to download an image while publishing\n");
                    }
                }
                var newPublishedCard = new PublishedCards
                {
                    Card_ID = card.Card_ID,
                    FK_Game_ID = card.FK_Game_ID,
                    Name = card.Name,
                    Image_URL = card.Image_URL,
                };

                // Upsert should automatically check for the primary key and replace if it exists, otherwise add the row normally
                var upsertCardResponse = await client
                .From<PublishedCards>()
                .Upsert(newPublishedCard);
                

                // Fetch card effects for this card
                var cardEffects = await client.From<CardEffects>()
                    .Where(ce => ce.FK_Card_ID == card.Card_ID)
                    .Get();

                // Copy effects to published_card_effects
                foreach (var effect in cardEffects.Models)
                {
                    var newPublishedEffect = new PublishedCardEffects
                    {
                        Effect = effect.Effect,
                        FK_Card_ID = effect.FK_Card_ID,
                        Action = effect.Action,
                        X = effect.X
                    };

                    var upsertEffectResponse = await client
                    .From<PublishedCardEffects>()
                    .Upsert(newPublishedEffect);

                }
            }
            
            Debug.Log("Game, cards, and card effects successfully published.");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error publishing game: {ex.Message}");
            // Revert changes if there's an error
            var updateResponse = await client
                .From<CardGames>()
                .Where(cg => cg.Game_ID == cardGame.Game_ID)
                .Set(cg => cg.Is_Published, false)
                .Update();
            await client.From<PublishedGames>().Where(pcg => pcg.Game_ID == cardGame.Game_ID).Delete();
            return false;
        }
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

                if (updateResponse.Models.Count == 0)
                {
                    Debug.LogError("Failed to update the game in the database.");
                    return false;
                }

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
    
    // <-----------------------------------CARDS------------------------------------------>
    public async Task UploadCard(string filePath, string name, int fk_game_id, Dictionary<string, List<EffectWithX>> cardEffects)
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
            
            card = await AddCardToDatabase(name, fk_game_id);
            string fileNameWithExtension = card.Card_ID.ToString() + fileExtension;
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
                await DeleteCardFromStorage(fileUrl);
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
            string result = fileUrl.Replace(bucket, "");
            return result;
        }
        catch (Exception e)
        {
            Debug.LogError("An error occurred while uploading the file: " + e.Message);
            return "";
        }
    }
    private async Task<bool> DeleteCardFromStorage(string filePath)
    {
        if(await DeleteFileFromStorage(filePath, CARD_BUCKET))
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
        
        var files = await client.Storage.From(CARD_BUCKET).List();
        bool fileExists = files.Any(file => file.Name == card.Image_URL);

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

       
            string fileNameWithExtension = card.Card_ID.ToString() + fileExtension;

            if (await DeleteCardFromStorage(card.Image_URL))
            {
                string fileUrl = await AddCardToStorage(filePath, fileNameWithExtension, CARD_BUCKET);
                card.Is_Image_Changed = true;
                if (!string.IsNullOrEmpty(fileUrl))
                {
                    await EditCardOnDatabase(card, card.Name, fileUrl);
                }
                
            }
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("An error occurred while uploading the file: " + e.Message);
            return false;
        }
        //if (fileUrl.IsNullOrEmpty() && card != null)
        //{
        //    await DeleteCardFromDatabase(card.Card_ID);
        //}
        //else if (!fileUrl.IsNullOrEmpty() && card != null)
        //{
        //    card.Image_URL = fileUrl;
        //    try
        //    {
        //        var update = await client
        //          .From<Cards>()
        //          .Where(c => c.Card_ID == card.Card_ID)
        //          .Set(c => c.Image_URL, fileUrl)
        //          .Update();

        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError($"Failed to update card with file URL: {e.Message}");
        //        await DeleteCardFromDatabase(card.Card_ID);
        //        await DeleteFileFromStorage(fileUrl, CARD_BUCKET);
        //    }
        //}

    }
    public async Task<bool> EditCardOnDatabase(Cards card, string newName, string newImageUrl)
    {
        try
        {
            if (card.Name != newName)
            {
                var updateResponse = await client
                    .From<Cards>()
                    .Where(c => c.Card_ID == card.Card_ID)
                    .Set(c => c.Name, newName)
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
    public string GetCardUrl(string filePath)
    {
        return GetFileUrl(CARD_BUCKET, filePath);
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
            var existingFile = await client.Storage.From(bucketName).List(filePath);
            if (existingFile != null && existingFile.Count > 0)
            {
                Debug.Log($"File {filePath} already exists. Deleting...");
                await client.Storage.From(bucketName).Remove(new List<string> { filePath });
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
                    X = effectWithX.X
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
                    group => group.Select(e => new EffectWithX(e.Effect, e.X)).ToList()
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
            Debug.Log(filePath);
            Debug.Log(publicUrl);
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
            //string card_id_str = card_id.ToString();
            var result = await client.Storage
            .From(bucketName)
            .Remove(fileUrl);
            Debug.Log(result);
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
           .Order("games_played", Ordering.Descending)
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
    public async Task<List<Cards>> GetAllCardsInGame(int gameID)
    {
        var response = await client.From<Cards>()
            .Filter("fk_game_id", Postgrest.Constants.Operator.Equals, gameID)
            .Get();
        
        return response.Models;
    }
    public async Task<List<CardEffects>> GetAllCardEffects(int cardID)
    {
        var response = await client.From<CardEffects>()
            .Filter("fk_card_id", Postgrest.Constants.Operator.Equals, cardID)
            .Get();

        return response.Models;
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

