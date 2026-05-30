using SQLite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppP2.Models
{
    [SQLite.Table("Reviews")]
    public class Review
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int ProfessorId { get; set; }

        [SQLite.MaxLength(100)]
        public string UserEmail { get; set; }

        public int Rating { get; set; } // 1-5

        [SQLite.MaxLength(500)]
        public string Comment { get; set; }

        public DateTime Date { get; set; }
    }
}