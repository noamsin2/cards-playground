namespace Models
{

    using Postgrest.Models;
    using Postgrest.Attributes;
    using System;

    [Table("decks")]
    public class Decks : BaseModel
    {
        [PrimaryKey("deck_id")]
        public int Deck_ID { get; set; }
        [Column("name")]
        public string Name { get; set; }

        [Column("fk_game_id")]
        public int FK_Game_ID { get; set; }
        [Column("fk_user_id")]
        public int FK_User_ID { get; set; }
        [Column("card_ids")]
        public int[] Card_IDs { get; set; }

    }

}