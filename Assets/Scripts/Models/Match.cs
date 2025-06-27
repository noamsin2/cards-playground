namespace Models
{
    using Postgrest.Models;
    using Postgrest.Attributes;
    using System;
    using System.Collections.Generic;


    [Table("matches")]
    public class Match : BaseModel
    {
        [PrimaryKey("match_id", false)]
        public Guid Match_ID { get; set; }

        [Column("game_state")]
        public Dictionary<string, object> Game_State { get; set; }

        [Column("current_turn_index")]
        public int Current_Turn_Index { get; set; }

        [Column("created_at")]
        public DateTime Created_At { get; set; }
    }
}
