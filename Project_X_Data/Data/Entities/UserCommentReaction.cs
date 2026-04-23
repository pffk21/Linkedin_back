namespace Project_X_Data.Data.Entities
{
    public class UserCommentReaction
    {
        public Guid UserId { get; set; }
        public Guid CommentId { get; set; }
        public User User { get; set; } = null!;
        public Comment Comment { get; set; } = null!;
        public bool ReactionType { get; set; } 
    }
}
