using DotNetProject.WebApi.Features.Movies;
using DotNetProject.WebApi.Features.Users;
using DotNetProject.WebApi.Features.Ratings;
using DotNetProject.WebApi.Features.WatchList;
using DotNetProject.WindowForm.Services;

namespace DotNetProject.WindowForm
{
    internal partial class Form1 : Form
    {
        private readonly HttpClientServices _httpClientServices;

        public UserModel CurrentUser { get; set; }

        private string ApiBaseUrl => txtApiBaseUrl.Text.Trim().TrimEnd('/');

        public Form1(HttpClientServices httpClientServices)
        {
            InitializeComponent();
            _httpClientServices = httpClientServices;

            Load += Form1_Load;
            WireMovieEvents();
            WireUserEvents();
            WireRatingEvents();
            WireWatchListEvents();

            // Set grid formatting and hide unwanted system/internal columns automatically
            dgvUsers.DataBindingComplete += (s, e) => ConfigureGridColumns(dgvUsers);
            dgvRatings.DataBindingComplete += (s, e) => ConfigureGridColumns(dgvRatings);
            dgvWatchList.DataBindingComplete += (s, e) => ConfigureGridColumns(dgvWatchList);
        }

        private void ConfigureGridColumns(DataGridView dgv)
        {
            var colsToHide = new List<string> { "Password", "IsDeleted", "DeletedAt", "CreatedAt", "UpdatedAt", "AddedAt", "CreatedDate" };
            if (dgv == dgvRatings)
            {
                colsToHide.Add("Id"); // Hide Rating ID column as requested
            }
            foreach (var colName in colsToHide)
            {
                if (dgv.Columns.Contains(colName))
                {
                    dgv.Columns[colName].Visible = false;
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            statusLabel.Text = CurrentUser != null
                ? $"Logged in as {CurrentUser.DisplayName ?? CurrentUser.UserName}"
                : "Ready";

            // Hide Rating ID inputs from user view in both admin and user modes
            lblRatingId.Visible = false;
            txtRatingId.Visible = false;

            ApplyRoleBasedUI();
            ApplyTabStyle();
        }

        private void ApplyTabStyle()
        {
            this.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            tabControlMain.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            tabControlMain.ItemSize = new System.Drawing.Size(130, 36);
            tabControlMain.Padding = new System.Drawing.Point(16, 8);
            tabControlMain.DrawItem += (s, e) =>
            {
                var g = e.Graphics;
                var tab = tabControlMain.TabPages[e.Index];
                bool selected = e.Index == tabControlMain.SelectedIndex;
                var bg = selected
                    ? System.Drawing.Color.FromArgb(37, 99, 235)
                    : System.Drawing.Color.FromArgb(226, 232, 240);
                var fg = selected
                    ? System.Drawing.Color.White
                    : System.Drawing.Color.FromArgb(71, 85, 105);
                using var br = new System.Drawing.SolidBrush(bg);
                g.FillRectangle(br, e.Bounds);
                var sf = new System.Drawing.StringFormat
                {
                    Alignment = System.Drawing.StringAlignment.Center,
                    LineAlignment = System.Drawing.StringAlignment.Center
                };
                using var tb = new System.Drawing.SolidBrush(fg);
                using var fnt = new System.Drawing.Font("Segoe UI", 9.5f,
                    selected ? System.Drawing.FontStyle.Bold : System.Drawing.FontStyle.Regular);
                g.DrawString(tab.Text, fnt, tb, e.Bounds, sf);
            };
        }

        private void ApplyRoleBasedUI()
        {
            bool isAdmin = CurrentUser?.UserName?.Equals("admin", StringComparison.OrdinalIgnoreCase) == true;

            // Users tab — admin only
            if (!isAdmin)
            {
                if (tabControlMain.TabPages.Contains(tabUsers))
                {
                    tabControlMain.TabPages.Remove(tabUsers);
                }
            }
            else
            {
                if (!tabControlMain.TabPages.Contains(tabUsers))
                {
                    tabControlMain.TabPages.Insert(1, tabUsers);
                }
            }

            // Movie Details panel — Always visible so normal users can see selected movie details, but read-only
            pnlMovieDetails.Visible = true;
            txtMovieId.ReadOnly = true;
            txtMovieTitle.ReadOnly = !isAdmin;
            txtMovieGenre.ReadOnly = !isAdmin;
            txtMoviePosterUrl.ReadOnly = !isAdmin;
            txtMovieDescription.ReadOnly = !isAdmin;
            numMovieReleaseYear.Enabled = isAdmin;
            numMovieTmdbId.Enabled = isAdmin;
            chkMovieIsManual.Enabled = isAdmin;

            // Movie buttons — non-admin can see Load/Search/Get but not Add/Update/Delete/Clear
            btnMovieAdd.Visible = isAdmin;
            btnMovieUpdate.Visible = isAdmin;
            btnMovieDelete.Visible = isAdmin;
            btnMovieClear.Visible = isAdmin;
            pnlMovieButtons.Visible = true;

            // User management panel — admin only
            pnlUserDetails.Visible = isAdmin;
            pnlUserButtons.Visible = isAdmin;

            // Auto-set and disable user ID for normal users in Ratings/Watchlist tabs
            if (CurrentUser != null)
            {
                if (CurrentUser.Id >= numRatingUserId.Minimum && CurrentUser.Id <= numRatingUserId.Maximum)
                    numRatingUserId.Value = CurrentUser.Id;
                if (CurrentUser.Id >= numWatchListUserId.Minimum && CurrentUser.Id <= numWatchListUserId.Maximum)
                    numWatchListUserId.Value = CurrentUser.Id;

                numRatingUserId.Enabled = isAdmin;
                numWatchListUserId.Enabled = isAdmin;
            }

            // Auto-load movies for all users upon login
            _ = MovieLoadAll();
        }

        public static string ShowInputDialog(string text, string caption, string defaultVal = "")
        {
            Form prompt = new Form()
            {
                Width = 400,
                Height = 165,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(241, 245, 249)
            };
            Label textLabel = new Label() { Left = 20, Top = 20, Text = text, Width = 360, AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(30, 41, 59) };
            TextBox textBox = new TextBox() { Left = 20, Top = 45, Width = 340, Text = defaultVal, Font = new Font("Segoe UI", 9.5F) };
            Button confirmation = new Button() { Text = "OK", Left = 160, Width = 90, Top = 82, DialogResult = DialogResult.OK, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            Button cancel = new Button() { Text = "Cancel", Left = 270, Width = 90, Top = 82, DialogResult = DialogResult.Cancel, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(226, 232, 240), ForeColor = Color.FromArgb(71, 85, 105), Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            
            confirmation.FlatAppearance.BorderSize = 0;
            cancel.FlatAppearance.BorderSize = 0;

            confirmation.Click += (sender, e) => { prompt.Close(); };
            cancel.Click += (sender, e) => { prompt.Close(); };
            
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(cancel);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;
            prompt.CancelButton = cancel;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }

        private void SetStatus(string message, bool isError = false)
        {
            statusLabel.Text = message;
            statusLabel.ForeColor = isError ? Color.Crimson : Color.Black;
        }

        // ==========================================================
        // MOVIES  (Netflix-style poster gallery)
        // ==========================================================
        private Panel _selectedMovieCard;
        private static readonly Size PosterCardSize = new Size(160, 280);
        private static readonly Size PosterImageSize = new Size(144, 190);

        private void WireMovieEvents()
        {
            btnMovieLoadAll.Click += async (s, e) => await MovieLoadAll();
            btnMovieGetById.Click += async (s, e) => await MovieGetById();
            btnMovieSearchGenre.Click += async (s, e) => await MovieSearchByGenre();
            btnMovieAdd.Click += async (s, e) => await MovieAdd();
            btnMovieUpdate.Click += async (s, e) => await MovieUpdate();
            btnMovieDelete.Click += async (s, e) => await MovieDelete();
            btnMovieClear.Click += (s, e) => MovieClearFields();
        }

        private void MovieClearFields()
        {
            txtMovieId.Clear();
            txtMovieTitle.Clear();
            txtMovieGenre.Clear();
            txtMoviePosterUrl.Clear();
            txtMovieDescription.Clear();
            numMovieReleaseYear.Value = numMovieReleaseYear.Minimum >= 2024 ? numMovieReleaseYear.Minimum : 2024;
            numMovieTmdbId.Value = 0;
            chkMovieIsManual.Checked = false;
            if (_selectedMovieCard != null)
            {
                _selectedMovieCard.BackColor = Color.White;
                _selectedMovieCard = null;
            }
        }

        // Redraws the poster gallery from a fresh list of movies.
        private void MovieRenderGallery(List<MovieModel> movies)
        {
            flpMoviePosters.SuspendLayout();
            flpMoviePosters.Controls.Clear();
            _selectedMovieCard = null;

            if (movies != null)
            {
                foreach (var movie in movies)
                {
                    flpMoviePosters.Controls.Add(MovieCreateCard(movie));
                }
            }

            flpMoviePosters.ResumeLayout();
        }

        // Builds one Netflix-style poster card: image on top, title + genre/year below.
        private Panel MovieCreateCard(MovieModel movie)
        {
            var card = new Panel
            {
                Size = PosterCardSize,
                Margin = new Padding(10),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Cursor = Cursors.Hand,
                Tag = movie
            };

            var picture = new PictureBox
            {
                Size = PosterImageSize,
                Location = new Point(8, 8),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(40, 40, 40),
                Cursor = Cursors.Hand
            };

            if (!string.IsNullOrWhiteSpace(movie.PosterUrl) &&
                Uri.TryCreate(movie.PosterUrl, UriKind.Absolute, out _))
            {
                picture.Image = MovieCreatePlaceholder(movie.Title);
                picture.LoadCompleted += (s, e) =>
                {
                    if (e.Error != null) picture.Image = MovieCreatePlaceholder(movie.Title);
                };
                try { picture.LoadAsync(movie.PosterUrl); }
                catch { picture.Image = MovieCreatePlaceholder(movie.Title); }
            }
            else
            {
                picture.Image = MovieCreatePlaceholder(movie.Title);
            }

            var lblTitle = new Label
            {
                Text = string.IsNullOrWhiteSpace(movie.Title) ? "(untitled)" : movie.Title,
                Location = new Point(8, 202),
                Size = new Size(144, 38),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                AutoEllipsis = true
            };

            var metaParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(movie.Genre)) metaParts.Add(movie.Genre);
            if (movie.ReleaseYear.HasValue) metaParts.Add(movie.ReleaseYear.Value.ToString());
            var lblMeta = new Label
            {
                Text = string.Join("  •  ", metaParts),
                Location = new Point(8, 240),
                Size = new Size(144, 20),
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 8F),
                AutoEllipsis = true
            };

            var lblManual = new Label
            {
                Text = movie.IsManual ? "Manual entry" : "",
                Location = new Point(8, 258),
                Size = new Size(144, 16),
                ForeColor = Color.SteelBlue,
                Font = new Font("Segoe UI", 7.5F, FontStyle.Italic)
            };

            card.Controls.Add(picture);
            card.Controls.Add(lblTitle);
            card.Controls.Add(lblMeta);
            card.Controls.Add(lblManual);

            EventHandler selectHandler = (s, e) => MovieSelectCard(movie, card);
            card.Click += selectHandler;
            picture.Click += selectHandler;
            lblTitle.Click += selectHandler;
            lblMeta.Click += selectHandler;

            return card;
        }

        // Draws a simple dark placeholder poster with the title, used while loading or on failure.
        private Image MovieCreatePlaceholder(string title)
        {
            var bmp = new Bitmap(PosterImageSize.Width, PosterImageSize.Height);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.FromArgb(64, 64, 64));
                using var font = new Font("Segoe UI", 9F, FontStyle.Bold);
                var text = string.IsNullOrWhiteSpace(title) ? "No Poster" : title;
                var rect = new RectangleF(6, 6, bmp.Width - 12, bmp.Height - 12);
                using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(text, font, Brushes.White, rect, sf);
            }
            return bmp;
        }

