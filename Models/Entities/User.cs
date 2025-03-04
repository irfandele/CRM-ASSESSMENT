using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace CRM.Models.Entities
{
    public class User
    {
        [Key]
        public Guid EmpID { get; set; } = Guid.NewGuid(); // Unique Identifier

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty; // User's login name

        [Required]
        public string PasswordHash { get; set; } = string.Empty; // Encrypted password

        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty; // Optional phone number

        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = "CustomerSupport"; // Role (CustomerSupport, WhatsAppTeam)

        // Navigation property for Cases
        public ICollection<Case> Cases { get; set; } = new List<Case>();
    }
}