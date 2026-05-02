namespace Project_X_Data.Models.Post
{
    public class PostReactionViewModel
    {
        public Guid PostId { get; set; }
        public Guid UserId { get; set; }
        public bool ReactionType { get; set; }
    }
}
