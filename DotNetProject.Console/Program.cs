using System.Text;
using Newtonsoft.Json;

namespace MovieConsole
{
    // ─── Models ──────────────────────────────────────────────────────────────────
    record MovieModel(int Id, string Title, string Genre, int? ReleaseYear, string Description, string PosterUrl, int? TmdbId, bool IsManual);
    record RatingModel(int Id, int UserId, int MovieID, int Score, string Review, DateTime? CreatedAt);
    record WatchListModel(int Id, int UserId, int MovieId, int Status, int CurrentEpisode, int TotalEpisodes, DateTime AddedAt);
    record UserModel(string UserName, string Email, string DisplayName);

    // ─── API Service ──────────────────────────────────────────────────────────────
    class ApiService
    {
        private static readonly HttpClient Http = new()
        {
            BaseAddress = new Uri("http://localhost:5134/api/"),
            Timeout = TimeSpan.FromSeconds(15)
        };

        public async Task<List<T>> GetAsync<T>(string url)
        {
            var res = await Http.GetAsync(url);
            res.EnsureSuccessStatusCode();
            var json = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<T>>(json) ?? new();
        }

        public async Task<T?> GetOneAsync<T>(string url)
        {
            var res = await Http.GetAsync(url);
            if (!res.IsSuccessStatusCode) return default;
            return JsonConvert.DeserializeObject<T>(await res.Content.ReadAsStringAsync());
        }

        public async Task<string> PostAsync(string url, object body)
        {
            var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
            var res = await Http.PostAsync(url, content);
            if (res.IsSuccessStatusCode)
            {
                var json = await res.Content.ReadAsStringAsync();
                return "✅ " + (json.Length > 200 ? "Success!" : json.Trim('"'));
            }
            return $"❌ Error: {res.StatusCode}";
        }

        public async Task<string> PutAsync(string url, object body)
        {
            var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
            var res = await Http.PutAsync(url, content);
            return res.IsSuccessStatusCode ? "✅ Updated successfully!" : $"❌ Error: {res.StatusCode}";
        }

        public async Task<string> DeleteBodyAsync(string url, object body)
        {
            var req = new HttpRequestMessage(HttpMethod.Delete, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
            };
            var res = await Http.SendAsync(req);
            return res.IsSuccessStatusCode ? "✅ Deleted successfully!" : $"❌ Error: {res.StatusCode}";
        }

        public async Task<string> DeleteAsync(string url)
        {
            var res = await Http.DeleteAsync(url);
            return res.IsSuccessStatusCode ? "✅ Deleted successfully!" : $"❌ Error: {res.StatusCode}";
        }

        public async Task<T?> PostGetAsync<T>(string url, object body)
        {
            var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
            var res = await Http.PostAsync(url, content);
            if (!res.IsSuccessStatusCode) return default;
            return JsonConvert.DeserializeObject<T>(await res.Content.ReadAsStringAsync());
        }
    }

    // ─── Console UI Helpers ───────────────────────────────────────────────────────
    static class UI
    {
        public static void Header(string title)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  " + new string('═', 58));
            Console.WriteLine($"  🎬  {title}");
            Console.WriteLine("  " + new string('═', 58));
            Console.ResetColor();
        }

        public static void Success(string msg) { Console.ForegroundColor = ConsoleColor.Green;   Console.WriteLine("  " + msg); Console.ResetColor(); }
        public static void Error(string msg)   { Console.ForegroundColor = ConsoleColor.Red;     Console.WriteLine("  " + msg); Console.ResetColor(); }
        public static void Info(string msg)    { Console.ForegroundColor = ConsoleColor.Yellow;  Console.WriteLine("  " + msg); Console.ResetColor(); }
        public static void Dim(string msg)     { Console.ForegroundColor = ConsoleColor.DarkGray;Console.WriteLine("  " + msg); Console.ResetColor(); }

        public static void Row(params string[] cols)
        {
            Console.ForegroundColor = ConsoleColor.White;
            var parts = cols.Select((c, i) => i == 0 ? (c ?? "-").PadRight(4) : (c ?? "-").PadRight(20));
            Console.WriteLine("  " + string.Join(" │ ", parts));
            Console.ResetColor();
        }

        public static void Divider() => Dim(new string('─', 60));

