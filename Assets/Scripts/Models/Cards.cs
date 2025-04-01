namespace Models
{

    using Postgrest.Models;
    using Postgrest.Attributes;
    using System;

    [Table("cards")]
    public class Cards : BaseModel
    {
        [PrimaryKey("card_id")] // Optional: Define a primary key if needed
        public int Card_ID { get; set; }

        //[Column("created_at")]
        //public DateTimeOffset CreatedAt { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("fk_game_id")]
        public int FK_Game_ID { get; set; }
        [Column("image_url")]
        public string Image_URL { get; set; }
        [Column("is_image_changed")]
        public bool Is_Image_Changed { get; set; }

    }

}