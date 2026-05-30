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

        [SQLite.MaxLength(50)]
        public string Name { get; set; }

        [Unique]
        [SQLite.MaxLength(100)]
        public string Email { get; set; }

        [SQLite.MaxLength(100)]
        public string Password { get; set; }
    }
}