using static Supabase.Realtime.PostgresChanges.PostgresChangesOptions;
using Supabase.Realtime;

using System.Collections.Generic;
using System;
using UnityEngine;
using System.Threading.Tasks;
using Client = Supabase.Client;
using Supabase.Realtime.PostgresChanges;

public class RealTimeChannelManager : MonoBehaviour
{
    // Dictionary to keep track of active channels
    private Dictionary<string, RealtimeChannel> activeChannels = new Dictionary<string, RealtimeChannel>();
    private Client client;

    private void Start()
    {
        client = SupabaseManager.Instance.client;
    }
    // Subscribe to a channel, ensuring we don't have duplicate subscriptions
    public async Task SubscribeToChannel(string channelName, ListenType listenType, Action<PostgresChangesResponse> onUpdate)
    {
        if (activeChannels.ContainsKey(channelName))
        {
            Debug.Log($"Already subscribed to {channelName}, skipping subscription.");
            return;
        }

        var channel = client.Realtime.Channel("realtime", "public", channelName);
        
        // Add event handler with the correct listen type
        channel.AddPostgresChangeHandler(listenType, (sender, change) =>
        {
            Debug.Log($"{channelName} table {listenType} received");
            onUpdate?.Invoke(change);
        });

        try
        {
            await channel.Subscribe();
            activeChannels[channelName] = channel; // Store the channel reference
            Debug.Log($"📡 Successfully subscribed to {channelName}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Error during subscription to {channelName}: " + ex.Message);
        }
    }
    // Unsubscribe from a channel when no longer needed
    public void UnsubscribeFromChannel(string channelName)
    {
        if (activeChannels.ContainsKey(channelName))
        {
            var channel = activeChannels[channelName];
            channel.Unsubscribe();
            activeChannels.Remove(channelName); // Clean up after unsubscribe
            Debug.Log($"📡 Unsubscribed from {channelName}");
        }
        else
        {
            Debug.LogWarning($"No active subscription found for {channelName}");
        }
    }
}
