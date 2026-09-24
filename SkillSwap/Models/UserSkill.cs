namespace SkillSwap.Models
{
    public class UserSkill
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int SkillId { get; set; }
        public Skill Skill { get; set; } = null!;

        public string Level { get; set; } = "Beginner";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}