        // Clicking a poster card selects it (highlight) and loads its data into the edit form.
        // Note: Id still can't be auto-filled because the WebApi's MovieModel doesn't include it.
        private void MovieSelectCard(MovieModel movie, Panel card)
        {
            if (_selectedMovieCard != null) _selectedMovieCard.BackColor = Color.White;
            _selectedMovieCard = card;
            card.BackColor = Color.FromArgb(224, 236, 255);

            txtMovieId.Text = movie.Id.ToString();
            txtMovieTitle.Text = movie.Title;
            txtMovieGenre.Text = movie.Genre;
            txtMoviePosterUrl.Text = movie.PosterUrl;
            txtMovieDescription.Text = movie.Description;
            if (movie.ReleaseYear.HasValue && movie.ReleaseYear.Value >= numMovieReleaseYear.Minimum
                && movie.ReleaseYear.Value <= numMovieReleaseYear.Maximum)
                numMovieReleaseYear.Value = movie.ReleaseYear.Value;
            if (movie.TmdbId.HasValue) numMovieTmdbId.Value = Math.Min((decimal)movie.TmdbId.Value, numMovieTmdbId.Maximum);
            chkMovieIsManual.Checked = movie.IsManual;

            // Auto-fill selected movie Id into Ratings and WatchList tabs for user convenience
            if (movie.Id >= numRatingMovieId.Minimum && movie.Id <= numRatingMovieId.Maximum)
                numRatingMovieId.Value = movie.Id;
            if (movie.Id >= numWatchListMovieId.Minimum && movie.Id <= numWatchListMovieId.Maximum)
                numWatchListMovieId.Value = movie.Id;
        }

