using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Recipe_Manager.Server.Models
{
    public class Recipe
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        public string? ImagePath { get; set; }

        public virtual User User { get; set; }
        public virtual List<Step>? Steps { get; set; }
        public virtual ICollection<Category>? Categories { get; set; }

    }
}
