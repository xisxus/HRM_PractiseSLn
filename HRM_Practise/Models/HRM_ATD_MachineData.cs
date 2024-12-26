using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace HRM_Practise.Models
{
    [Table("HRM_ATD_MachineData")]  
    public class HRM_ATD_MachineData
    {
        [Key]  
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]  
        public int AutoId { get; set; }

        [Required]  
        [StringLength(50)]  
        public string FingerPrintId { get; set; }

        [Required]  
        [StringLength(50)]  
        public string MachineId { get; set; }

        [Required]  
        public DateTime Date { get; set; }

        [Required]  
        public DateTime Time { get; set; }

        [StringLength(int.MaxValue)]  
        public string Latitude { get; set; }

        [StringLength(int.MaxValue)]  
        public string Longitude { get; set; }

        [StringLength(50)]  
        public string HOALR { get; set; }
    }
}