        private async Task MovieLoadAll()
        {
            try
            {
                var movies = await _httpClientServices.ExectueAsync<List<MovieModel>>(
                    $"{ApiBaseUrl}/Movie", null, EnumHttpMethod.Get);
                MovieRenderGallery(movies);
                SetStatus($"Loaded {movies?.Count ?? 0} movie(s).");
            }
            catch (Exception ex)
            {
                SetStatus("Failed to load movies: " + ex.Message, true);
            }
        }

        private async Task MovieGetById()
        {
            string input = ShowInputDialog("Enter Movie ID:", "Get Movie By ID", txtMovieId.Text);
            if (string.IsNullOrWhiteSpace(input)) return;

            if (!int.TryParse(input, out var id))
            {
                SetStatus("Enter a valid numeric movie Id.", true);
                return;
            }
            try
            {
                var movie = await _httpClientServices.ExectueAsync<MovieModel>(
                    $"{ApiBaseUrl}/Movie/{id}", null, EnumHttpMethod.Get);
                MovieRenderGallery(movie == null ? null : new List<MovieModel> { movie });
                SetStatus(movie == null ? "Movie not found." : "Movie loaded.");
            }
            catch (Exception ex)
            {
                SetStatus("Failed to get movie: " + ex.Message, true);
            }
        }

