namespace Project_X_Data.Data.Entities
{
    public class Skill
    {
        public Guid Id { get; set; }
        public string SkillName { get; set; } = null!;
        public ICollection<UserSkill> UserSkills { get; set; } = new List<UserSkill>();
    }
}
