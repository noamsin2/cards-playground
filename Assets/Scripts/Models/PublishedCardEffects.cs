namespace Models
{

    using Postgrest.Models;
    using Postgrest.Attributes;
    using System;

    [Table("published_card_effects")]
    public class PublishedCardEffects : BaseModel
    {
        // Foreign key for the card
        [Column("fk_card_id")]
        public int FK_Card_ID { get; set; }

        // Effect of the card
        [Column("effect")]
        public string Effect { get; set; }
        [Column("action")]
        // Action of the card
        public string Action { get; set; }
        [Column("x")]
        // Action of the card
        public string X { get; set; }
    }

}