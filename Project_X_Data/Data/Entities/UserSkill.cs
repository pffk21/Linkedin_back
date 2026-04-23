namespace Project_X_Data.Data.Entities
{
    public class UserSkill
    {
        public Guid UserId { get; set; }
        public Guid SkillId { get; set; }

        public int Level { get; set; }

        public User User { get; set; } = null!;
        public Skill Skill { get; set; } = null!;
    }
}
