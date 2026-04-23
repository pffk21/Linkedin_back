using System.Text.Json.Serialization;

namespace Project_X_Data.Data.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string AboitSection { get; set; } = null!;
        public string AvatarPhoto { get; set; } = null!;
        public DateTime DateOfBirth { get; set; } 
        public DateTime CreatedAt { get; set; } 

        [JsonIgnore]
        public ICollection<UserAccess> Accesses { get; set; } = [];
        [JsonIgnore]
        public ICollection<UserSkill>? UserSkills { get; set; } = new List<UserSkill>();
        [JsonIgnore]
        public ICollection<Expirience>? Expiriences { get; set; } = new List<Expirience>();
        [JsonIgnore]
        public ICollection<Education>? Educations { get; set; } = new List<Education>();
        [JsonIgnore]
        public ICollection<Message>? Messages { get; set; } = new List<Message>();
        [JsonIgnore]
        public ICollection<UserPostReaction>? PostReactions { get; set; } = new List<UserPostReaction>();
        [JsonIgnore]
        public ICollection<UserCommentReaction>? CommentReactions { get; set; } = new List<UserCommentReaction>();
        [JsonIgnore]
        public ICollection<ContactInformation>? ContactInformations { get; set; } = new List<ContactInformation>();

    }
}
