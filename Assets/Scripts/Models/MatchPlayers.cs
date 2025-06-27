using Postgrest.Attributes;
using Postgrest.Models;
using System;

namespace Models
{
    [Table("match_players")]
    public class MatchPlayers : BaseModel
    {
        [PrimaryKey("id")]
        public Guid Id { get; set; }

        [Column("match_id")]
        public Guid Match_ID { get; set; }

        [Column("user_id")]
        public long User_ID { get; set; }

        [Column("player_index")]
        public int Player_Index { get; set; }
        [Column("current_health")]
        public int Current_Health { get; set; }
        [Column("current_cards_in_deck")]
        public int[] Current_Cards_In_Deck {  get; set; }
        [Column("current_cards_in_hand")]
        public int[] Current_Cards_In_Hand { get; set; }
        [Column("has_mulliganed")]
        public bool HasMulliganed { get; set; }
        [Column("cards_played")]
        public int[] Cards_Played { get; set; }
    }
}
