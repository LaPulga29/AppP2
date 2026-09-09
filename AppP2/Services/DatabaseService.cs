using SQLite;
using AppP2.Models;

namespace AppP2.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection? _database;
        private readonly string _dbPath;

        public DatabaseService()
        {
            _dbPath = Path.Combine(FileSystem.AppDataDirectory, "ProfeView.db3");
            InitializeDatabase();
        }

        private async void InitializeDatabase()
        {
            _database = new SQLiteAsyncConnection(_dbPath);
            await _database.CreateTableAsync<User>();
            await _database.CreateTableAsync<Professor>();
            await _database.CreateTableAsync<Review>();

            await SeedUsers();
        }

        // ==================== FACULTADES Y MATERIAS ====================
        public class FacultadInfo
        {
            public string Facultad { get; set; } = string.Empty;
            public List<string> Materias { get; set; } = new List<string>();
        }

        public List<FacultadInfo> GetFacultadesYMaterias()
        {
            return new List<FacultadInfo>
            {
                new FacultadInfo
                {
                    Facultad = "FACEA",
                    Materias = new List<string>
                    {
                        "Administración",
                        "Administración Financiera",
                        "Automatización de Procesos",
                        "Comercio Exterior",
                        "Contabilidad",
                        "Contabilidad de Costos",
                        "Contabilidad Intermedia",
                        "Dirección Estratégica",
                        "Diseño y Gestión de Proyectos",
                        "Finanzas Corporativas I",
                        "Gerencia de Servicios y Gobierno de TI",
                        "Introducción al Marketing",
                        "Investigación de Mercados",
                        "Marco Legal de los Negocios Digitales",
                        "Matemática Aplicada a la Administración y Economía I",
                        "Matemática Aplicada a la Administración y Economía II",
                        "Microeconomía I",
                        "Macroeconomía",
                        "Negocios Internacionales",
                        "Probabilidades y Estadística"
                    }
                },
                new FacultadInfo
                {
                    Facultad = "FICA",
                    Materias = new List<string>
                    {
                        "Algoritmos",
                        "Desarrollo Multiplataforma",
                        "Estadística Aplicada",
                        "Fundamentos de Ciberseguridad",
                        "Introducción a la Ingeniería en TI",
                        "Introducción a la Ingeniería de Software",
                        "Programación I",
                        "Programación II"
                    }
                },
                new FacultadInfo
                {
                    Facultad = "VIDA",
                    Materias = new List<string>
                    {
                        "Aprendizaje Estratégico y Liderazgo",
                        "Comunicación Efectiva",
                        "Interacción Efectiva en Sistemas Sociales",
                        "Pensamiento Crítico Aplicado"
                    }
                },
                new FacultadInfo
                {
                    Facultad = "INGLÉS",
                    Materias = new List<string>
                    {
                        "Inglés Intermedio I",
                        "Inglés Intermedio II",
                        "Inglés Avanzado I",
                        "Inglés Avanzado II"
                    }
                }
            };
        }

        public List<string> GetFacultadesList()
        {
            return GetFacultadesYMaterias().Select(f => f.Facultad).ToList();
        }

        public List<string> GetMateriasByFacultad(string facultad)
        {
            var facultadInfo = GetFacultadesYMaterias()
                .FirstOrDefault(f => f.Facultad == facultad);
            return facultadInfo?.Materias ?? new List<string>();
        }

        public List<string> GetMateriasList()
        {
            return GetFacultadesYMaterias().SelectMany(f => f.Materias).ToList();
        }

        // ==================== SEED DATA ====================
        private async Task SeedUsers()
        {
            var userCount = await _database.Table<User>().CountAsync();
            if (userCount > 0) return;

            var users = new List<User>
            {
                new User
                {
                    FullName = "Juan Pérez",
                    Email = "juan@test.com",
                    Password = "123456",
                    RegistrationDate = DateTime.Now
                },
                new User
                {
                    FullName = "María González",
                    Email = "maria@test.com",
                    Password = "123456",
                    RegistrationDate = DateTime.Now
                },
                new User
                {
                    FullName = "Carlos Rodríguez",
                    Email = "carlos@test.com",
                    Password = "123456",
                    RegistrationDate = DateTime.Now
                }
            };

            await _database.InsertAllAsync(users);
        }

        // ==================== USERS CRUD ====================
        public async Task<int> SaveUser(User user)
        {
            return await _database.InsertAsync(user);
        }

        public async Task<int> InsertUser(User user)
        {
            return await _database.InsertAsync(user);
        }

        public async Task<User> GetUserByEmail(string email)
        {
            return await _database.Table<User>().FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> GetUserById(int id)
        {
            return await _database.Table<User>().FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<List<User>> GetAllUsers()
        {
            return await _database.Table<User>().ToListAsync();
        }

        // ==================== PROFESSORS CRUD ====================
        public async Task<int> SaveProfessor(Professor professor)
        {
            return await _database.InsertAsync(professor);
        }

        public async Task<Professor> GetProfessor(int professorId)
        {
            return await _database.Table<Professor>().FirstOrDefaultAsync(p => p.Id == professorId);
        }

        public async Task<List<Professor>> GetAllProfessors()
        {
            return await _database.Table<Professor>().ToListAsync();
        }

        public async Task<List<Professor>> GetProfessorsWithReviews()
        {
            var professors = await _database.Table<Professor>().ToListAsync();
            var reviews = await _database.Table<Review>().ToListAsync();

            foreach (var professor in professors)
            {
                var professorReviews = reviews.Where(r => r.ProfessorId == professor.Id).ToList();
                professor.Reviews = professorReviews;
                professor.AverageRating = professorReviews.Any() ? professorReviews.Average(r => r.Rating) : 0;
            }

            return professors;
        }

        public async Task<Professor> GetProfessorWithReviews(int professorId)
        {
            var professor = await _database.Table<Professor>().FirstOrDefaultAsync(p => p.Id == professorId);
            if (professor != null)
            {
                var reviews = await _database.Table<Review>().Where(r => r.ProfessorId == professorId).ToListAsync();
                var users = await _database.Table<User>().ToListAsync();

                foreach (var review in reviews)
                {
                    var user = users.FirstOrDefault(u => u.Id == review.UserId);
                    review.UserName = user?.FullName ?? "Usuario desconocido";
                }

                professor.Reviews = reviews;
                professor.AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;
            }
            return professor;
        }

        // ==================== ELIMINAR PROFESOR CON RESEÑAS ====================
        public async Task<int> DeleteProfessorWithReviews(int professorId)
        {
            try
            {
                var reviews = await _database.Table<Review>().Where(r => r.ProfessorId == professorId).ToListAsync();
                foreach (var review in reviews)
                {
                    await _database.DeleteAsync(review);
                }

                var result = await _database.DeleteAsync<Professor>(professorId);
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al eliminar profesor: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> DeleteProfessor(int professorId)
        {
            return await _database.DeleteAsync<Professor>(professorId);
        }

        // ==================== FILTER METHODS ====================
        public async Task<List<Professor>> FilterProfessorsBySubject(string subject)
        {
            var professors = await GetProfessorsWithReviews();
            return professors.Where(p => p.Subject.Contains(subject, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public async Task<List<Professor>> FilterProfessorsByFaculty(string faculty)
        {
            var professors = await GetProfessorsWithReviews();
            return professors.Where(p => p.Faculty.Contains(faculty, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public async Task<List<Professor>> SearchProfessorsByName(string name)
        {
            var professors = await GetProfessorsWithReviews();
            return professors.Where(p => p.Name.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public async Task<List<Professor>> SearchProfessors(string searchTerm)
        {
            var professors = await GetProfessorsWithReviews();
            return professors.Where(p =>
                p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                p.Subject.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                p.Faculty.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        // ==================== REVIEWS CRUD ====================
        public async Task<int> SaveReview(Review review)
        {
            return await _database.InsertAsync(review);
        }

        public async Task<int> DeleteReview(int reviewId)
        {
            return await _database.DeleteAsync<Review>(reviewId);
        }

        public async Task<Review> GetReview(int reviewId)
        {
            return await _database.Table<Review>().FirstOrDefaultAsync(r => r.Id == reviewId);
        }

        public async Task<List<Review>> GetReviewsByProfessor(int professorId)
        {
            return await _database.Table<Review>().Where(r => r.ProfessorId == professorId).ToListAsync();
        }

        public async Task<List<Review>> GetReviewsByUser(int userId)
        {
            return await _database.Table<Review>().Where(r => r.UserId == userId).ToListAsync();
        }

        public async Task<bool> UserHasReviewedProfessor(int userId, int professorId)
        {
            var review = await _database.Table<Review>()
                .FirstOrDefaultAsync(r => r.UserId == userId && r.ProfessorId == professorId);
            return review != null;
        }

        public async Task ResetDatabase()
        {
            try
            {
                if (File.Exists(_dbPath))
                {
                    File.Delete(_dbPath);
                }
                _database = new SQLiteAsyncConnection(_dbPath);
                await _database.CreateTableAsync<User>();
                await _database.CreateTableAsync<Professor>();
                await _database.CreateTableAsync<Review>();

                await SeedUsers();

                System.Diagnostics.Debug.WriteLine("✅ Base de datos reseteada correctamente");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error en ResetDatabase: {ex.Message}");
            }
        }

        public async Task<bool> DatabaseExists()
        {
            return await Task.Run(() => File.Exists(_dbPath));
        }

        public string GetDatabasePath()
        {
            return _dbPath;
        }
    }
}