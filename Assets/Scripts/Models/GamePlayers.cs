namespace Models
{

    using Postgrest.Models;
    using Postgrest.Attributes;
    using System;
    [Table("game_players")]
    public class GamePlayers : BaseModel
    {
        [Column("fk_user_id")]
        public int FK_User_ID { get; set; }

        [Column("fk_game_id")]
        public int FK_Game_ID { get; set; }
        [Column("last_logged_in")]
        public DateTime? Last_Logged_In { get; set; }
        [Column("win_count")]
        public int Win_Count {  get; set; }
        [Column("lose_count")]
        public int Lose_Count {  get; set; }
    }

}
