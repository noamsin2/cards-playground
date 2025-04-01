using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[Serializable]
public class GameSettings
{
    public bool health_win_condition;
    public bool cards_win_condition;
    public string player_health;
    public string deck_size;
    public string initial_hand_size;
    public string max_hand_size;
    public int turn_length;
    public string card_copies;
    public string limit_hand_size;
}
