namespace Models
{

    using Postgrest.Models;
    using Postgrest.Attributes;
    using System;

    [Table("published_games")]
    public class PublishedGames : BaseModel
    {
        [PrimaryKey("game_id")] // Optional: Define a primary key if needed
        public int Game_ID { get; set; }

        //[Column("created_at")]
        //public DateTimeOffset CreatedAt { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("fk_user_id")]
        public int FK_User_ID { get; set; }
        [Column("is_deleted")]
        public bool Is_Deleted { get; set; }
        [Column("is_published")]
        public bool Is_Published { get; set; }
        [Column("deleted_at")]
        public DateTime? Deleted_At { get; set; }
        [Column("game_settings")]
        public string Game_Settings { get; set; }
        [Column("games_played")]
        public int Games_Played { get; set; }
        [Column("updated_at")]
        public DateTime? Updated_At { get; set; }
    }

}