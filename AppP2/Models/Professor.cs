using SQLite;

namespace AppP2.Models
{
    [Table("Professors")]
    public class Professor
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Subject { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Faculty { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Bio { get; set; }

        [Ignore]
        public double AverageRating { get; set; }

        [Ignore]
        public List<Review> Reviews { get; set; } = new List<Review>();
    }
}