        public static string Ask(string prompt)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write($"  ▶ {prompt}: ");
            Console.ResetColor();
            return Console.ReadLine()?.Trim() ?? "";
        }

        public static int AskInt(string prompt, int defaultVal = 0)
        {
            var s = Ask(prompt);
            return int.TryParse(s, out var v) ? v : defaultVal;
        }

        public static void Pause()
        {
            Dim("\n  Press any key to continue...");
            Console.ReadKey(true);
        }

        public static string Menu(string title, string[] options)
        {
            Header(title);
            for (int i = 0; i < options.Length; i++)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write($"  [{i + 1}]");
                Console.ResetColor();
                Console.WriteLine($" {options[i]}");
            }
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("  [0] Back / Exit");
            Console.ResetColor();
            Console.WriteLine();
            return Ask("Choose");
        }
    }

    // ─── Main Program ─────────────────────────────────────────────────────────────
    class Program
    {
        static readonly ApiService Api = new();
        static string _currentUserName = "";
        static int _currentUserId = 0;

        static async Task Main()
        {
            Console.Title = "🎬 MyFlix Movie Console";
            Console.OutputEncoding = Encoding.UTF8;

            // Login
            if (!await LoginAsync()) return;

            while (true)
            {
                var options = new List<string> { "🎬 Movies", "⭐ My Ratings", "📋 My WatchList" };
                if (_currentUserName.Equals("admin", StringComparison.OrdinalIgnoreCase))
                    options.Add("👥 Users (Admin)");

                var choice = UI.Menu($"MyFlix — Welcome, {_currentUserName}!", options.ToArray());

                switch (choice)
                {
                    case "1": await MoviesMenu(); break;
                    case "2": await RatingsMenu(); break;
                    case "3": await WatchListMenu(); break;
                    case "4" when _currentUserName.Equals("admin", StringComparison.OrdinalIgnoreCase):
                        await UsersMenu(); break;
                    case "0":
                        UI.Info("\n  👋 Goodbye!\n");
                        return;
                    default:
                        UI.Error("Invalid choice."); break;
                }
            }
        }

        // ──────────────────────────── LOGIN ──────────────────────────────────────
        static async Task<bool> LoginAsync()
        {
            Console.Clear();
            UI.Header("MyFlix Movie Console — Login");
            Console.WriteLine();

            while (true)
            {
                var email = UI.Ask("Email or Username");
                var password = UI.Ask("Password");

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    UI.Error("Email and password are required."); continue;
                }

                // Admin shortcut
                if (email.Equals("admin", StringComparison.OrdinalIgnoreCase) && password == "admin")
                {
                    _currentUserName = "admin";
                    _currentUserId = 0;
                    UI.Success("✅ Logged in as Administrator!");
                    UI.Pause();
                    Console.Clear();
                    return true;
                }

                try
                {
                    var result = await Api.PostGetAsync<LoginResponse>("User/login", new { Email = email, Password = password });
                    if (result != null && result.IsSuccess)
                    {
                        _currentUserName = result.UserName;
                        UI.Success($"✅ Logged in as {result.DisplayName}!");
                        UI.Pause();
                        Console.Clear();
                        return true;
                    }
                    UI.Error(result?.Message ?? "Invalid email/username or password.");
                    UI.Info("  Try again or press Ctrl+C to exit.");
                }
                catch (Exception ex) { UI.Error($"Could not reach server: {ex.Message}"); return false; }
            }
        }

        record LoginResponse(bool IsSuccess, string Message, string UserName, string DisplayName, string Email);

        // ──────────────────────────── MOVIES ─────────────────────────────────────
        static async Task MoviesMenu()
        {
            var isAdmin = _currentUserName.Equals("admin", StringComparison.OrdinalIgnoreCase);
            var options = new List<string> { "List All Movies", "Search by Genre", "View by ID" };
            if (isAdmin)
            {
                options.Add("Add New Movie");
                options.Add("Update Movie");
                options.Add("Delete Movie");
            }

            while (true)
            {
                var choice = UI.Menu("🎬 MOVIES", options.ToArray());
                switch (choice)
                {
                    case "1": await ListMovies(); break;
                    case "2": await SearchMoviesByGenre(); break;
                    case "3": await ViewMovieById(); break;
                    case "4" when isAdmin: await AddMovie(); break;
                    case "5" when isAdmin: await UpdateMovie(); break;
                    case "6" when isAdmin: await DeleteMovie(); break;
                    case "0": return;
                    default: UI.Error("Invalid choice."); break;
                }
            }
        }

        static async Task ListMovies()
        {
            UI.Header("All Movies");
            try
            {
                var list = await Api.GetAsync<MovieModel>("Movie");
                if (list.Count == 0) { UI.Info("No movies found."); UI.Pause(); return; }
                UI.Row("ID", "Title", "Genre", "Year");
                UI.Divider();
                foreach (var m in list)
                    UI.Row(m.Id.ToString(), m.Title, m.Genre, m.ReleaseYear?.ToString() ?? "-");
                UI.Info($"\n  Total: {list.Count} movie(s)");
            }
            catch (Exception ex) { UI.Error($"❌ {ex.Message}"); }
            UI.Pause();
        }

        static async Task SearchMoviesByGenre()
        {
            var genre = UI.Ask("Genre (e.g. Action, Drama)");
            UI.Header($"Movies — Genre: {genre}");
            try
            {
                var list = await Api.GetAsync<MovieModel>($"Movie/genre/{genre}");
                if (list.Count == 0) { UI.Info("No movies found for this genre."); UI.Pause(); return; }
                UI.Row("ID", "Title", "Year", "Description");
                UI.Divider();
                foreach (var m in list)
                    UI.Row(m.Id.ToString(), m.Title, m.ReleaseYear?.ToString() ?? "-",
                           (m.Description ?? "").Length > 18 ? m.Description![..18] + "…" : m.Description ?? "-");
                UI.Info($"\n  Found: {list.Count} movie(s)");
            }
            catch (Exception ex) { UI.Error($"❌ {ex.Message}"); }
            UI.Pause();
        }

        static async Task ViewMovieById()
        {
            var id = UI.AskInt("Movie ID");
            try
            {
                var m = await Api.GetOneAsync<MovieModel>($"Movie/{id}");
                if (m == null) { UI.Error("Movie not found."); UI.Pause(); return; }
                UI.Header($"Movie #{m.Id}");
                Console.WriteLine($"  Title       : {m.Title}");
                Console.WriteLine($"  Genre       : {m.Genre}");
                Console.WriteLine($"  Release Year: {m.ReleaseYear ?? 0}");
                Console.WriteLine($"  TMDB Id     : {m.TmdbId ?? 0}");
                Console.WriteLine($"  Manual Entry: {(m.IsManual ? "Yes" : "No")}");
                Console.WriteLine($"  Poster URL  : {m.PosterUrl ?? "-"}");
                Console.WriteLine($"  Description : {m.Description ?? "-"}");
            }
            catch (Exception ex) { UI.Error($"❌ {ex.Message}"); }
            UI.Pause();
        }

        static async Task AddMovie()
        {
            UI.Header("Add New Movie");
            var title   = UI.Ask("Title");
            var genre   = UI.Ask("Genre");
            var year    = UI.AskInt("Release Year", DateTime.Now.Year);
            var tmdb    = UI.AskInt("TMDB Id (0 = none)", 0);
            var poster  = UI.Ask("Poster URL (leave blank to skip)");
            var desc    = UI.Ask("Description");
            var manual  = UI.Ask("Is Manual Entry? (y/n)").ToLower() == "y";

            try
            {
                var msg = await Api.PostAsync("Movie", new
                {
                    Title = title, Genre = genre, ReleaseYear = year,
                    TmdbId = tmdb == 0 ? (int?)null : tmdb,
                    PosterUrl = poster, Description = desc, IsManual = manual,
                    CreateAt = DateTime.UtcNow
                });
                UI.Success(msg);
            }
            catch (Exception ex) { UI.Error($"❌ {ex.Message}"); }
            UI.Pause();
        }

        static async Task UpdateMovie()
        {
            UI.Header("Update Movie");
            var id      = UI.AskInt("Movie ID to update");
            var title   = UI.Ask("New Title");
            var genre   = UI.Ask("New Genre");
            var year    = UI.AskInt("New Release Year", DateTime.Now.Year);
            var tmdb    = UI.AskInt("New TMDB Id (0 = none)", 0);
            var poster  = UI.Ask("New Poster URL");
            var desc    = UI.Ask("New Description");
            var manual  = UI.Ask("Is Manual Entry? (y/n)").ToLower() == "y";

            try
            {
                var msg = await Api.PutAsync("Movie", new
                {
                    Id = id, Title = title, Genre = genre, ReleaseYear = year,
                    TmdbId = tmdb == 0 ? (int?)null : tmdb,
                    PosterUrl = poster, Description = desc, IsManual = manual
                });
                UI.Success(msg);
            }
            catch (Exception ex) { UI.Error($"❌ {ex.Message}"); }
            UI.Pause();
        }

        static async Task DeleteMovie()
        {
            UI.Header("Delete Movie");
            var id = UI.AskInt("Movie ID to delete");
            UI.Info($"  Deleting movie #{id}...");
            try
            {
                var msg = await Api.DeleteBodyAsync("Movie", new { Id = id });
                UI.Success(msg);
            }
            catch (Exception ex) { UI.Error($"❌ {ex.Message}"); }
            UI.Pause();
        }

        // ──────────────────────────── RATINGS ────────────────────────────────────
        static async Task RatingsMenu()
        {
            while (true)
            {
                var choice = UI.Menu("⭐ RATINGS", new[]
                {
                    "View My Ratings",
                    "Add / Rate a Movie",
                    "Update a Rating",
                    "Delete a Rating"
                });
                switch (choice)
                {
                    case "1": await ListRatings(); break;
                    case "2": await AddRating(); break;
                    case "3": await UpdateRating(); break;
                    case "4": await DeleteRating(); break;
                    case "0": return;
                    default: UI.Error("Invalid choice."); break;
                }
            }
        }

        static async Task ListRatings()
        {
            UI.Header("My Ratings");
            try
            {
                var list = await Api.GetAsync<RatingModel>($"Rating/userid?userid={_currentUserId}");
                if (list.Count == 0) { UI.Info("No ratings found."); UI.Pause(); return; }
                UI.Row("ID", "Movie ID", "Score", "Review");
                UI.Divider();
                foreach (var r in list)
                    UI.Row(r.Id.ToString(), r.MovieID.ToString(), $"⭐ {r.Score}/10",
                           (r.Review ?? "").Length > 18 ? r.Review![..18] + "…" : r.Review ?? "-");
                UI.Info($"\n  Total: {list.Count} rating(s)");
            }
            catch (Exception ex) { UI.Error($"❌ {ex.Message}"); }
            UI.Pause();
        }

        static async Task AddRating()
        {
            UI.Header("Rate a Movie");
            var movieId = UI.AskInt("Movie ID");
            var score   = UI.AskInt("Score (1–10)", 5);
            if (score < 1 || score > 10) { UI.Error("Score must be 1–10."); UI.Pause(); return; }
            var review  = UI.Ask("Review (optional)");

            try
            {
                var msg = await Api.PostAsync("Rating", new
                {
                    UserId = _currentUserId, MovieID = movieId,
                    Score = score, Review = review, CreatedAt = DateTime.UtcNow
                });
                UI.Success(msg);
            }
            catch (Exception ex) { UI.Error($"❌ {ex.Message}"); }
            UI.Pause();
        }

        static async Task UpdateRating()
        {
            UI.Header("Update Rating");
            var id     = UI.AskInt("Rating ID to update");
            var score  = UI.AskInt("New Score (1–10)", 5);
            var review = UI.Ask("New Review");

            try
            {
                var msg = await Api.PutAsync("Rating", new
                {
                    Id = id, Score = score, Review = review, UpdatedAt = DateTime.UtcNow
                });
                UI.Success(msg);
            }
            catch (Exception ex) { UI.Error($"❌ {ex.Message}"); }
            UI.Pause();
        }

        static async Task DeleteRating()
        {
            UI.Header("Delete Rating");
            var id = UI.AskInt("Rating ID to delete");
            try
            {
                var msg = await Api.DeleteBodyAsync("Rating", new { Id = id });
                UI.Success(msg);
            }
            catch (Exception ex) { UI.Error($"❌ {ex.Message}"); }
            UI.Pause();
        }

        // ──────────────────────────── WATCHLIST ──────────────────────────────────
        static async Task WatchListMenu()
        {
            while (true)
            {
                var choice = UI.Menu("📋 WATCHLIST", new[]
                {
                    "View My WatchList",
                    "Add Movie to WatchList",
                    "Update WatchList Status",
                    "Remove from WatchList"
                });
                switch (choice)
                {
                    case "1": await ListWatchList(); break;
                    case "2": await AddToWatchList(); break;
                    case "3": await UpdateWatchList(); break;
                    case "4": await DeleteWatchList(); break;
                    case "0": return;
                    default: UI.Error("Invalid choice."); break;
                }
            }
        }

        static string WatchStatusName(int s) => s switch
        {
            1 => "Want to Watch",
            2 => "Watching",
            3 => "Completed",
            4 => "Dropped",
            _ => "Unknown"
        };

        static async Task ListWatchList()
        {
            UI.Header("My WatchList");
            try
            {
                var list = await Api.GetAsync<WatchListModel>($"WatchList/UserId?userId={_currentUserId}");
                if (list.Count == 0) { UI.Info("Your watchlist is empty."); UI.Pause(); return; }
                UI.Row("ID", "Movie ID", "Status", "Progress");
                UI.Divider();
                foreach (var w in list)
                    UI.Row(w.Id.ToString(), w.MovieId.ToString(),
                           WatchStatusName(w.Status), $"{w.CurrentEpisode}/{w.TotalEpisodes} ep");
                UI.Info($"\n  Total: {list.Count} item(s)");
            }
            catch (Exception ex) { UI.Error($"❌ {ex.Message}"); }
            UI.Pause();
        }

        static async Task AddToWatchList()
        {
            UI.Header("Add to WatchList");
            var movieId  = UI.AskInt("Movie ID");
            Console.WriteLine();
            UI.Info("  Status options:");
            UI.Dim("  [1] Want to Watch  [2] Watching  [3] Completed  [4] Dropped");
            var status   = UI.AskInt("Status (1–4)", 1);
            var curEp    = UI.AskInt("Current Episode (0 if not started)", 0);
            var totalEp  = UI.AskInt("Total Episodes (0 if unknown)", 0);

            try
            {
                var msg = await Api.PostAsync($"WatchList/userid?userid={_currentUserId}", new
                {
                    MovieId = movieId, Status = status,
                    CurrentEpisode = curEp, TotalEpisodes = totalEp
                });
                UI.Success(msg);
            }
            catch (Exception ex) { UI.Error($"❌ {ex.Message}"); }
            UI.Pause();
        }

        static async Task UpdateWatchList()
        {
            UI.Header("Update WatchList Item");
            var id = UI.AskInt("WatchList Item ID");
            UI.Info("  Status options:");
            UI.Dim("  [1] Want to Watch  [2] Watching  [3] Completed  [4] Dropped");
            var status  = UI.AskInt("New Status (1–4)", 2);
            var curEp   = UI.AskInt("Current Episode", 0);
            var totalEp = UI.AskInt("Total Episodes", 0);

            try
            {
                var msg = await Api.PutAsync($"WatchList/id?id={id}", new
                {
                    Status = status, CurrentEpisode = curEp, TotalEpisodes = totalEp
                });
                UI.Success(msg);
            }
            catch (Exception ex) { UI.Error($"❌ {ex.Message}"); }
            UI.Pause();
        }

        static async Task DeleteWatchList()
        {
            UI.Header("Remove from WatchList");
            var id = UI.AskInt("WatchList Item ID to remove");
            try
            {
                var msg = await Api.DeleteAsync($"WatchList/id?id={id}");
                UI.Success(msg);
            }
            catch (Exception ex) { UI.Error($"❌ {ex.Message}"); }
            UI.Pause();
        }

        // ──────────────────────────── USERS (Admin) ───────────────────────────────
        static async Task UsersMenu()
        {
            while (true)
            {
                var choice = UI.Menu("👥 USERS — Admin", new[]
                {
                    "List All Users",
                    "View User by ID"
                });
                switch (choice)
                {
                    case "1": await ListUsers(); break;
                    case "2": await ViewUserById(); break;
                    case "0": return;
                    default: UI.Error("Invalid choice."); break;
                }
            }
        }

        static async Task ListUsers()
        {
            UI.Header("All Users");
            try
            {
                var list = await Api.GetAsync<UserModel>("User");
                if (list.Count == 0) { UI.Info("No users found."); UI.Pause(); return; }
                UI.Row("Username", "Display Name", "Email");
                UI.Divider();
                foreach (var u in list)
                    UI.Row(u.UserName, u.DisplayName ?? "-", u.Email ?? "-");
                UI.Info($"\n  Total: {list.Count} user(s)");
            }
            catch (Exception ex) { UI.Error($"❌ {ex.Message}"); }
            UI.Pause();
        }

        static async Task ViewUserById()
        {
            var id = UI.AskInt("User ID");
            try
            {
                var u = await Api.GetOneAsync<UserModel>($"User/{id}");
                if (u == null) { UI.Error("User not found."); UI.Pause(); return; }
                UI.Header($"User: {u.UserName}");
                Console.WriteLine($"  Username    : {u.UserName}");
                Console.WriteLine($"  Display Name: {u.DisplayName ?? "-"}");
                Console.WriteLine($"  Email       : {u.Email ?? "-"}");
            }
            catch (Exception ex) { UI.Error($"❌ {ex.Message}"); }
            UI.Pause();
        }
    }
}