        private async Task MovieSearchByGenre()
        {
            string genre = ShowInputDialog("Enter Genre:", "Search Movies by Genre", txtMovieGenre.Text);
            if (string.IsNullOrWhiteSpace(genre)) return;

            try
            {
                var movies = await _httpClientServices.ExectueAsync<List<MovieModel>>(
                    $"{ApiBaseUrl}/Movie/genre/{Uri.EscapeDataString(genre)}", null, EnumHttpMethod.Get);
                MovieRenderGallery(movies);
                SetStatus($"Found {movies?.Count ?? 0} movie(s) in genre '{genre}'.");
            }
            catch (Exception ex)
            {
                SetStatus("Search failed: " + ex.Message, true);
            }
        }

        private async Task MovieAdd()
        {
            try
            {
                var request = new MovieCreateRequestModel
                {
                    Title = txtMovieTitle.Text.Trim(),
                    Genre = txtMovieGenre.Text.Trim(),
                    ReleaseYear = (int)numMovieReleaseYear.Value,
                    Description = txtMovieDescription.Text.Trim(),
                    PosterUrl = txtMoviePosterUrl.Text.Trim(),
                    TmdbId = (int)numMovieTmdbId.Value,
                    IsManual = chkMovieIsManual.Checked
                };
                var result = await _httpClientServices.ExectueAsync<MovieCreateResponseModel>(
                    $"{ApiBaseUrl}/Movie", request, EnumHttpMethod.Post);
                SetStatus(result?.Message ?? "Movie added.", result?.IsSuccess != "true");
                await MovieLoadAll();
            }
            catch (Exception ex)
            {
                SetStatus("Failed to add movie: " + ex.Message, true);
            }
        }

