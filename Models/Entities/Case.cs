namespace CRM.Models.Entities
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;

    public class Case
    {
        [Key]
        [Required]
        public Guid CustomerID { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(255)]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Contact { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string CaseChannel { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Open";

        public Guid EmpID { get; set; }  // Employee ID who created the case

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public Guid CreatedBy { get; set; } // To store the ID of the user who created the case (same as EmpID)

        
        [JsonIgnore]
        public User? User { get; set; } // Navigation property to User
    }
}
