using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Models
{
    [Table("Incomes")]
    public class Income
    {
        [Key]
        public int IncomeID { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }

        [Required]
        [MaxLength(100)]
        public string IncomeName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal IncomeAmount { get; set; }

        public DateTime IncomeDate { get; set; } = DateTime.Now;

        public User? User { get; set; }
    }
}
