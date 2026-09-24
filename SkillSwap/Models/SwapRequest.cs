using System.ComponentModel.DataAnnotations;

namespace SkillSwap.Models
{
    public class SwapRequest
    {
        public int Id { get; set; }

        public int SenderId { get; set; }
        public User Sender { get; set; } = null!;

        public int ReceiverId { get; set; }
        public User Receiver { get; set; } = null!;

        public int OfferedSkillId { get; set; }
        public Skill OfferedSkill { get; set; } = null!;

        public int RequestedSkillId { get; set; }
        public Skill RequestedSkill { get; set; } = null!;

        [StringLength(500)]
        public string Message { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}