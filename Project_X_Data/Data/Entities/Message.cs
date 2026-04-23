using System.Text.Json.Serialization;

namespace Project_X_Data.Data.Entities
{
    public class Message
    {
        public Guid Id { get; set; }

        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        [JsonIgnore]
        public User Sender { get; set; } = null!;
        [JsonIgnore]
        public User Receiver { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime SentAt { get; set; }
        public DateTime? Deleted { get; set; }


    }
}
