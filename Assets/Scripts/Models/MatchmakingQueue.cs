namespace Models
{
    using Postgrest.Models;
    using Postgrest.Attributes;
    using System;

    [Table("matchmaking_queue")]
    public class MatchmakingQueue : BaseModel
    {
        [PrimaryKey("match_id")] // The primary key of the table, unique for each match
        public Guid? Match_ID { get; set; } // Nullable in case it's not yet assigned

        [Column("user_id")] // Maps to the user_id column in the table
        public int User_ID { get; set; }

        [Column("queued_at")] // Maps to the queued_at column in the table
        public DateTime Queued_At { get; set; }
    }
}
