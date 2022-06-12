using System.ComponentModel.DataAnnotations;

namespace Recipe_Manager.Server.Models
{
    public class Ingriedient
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string? Image_path { get; set; }
        [Required]
        public double Amount { get; set; }
        [Required]
        public string Unit { get; set; }
        public string StepId { get; set; }
        public virtual Step Step { get; set; }

    }   
}
