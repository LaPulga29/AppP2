using SQLite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppP2.Models
{
    [SQLite.Table("Professors")]
    public class Professor
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [SQLite.MaxLength(100)]
        public string Name { get; set; }

        [SQLite.MaxLength(50)]
        public string Subject { get; set; }

        [SQLite.MaxLength(50)]
        public string Faculty { get; set; }

        public double AverageRating { get; set; }
    }
}