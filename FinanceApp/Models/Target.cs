using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Models
{
    [Table("Targets")]
    public class Target
    {
        [Key]
        public int TargetID { get; set; }

        [Required]
        [MaxLength(100)]
        public string TargetName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TargetAmount { get; set; }

        public DateTime StartDate { get; set; } = DateTime.Now;

        public DateTime EndDate { get; set; }

        public ICollection<UserTarget> UserTargets { get; set; } = new List<UserTarget>();
    }
}
