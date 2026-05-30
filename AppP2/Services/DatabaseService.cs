using SQLite;
using AppP2.Models;

namespace AppP2.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _database;
        private readonly string _dbPath;

        public DatabaseService()
        {
            _dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AppP2.db3");
            InitializeDatabase();
        }

        private async Task InitializeDatabase()
        {
            if (_database == null)
            {
                _database = new SQLiteAsyncConnection(_dbPath);
                await _database.CreateTableAsync<User>();
                await _database.CreateTableAsync<Professor>();
                await _database.CreateTableAsync<Review>();

                // Insertar datos de ejemplo
                await SeedData();
            }
        }

        private async Task SeedData()
        {
            var professors = await _database.Table<Professor>().ToListAsync();
            if (professors.Count == 0)
            {
                var professorsList = new List<Professor>
                {
                    new Professor { Name = "Dr. Luis Aguas", Subject = "Programación", Faculty = "Ingeniería", AverageRating = 4.5 },
                    new Professor { Name = "MSc. María López", Subject = "Base de Datos", Faculty = "Ingeniería", AverageRating = 4.2 },
                    new Professor { Name = "Ing. Carlos Pérez", Subject = "Redes", Faculty = "Ingeniería", AverageRating = 3.8 }
                };
                await _database.InsertAllAsync(professorsList);
            }
        }

        // Usuarios
        public Task<int> InsertUser(User user) => _database.InsertAsync(user);
        public Task<List<User>> GetUsers() => _database.Table<User>().ToListAsync();
        public Task<User> GetUserByEmail(string email) => _database.Table<User>().FirstOrDefaultAsync(u => u.Email == email);

        // Profesores
        public Task<List<Professor>> GetProfessors() => _database.Table<Professor>().ToListAsync();
        public Task<List<Professor>> GetProfessorsByFaculty(string faculty) => _database.Table<Professor>().Where(p => p.Faculty == faculty).ToListAsync();

        // Reseñas
        public Task<int> InsertReview(Review review) => _database.InsertAsync(review);
        public Task<List<Review>> GetReviewsByProfessor(int professorId) => _database.Table<Review>().Where(r => r.ProfessorId == professorId).ToListAsync();
    }
}