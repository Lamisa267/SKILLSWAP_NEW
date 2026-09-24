using System.ComponentModel.DataAnnotations;

namespace SkillSwap.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = "User";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<UserSkill> UserSkills { get; set; } = new List<UserSkill>();

        public ICollection<SwapRequest> SentSwapRequests { get; set; } = new List<SwapRequest>();

        public ICollection<SwapRequest> ReceivedSwapRequests { get; set; } = new List<SwapRequest>();
    }
}