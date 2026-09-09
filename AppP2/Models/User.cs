using SQLite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppP2.Models
{
    [SQLite.Table("Users")]
    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [SQLite.MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [SQLite.MaxLength(100)]
        [Unique]
        public string Email { get; set; } = string.Empty;

        [SQLite.MaxLength(100)]
        public string Password { get; set; } = string.Empty;

        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        [Ignore]
        public List<Review> Reviews { get; set; } = new List<Review>();
        public string Name { get; internal set; }
    }
}