namespace Recipe_Manager.Server.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

        public string Email { get; set; }

        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }

        public virtual List<Recipe> Recipes { get; set; }
        public virtual List<IdentityRole> Roles { get; set; }

    }
}