        private async Task MovieUpdate()
        {
            if (!int.TryParse(txtMovieId.Text, out var id))
            {
                SetStatus("Enter the Id of the movie to update.", true);
                return;
            }
            try
            {
                var request = new MovieUpdateRequestModel
                {
                    Id = id,
                    Title = txtMovieTitle.Text.Trim(),
                    Genre = txtMovieGenre.Text.Trim(),
                    ReleaseYear = (int)numMovieReleaseYear.Value,
                    Description = txtMovieDescription.Text.Trim(),
                    PosterUrl = txtMoviePosterUrl.Text.Trim(),
                    TmdbId = (int)numMovieTmdbId.Value,
                    IsManual = chkMovieIsManual.Checked
                };
                var result = await _httpClientServices.ExectueAsync<MovieUpdateResponseModel>(
                    $"{ApiBaseUrl}/Movie", request, EnumHttpMethod.Put);
                SetStatus(result?.Message ?? "Movie updated.", result?.IsSuccess != "true");
                await MovieLoadAll();
            }
            catch (Exception ex)
            {
                SetStatus("Failed to update movie: " + ex.Message, true);
            }
        }

        private async Task MovieDelete()
        {
            if (!int.TryParse(txtMovieId.Text, out var id))
            {
                SetStatus("Enter the Id of the movie to delete.", true);
                return;
            }
            if (MessageBox.Show($"Delete movie Id {id}?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;
            try
            {
                var request = new MovieDeleteRequestModel { Id = id };
                var result = await _httpClientServices.ExectueAsync<MovieDeleteResponseModel>(
                    $"{ApiBaseUrl}/Movie", request, EnumHttpMethod.Delete);
                SetStatus(result?.Message ?? "Movie deleted.", result?.IsSuccess != "true");
                await MovieLoadAll();
            }
            catch (Exception ex)
            {
                SetStatus("Failed to delete movie: " + ex.Message, true);
            }
        }

        // ==========================================================
        // USERS
        // ==========================================================
        private void WireUserEvents()
        {
            btnUserLoadAll.Click += async (s, e) => await UserLoadAll();
            btnUserGetById.Click += async (s, e) => await UserGetById();
            btnUserAdd.Click += async (s, e) => await UserAdd();
            btnUserUpdate.Click += async (s, e) => await UserUpdate();
            btnUserClear.Click += (s, e) => UserClearFields();
            dgvUsers.SelectionChanged += (s, e) => UserPopulateFromGrid();
        }

        private void UserClearFields()
        {
            txtUserId.Clear();
            txtUserName.Clear();
            txtUserEmail.Clear();
            txtUserPassword.Clear();
            txtUserDisplayName.Clear();
        }

        private void UserPopulateFromGrid()
        {
            if (dgvUsers.CurrentRow?.DataBoundItem is not UserModel user) return;
            txtUserName.Text = user.UserName;
            txtUserEmail.Text = user.Email;
            txtUserDisplayName.Text = user.DisplayName;
        }

        private async Task UserLoadAll()
        {
            try
            {
                var users = await _httpClientServices.ExectueAsync<List<UserModel>>(
                    $"{ApiBaseUrl}/User", null, EnumHttpMethod.Get);
                dgvUsers.DataSource = users;
                SetStatus($"Loaded {users?.Count ?? 0} user(s).");
            }
            catch (Exception ex)
            {
                SetStatus("Failed to load users: " + ex.Message, true);
            }
        }

        private async Task UserGetById()
        {
            if (!int.TryParse(txtUserId.Text, out var id))
            {
                SetStatus("Enter a valid numeric user Id first.", true);
                return;
            }
            try
            {
                var user = await _httpClientServices.ExectueAsync<UserModel>(
                    $"{ApiBaseUrl}/User/id?id={id}", null, EnumHttpMethod.Get);
                dgvUsers.DataSource = user == null ? null : new List<UserModel> { user };
                SetStatus(user == null ? "User not found." : "User loaded.");
            }
            catch (Exception ex)
            {
                SetStatus("Failed to get user: " + ex.Message, true);
            }
        }

        private async Task UserAdd()
        {
            try
            {
                var request = new UserCreateRequestModel
                {
                    UserName = txtUserName.Text.Trim(),
                    Email = txtUserEmail.Text.Trim(),
                    Password = txtUserPassword.Text,
                    DisplayName = txtUserDisplayName.Text.Trim()
                };
                var result = await _httpClientServices.ExectueAsync<UserCreateResponseModel>(
                    $"{ApiBaseUrl}/User", request, EnumHttpMethod.Post);
                SetStatus(result?.Message ?? "User added.", result?.IsSuccess != true);
                await UserLoadAll();
            }
            catch (Exception ex)
            {
                SetStatus("Failed to add user: " + ex.Message, true);
            }
        }

        private async Task UserUpdate()
        {
            if (!int.TryParse(txtUserId.Text, out var id))
            {
                SetStatus("Enter the Id of the user to update.", true);
                return;
            }
            try
            {
                var request = new UserUpdateRequestModel
                {
                    Id = id,
                    UserName = txtUserName.Text.Trim(),
                    Email = txtUserEmail.Text.Trim(),
                    Password = txtUserPassword.Text,
                    DisplayName = txtUserDisplayName.Text.Trim()
                };
                var result = await _httpClientServices.ExectueAsync<UserUpdateResponseModel>(
                    $"{ApiBaseUrl}/User", request, EnumHttpMethod.Put);
                SetStatus(result?.Message ?? "User updated.", result?.IsSuccess != true);
                await UserLoadAll();
            }
            catch (Exception ex)
            {
                SetStatus("Failed to update user: " + ex.Message, true);
            }
        }

        // ==========================================================
        // RATINGS
        // ==========================================================
        private void WireRatingEvents()
        {
            btnRatingLoadByUser.Click += async (s, e) => await RatingLoadByUser();
            btnRatingAdd.Click += async (s, e) => await RatingAdd();
            btnRatingUpdate.Click += async (s, e) => await RatingUpdate();
            btnRatingDelete.Click += async (s, e) => await RatingDelete();
            btnRatingClear.Click += (s, e) => RatingClearFields();
            dgvRatings.SelectionChanged += (s, e) => RatingPopulateFromGrid();
        }

        private void RatingClearFields()
        {
            txtRatingId.Clear();
            numRatingUserId.Value = 0;
            numRatingMovieId.Value = 0;
            numRatingScore.Value = numRatingScore.Minimum;
            txtRatingReview.Clear();
        }

        private void RatingPopulateFromGrid()
        {
            if (dgvRatings.CurrentRow?.DataBoundItem is not RatingModel rating) return;
            txtRatingId.Text = rating.Id.ToString();
            numRatingUserId.Value = Math.Min((decimal)rating.UserId, numRatingUserId.Maximum);
            numRatingMovieId.Value = Math.Min((decimal)rating.MovieID, numRatingMovieId.Maximum);
            numRatingScore.Value = Math.Min(Math.Max((decimal)rating.Score, numRatingScore.Minimum), numRatingScore.Maximum);
            txtRatingReview.Text = rating.Review;
        }

        private async Task RatingLoadByUser()
        {
            var userId = (int)numRatingUserId.Value;
            try
            {
                var ratings = await _httpClientServices.ExectueAsync<List<RatingModel>>(
                    $"{ApiBaseUrl}/Rating/userid?userid={userId}", null, EnumHttpMethod.Get);
                dgvRatings.DataSource = ratings;
                SetStatus($"Loaded {ratings?.Count ?? 0} rating(s) for user {userId}.");
            }
            catch (Exception ex)
            {
                SetStatus("Failed to load ratings: " + ex.Message, true);
            }
        }

        private async Task RatingAdd()
        {
            try
            {
                var request = new RatingCreateRequestModel
                {
                    UserId = (int)numRatingUserId.Value,
                    MovieID = (int)numRatingMovieId.Value,
                    Score = (int)numRatingScore.Value,
                    Review = txtRatingReview.Text.Trim()
                };
                var result = await _httpClientServices.ExectueAsync<RatingCreateResponseModel>(
                    $"{ApiBaseUrl}/Rating", request, EnumHttpMethod.Post);
                SetStatus(result?.Message ?? "Rating added.", result?.IsSuccess != true);
                await RatingLoadByUser();
            }
            catch (Exception ex)
            {
                SetStatus("Failed to add rating: " + ex.Message, true);
            }
        }

        private async Task RatingUpdate()
        {
            if (!int.TryParse(txtRatingId.Text, out var id))
            {
                SetStatus("Enter the Id of the rating to update.", true);
                return;
            }
            try
            {
                var request = new RatingUpdateRequestModel
                {
                    Id = id,
                    Score = (int)numRatingScore.Value,
                    Review = txtRatingReview.Text.Trim()
                };
                var result = await _httpClientServices.ExectueAsync<RatingUpdateResponseModel>(
                    $"{ApiBaseUrl}/Rating", request, EnumHttpMethod.Put);
                SetStatus(result?.Message ?? "Rating updated.", result?.IsSuccess != true);
                await RatingLoadByUser();
            }
            catch (Exception ex)
            {
                SetStatus("Failed to update rating: " + ex.Message, true);
            }
        }

        private async Task RatingDelete()
        {
            if (!int.TryParse(txtRatingId.Text, out var id))
            {
                SetStatus("Enter the Id of the rating to delete.", true);
                return;
            }
            if (MessageBox.Show($"Delete rating Id {id}?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;
            try
            {
                var request = new RatingDeleteRequestModel { Id = id };
                var result = await _httpClientServices.ExectueAsync<RatingDeleteResponseModel>(
                    $"{ApiBaseUrl}/Rating", request, EnumHttpMethod.Delete);
                SetStatus(result?.Message ?? "Rating deleted.", result?.IsSuccess != true);
                await RatingLoadByUser();
            }
            catch (Exception ex)
            {
                SetStatus("Failed to delete rating: " + ex.Message, true);
            }
        }

        // ==========================================================
        // WATCH LIST
        // ==========================================================
        private void WireWatchListEvents()
        {
            btnWatchListLoadByUser.Click += async (s, e) => await WatchListLoadByUser();
            btnWatchListGetById.Click += async (s, e) => await WatchListGetById();
            btnWatchListAdd.Click += async (s, e) => await WatchListAdd();
            btnWatchListUpdate.Click += async (s, e) => await WatchListUpdate();
            btnWatchListDelete.Click += async (s, e) => await WatchListDelete();
            btnWatchListClear.Click += (s, e) => WatchListClearFields();
            dgvWatchList.SelectionChanged += (s, e) => WatchListPopulateFromGrid();

            if (cmbWatchListStatus.Items.Count > 0) cmbWatchListStatus.SelectedIndex = 0;
        }

        private void WatchListClearFields()
        {
            txtWatchListId.Clear();
            numWatchListUserId.Value = 0;
            numWatchListMovieId.Value = 0;
            numWatchListCurrentEpisode.Value = 0;
            numWatchListTotalEpisodes.Value = 0;
            if (cmbWatchListStatus.Items.Count > 0) cmbWatchListStatus.SelectedIndex = 0;
        }

        private void WatchListPopulateFromGrid()
        {
            if (dgvWatchList.CurrentRow?.DataBoundItem is not WatchListModel item) return;
            txtWatchListId.Text = item.Id.ToString();
            numWatchListUserId.Value = Math.Min((decimal)item.UserId, numWatchListUserId.Maximum);
            numWatchListMovieId.Value = Math.Min((decimal)item.MovieId, numWatchListMovieId.Maximum);
            numWatchListCurrentEpisode.Value = Math.Min((decimal)item.CurrentEpisode, numWatchListCurrentEpisode.Maximum);
            numWatchListTotalEpisodes.Value = Math.Min((decimal)item.TotalEpisodes, numWatchListTotalEpisodes.Maximum);
            var statusText = item.Status.ToString();
            if (cmbWatchListStatus.Items.Contains(statusText)) cmbWatchListStatus.SelectedItem = statusText;
        }

        private WatchStatus GetSelectedWatchStatus()
        {
            var text = cmbWatchListStatus.SelectedItem?.ToString() ?? "WantToWatch";
            return Enum.TryParse<WatchStatus>(text, out var status) ? status : WatchStatus.WantToWatch;
        }

        private async Task WatchListLoadByUser()
        {
            var userId = (int)numWatchListUserId.Value;
            try
            {
                // Note: the API returns a single (first) watch-list entry for this user, not a list.
                var item = await _httpClientServices.ExectueAsync<WatchListModel>(
                    $"{ApiBaseUrl}/WatchList/UserId?userId={userId}", null, EnumHttpMethod.Get);
                dgvWatchList.DataSource = item == null ? null : new List<WatchListModel> { item };
                SetStatus(item == null ? "No watch-list entry found for this user." : "Watch-list entry loaded.");
            }
            catch (Exception ex)
            {
                SetStatus("Failed to load watch list: " + ex.Message, true);
            }
        }

        private async Task WatchListGetById()
        {
            if (!int.TryParse(txtWatchListId.Text, out var id))
            {
                SetStatus("Enter a valid numeric watch-list Id first.", true);
                return;
            }
            try
            {
                var item = await _httpClientServices.ExectueAsync<WatchListModel>(
                    $"{ApiBaseUrl}/WatchList?id={id}", null, EnumHttpMethod.Get);
                dgvWatchList.DataSource = item == null ? null : new List<WatchListModel> { item };
                SetStatus(item == null ? "Watch-list entry not found." : "Watch-list entry loaded.");
            }
            catch (Exception ex)
            {
                SetStatus("Failed to get watch-list entry: " + ex.Message, true);
            }
        }

        private async Task WatchListAdd()
        {
            var userId = (int)numWatchListUserId.Value;
            try
            {
                var request = new WatchListCreateRequestModel
                {
                    MovieId = (int)numWatchListMovieId.Value,
                    Status = GetSelectedWatchStatus(),
                    CurrentEpisode = (int)numWatchListCurrentEpisode.Value,
                    TotalEpisodes = (int)numWatchListTotalEpisodes.Value
                };
                var result = await _httpClientServices.ExectueAsync<WatchListCreateResponseModel>(
                    $"{ApiBaseUrl}/WatchList/userid?userid={userId}", request, EnumHttpMethod.Post);
                SetStatus(result?.Message ?? "Watch-list entry added.", result?.IsSuccess != true);
                await WatchListLoadByUser();
            }
            catch (Exception ex)
            {
                SetStatus("Failed to add watch-list entry: " + ex.Message, true);
            }
        }

        private async Task WatchListUpdate()
        {
            if (!int.TryParse(txtWatchListId.Text, out var id))
            {
                SetStatus("Enter the Id of the watch-list entry to update.", true);
                return;
            }
            try
            {
                var request = new WatchListUpdateRequestModel
                {
                    Status = GetSelectedWatchStatus(),
                    CurrentEpisode = (int)numWatchListCurrentEpisode.Value,
                    TotalEpisodes = (int)numWatchListTotalEpisodes.Value
                };
                var result = await _httpClientServices.ExectueAsync<WatchListUpdateResponseModel>(
                    $"{ApiBaseUrl}/WatchList/id?id={id}", request, EnumHttpMethod.Put);
                SetStatus(result?.Message ?? "Watch-list entry updated.", result?.IsSuccess != true);
                await WatchListLoadByUser();
            }
            catch (Exception ex)
            {
                SetStatus("Failed to update watch-list entry: " + ex.Message, true);
            }
        }

        private async Task WatchListDelete()
        {
            if (!int.TryParse(txtWatchListId.Text, out var id))
            {
                SetStatus("Enter the Id of the watch-list entry to delete.", true);
                return;
            }
            if (MessageBox.Show($"Delete watch-list entry Id {id}?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;
            try
            {
                var result = await _httpClientServices.ExectueAsync<WatchListDeleteResponseModel>(
                    $"{ApiBaseUrl}/WatchList/id?id={id}", null, EnumHttpMethod.Delete);
                SetStatus(result?.Message ?? "Watch-list entry deleted.", result?.IsSuccess != true);
                await WatchListLoadByUser();
            }
            catch (Exception ex)
            {
                SetStatus("Failed to delete watch-list entry: " + ex.Message, true);
            }
        }
    }
}
