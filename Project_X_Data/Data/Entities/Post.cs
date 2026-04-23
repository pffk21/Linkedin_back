using System.Text.Json.Serialization;

namespace Project_X_Data.Data.Entities
{
    public class Post
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = null!;
        public string ? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [JsonIgnore]
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        [JsonIgnore]
        public ICollection<UserPostReaction> PostReactions { get; set; } = new List<UserPostReaction>();
    }
}
