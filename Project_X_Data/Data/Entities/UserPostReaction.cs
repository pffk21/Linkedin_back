namespace Project_X_Data.Data.Entities
{
    public class UserPostReaction
    {
        public Guid UserId { get; set; }
        public Guid PostId { get; set; }
        public User User { get; set; } = null!;
        public Post Post { get; set; } = null!;
        public bool ReactionType { get; set; }

       
    }
}
