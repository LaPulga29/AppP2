using SQLite;

namespace AppP2.Models
{
    [Table("Reviews")]
    public class Review
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int ProfessorId { get; set; }

        public int UserId { get; set; }

        public int Rating { get; set; } // 1-5

        [MaxLength(1000)]
        public string Comment { get; set; } = string.Empty;

        public DateTime Date { get; set; } = DateTime.Now;

        [Ignore]
        public string UserName { get; set; } = string.Empty;

        [Ignore]
        public string ProfessorName { get; set; } = string.Empty;

        [Ignore]
        public bool IsOwnReview { get; set; }
    }
}