using System.ComponentModel.DataAnnotations;

namespace Recipe_Manager.Server.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public virtual ICollection<Recipe>? Recipes { get; set; }
    }
}
