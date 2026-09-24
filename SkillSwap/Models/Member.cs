using System.ComponentModel.DataAnnotations;

namespace SkillSwap.Models
{
    public class Member
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Bio")]
        public string? Bio { get; set; }

        [Required]
        [Display(Name = "Skill I Can Teach")]
        public string SkillToOffer { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Skill I Want To Learn")]
        public string SkillToLearn { get; set; } = string.Empty;

        [Display(Name = "Joined Date")]
        public DateTime JoinedDate { get; set; } = DateTime.Now;
    }
}