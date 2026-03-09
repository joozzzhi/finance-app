using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Models
{
    [Table("UserTargets")]
    public class UserTarget
    {
        [Key]
        public int UserTargetsID { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }

        [ForeignKey("Target")]
        public int TargetID { get; set; }

        public User? User { get; set; }
        public Target? Target { get; set; }
    }
}
