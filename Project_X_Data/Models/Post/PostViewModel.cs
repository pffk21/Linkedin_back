namespace Project_X_Data.Models.Post
{
    public class PostViewModel
    {
        public Guid UserId { get; set; }
        public string Description { get; set; } = null!;
        public IFormFile? ImageFile { get; set; }
        
    }
}
