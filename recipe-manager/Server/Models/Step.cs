using System.ComponentModel.DataAnnotations;

namespace Recipe_Manager.Server.Models
{
    public class Step
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Desciption { get; set; }
        [Required]
        public int Order { get; set; }
        public string? ImagePath { get; set; }
        public int RecipeId { get; set; }

        public virtual Recipe Recipe { get; set; }
        public virtual List<Ingriedient>? Ingriedients { get; set; }
    }
}
