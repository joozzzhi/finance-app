using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Models
{
    [Table("Expenses")]
    public class Expense
    {
        [Key]
        public int ExpenseID { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }

        [Required]
        [MaxLength(100)]
        public string ExpenseName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal ExpenseAmount { get; set; }

        public DateTime ExpenseDate { get; set; } = DateTime.Now;

        public User? User { get; set; }
    }
}
