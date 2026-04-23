namespace Project_X_Data.Data.Entities
{
    public class Comment
    {
        public Guid Id { get; set; }
        public Guid PostId { get; set; }
        public Post Post { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<UserCommentReaction> CommentReactions { get; set; } = new List<UserCommentReaction>();
    }
}
