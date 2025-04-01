namespace Models
{

    using Postgrest.Models;
    using Postgrest.Attributes;
    using System;

    [Table("users")]
    public class Users : BaseModel
    {
        [PrimaryKey("user_id", true)] // Optional: Define a primary key if needed
        public int User_ID { get; set; }

        //[Column("created_at")]
        //public DateTimeOffset CreatedAt { get; set; }

        [Column("steam_id")]
        public string Steam_ID { get; set; }
    }

}