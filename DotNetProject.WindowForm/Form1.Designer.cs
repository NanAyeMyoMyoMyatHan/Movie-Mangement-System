namespace DotNetProject.WindowForm
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabMovies = new System.Windows.Forms.TabPage();
            this.pnlMovieDetails = new System.Windows.Forms.Panel();
            this.lblMovieId = new System.Windows.Forms.Label();
            this.txtMovieId = new System.Windows.Forms.TextBox();
            this.lblMovieTitle = new System.Windows.Forms.Label();
            this.txtMovieTitle = new System.Windows.Forms.TextBox();
            this.lblMovieGenre = new System.Windows.Forms.Label();
            this.txtMovieGenre = new System.Windows.Forms.TextBox();
            this.lblMovieReleaseYear = new System.Windows.Forms.Label();
            this.numMovieReleaseYear = new System.Windows.Forms.NumericUpDown();
            this.lblMoviePosterUrl = new System.Windows.Forms.Label();
            this.txtMoviePosterUrl = new System.Windows.Forms.TextBox();
            this.lblMovieTmdbId = new System.Windows.Forms.Label();
            this.numMovieTmdbId = new System.Windows.Forms.NumericUpDown();
            this.chkMovieIsManual = new System.Windows.Forms.CheckBox();
            this.lblMovieDescription = new System.Windows.Forms.Label();
            this.txtMovieDescription = new System.Windows.Forms.TextBox();
            this.pnlMovieButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.btnMovieLoadAll = new System.Windows.Forms.Button();
            this.btnMovieGetById = new System.Windows.Forms.Button();
            this.btnMovieSearchGenre = new System.Windows.Forms.Button();
            this.btnMovieAdd = new System.Windows.Forms.Button();
            this.btnMovieUpdate = new System.Windows.Forms.Button();
            this.btnMovieDelete = new System.Windows.Forms.Button();
            this.btnMovieClear = new System.Windows.Forms.Button();
            this.flpMoviePosters = new System.Windows.Forms.FlowLayoutPanel();
            this.tabUsers = new System.Windows.Forms.TabPage();
            this.pnlUserDetails = new System.Windows.Forms.Panel();
            this.lblUserId = new System.Windows.Forms.Label();
            this.txtUserId = new System.Windows.Forms.TextBox();
            this.lblUserName = new System.Windows.Forms.Label();
            this.txtUserName = new System.Windows.Forms.TextBox();
            this.lblUserEmail = new System.Windows.Forms.Label();
            this.txtUserEmail = new System.Windows.Forms.TextBox();
            this.lblUserPassword = new System.Windows.Forms.Label();
            this.txtUserPassword = new System.Windows.Forms.TextBox();
            this.lblUserDisplayName = new System.Windows.Forms.Label();
            this.txtUserDisplayName = new System.Windows.Forms.TextBox();
            this.pnlUserButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.btnUserLoadAll = new System.Windows.Forms.Button();
            this.btnUserGetById = new System.Windows.Forms.Button();
            this.btnUserAdd = new System.Windows.Forms.Button();
            this.btnUserUpdate = new System.Windows.Forms.Button();
            this.btnUserClear = new System.Windows.Forms.Button();
            this.dgvUsers = new System.Windows.Forms.DataGridView();
            this.tabRatings = new System.Windows.Forms.TabPage();
            this.pnlRatingDetails = new System.Windows.Forms.Panel();
            this.lblRatingId = new System.Windows.Forms.Label();
            this.txtRatingId = new System.Windows.Forms.TextBox();
            this.lblRatingUserId = new System.Windows.Forms.Label();
            this.numRatingUserId = new System.Windows.Forms.NumericUpDown();
            this.lblRatingMovieId = new System.Windows.Forms.Label();
            this.numRatingMovieId = new System.Windows.Forms.NumericUpDown();
            this.lblRatingScore = new System.Windows.Forms.Label();
            this.numRatingScore = new System.Windows.Forms.NumericUpDown();
            this.lblRatingReview = new System.Windows.Forms.Label();
            this.txtRatingReview = new System.Windows.Forms.TextBox();
            this.pnlRatingButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.btnRatingLoadByUser = new System.Windows.Forms.Button();
            this.btnRatingAdd = new System.Windows.Forms.Button();
            this.btnRatingUpdate = new System.Windows.Forms.Button();
            this.btnRatingDelete = new System.Windows.Forms.Button();
            this.btnRatingClear = new System.Windows.Forms.Button();
            this.dgvRatings = new System.Windows.Forms.DataGridView();
            this.tabWatchList = new System.Windows.Forms.TabPage();
            this.pnlWatchListDetails = new System.Windows.Forms.Panel();
            this.lblWatchListId = new System.Windows.Forms.Label();
            this.txtWatchListId = new System.Windows.Forms.TextBox();
            this.lblWatchListUserId = new System.Windows.Forms.Label();
            this.numWatchListUserId = new System.Windows.Forms.NumericUpDown();
            this.lblWatchListMovieId = new System.Windows.Forms.Label();
            this.numWatchListMovieId = new System.Windows.Forms.NumericUpDown();
            this.lblWatchListStatus = new System.Windows.Forms.Label();
            this.cmbWatchListStatus = new System.Windows.Forms.ComboBox();
            this.lblWatchListCurrentEpisode = new System.Windows.Forms.Label();
            this.numWatchListCurrentEpisode = new System.Windows.Forms.NumericUpDown();
            this.lblWatchListTotalEpisodes = new System.Windows.Forms.Label();
            this.numWatchListTotalEpisodes = new System.Windows.Forms.NumericUpDown();
            this.pnlWatchListButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.btnWatchListLoadByUser = new System.Windows.Forms.Button();
            this.btnWatchListGetById = new System.Windows.Forms.Button();
            this.btnWatchListAdd = new System.Windows.Forms.Button();
            this.btnWatchListUpdate = new System.Windows.Forms.Button();
            this.btnWatchListDelete = new System.Windows.Forms.Button();
            this.btnWatchListClear = new System.Windows.Forms.Button();
            this.dgvWatchList = new System.Windows.Forms.DataGridView();
            this.pnlTop = new System.Windows.Forms.Panel();
            this.lblApiBaseUrl = new System.Windows.Forms.Label();
            this.txtApiBaseUrl = new System.Windows.Forms.TextBox();
            this.lblAppTitle = new System.Windows.Forms.Label();
            this.tabControlMain = new System.Windows.Forms.TabControl();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();

            this.pnlTop.SuspendLayout();
            this.tabControlMain.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.pnlMovieDetails.SuspendLayout();
            this.pnlMovieButtons.SuspendLayout();
            this.tabMovies.SuspendLayout();
            this.pnlUserDetails.SuspendLayout();
            this.pnlUserButtons.SuspendLayout();
            this.tabUsers.SuspendLayout();
            this.pnlRatingDetails.SuspendLayout();
            this.pnlRatingButtons.SuspendLayout();
            this.tabRatings.SuspendLayout();
            this.pnlWatchListDetails.SuspendLayout();
            this.pnlWatchListButtons.SuspendLayout();
            this.tabWatchList.SuspendLayout();

            this.SuspendLayout();

            // 
            // lblAppTitle
            // 
            this.lblAppTitle.AutoSize = true;
            this.lblAppTitle.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblAppTitle.ForeColor = System.Drawing.Color.FromArgb(30, 41, 59);
            this.lblAppTitle.Location = new System.Drawing.Point(14, 10);
            this.lblAppTitle.Name = "lblAppTitle";
            this.lblAppTitle.Size = new System.Drawing.Size(200, 24);
            this.lblAppTitle.Text = "🎬 MyFlix Movie Manager";

            // 
            // lblApiBaseUrl
            // 
            this.lblApiBaseUrl.AutoSize = true;
            this.lblApiBaseUrl.Name = "lblApiBaseUrl";
            this.lblApiBaseUrl.Text = "API URL:";
            this.lblApiBaseUrl.Visible = false;

            // 
            // txtApiBaseUrl
            // 
            this.txtApiBaseUrl.Name = "txtApiBaseUrl";
            this.txtApiBaseUrl.Text = "http://localhost:5134/api";
            this.txtApiBaseUrl.Visible = false;

            // 
            // pnlTop
            // 
            this.pnlTop.BackColor = System.Drawing.Color.FromArgb(255, 255, 255);
            this.pnlTop.Controls.Add(this.txtApiBaseUrl);
            this.pnlTop.Controls.Add(this.lblApiBaseUrl);
            this.pnlTop.Controls.Add(this.lblAppTitle);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Padding = new System.Windows.Forms.Padding(10);
            this.pnlTop.Size = new System.Drawing.Size(1100, 48);

            // 
            // tabControlMain
            // 
            this.tabControlMain.Controls.AddRange(new System.Windows.Forms.Control[] {
            this.tabMovies,
            this.tabUsers,
            this.tabRatings,
            this.tabWatchList});
            this.tabControlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlMain.Location = new System.Drawing.Point(0, 45);
            this.tabControlMain.Name = "tabControlMain";
            this.tabControlMain.SelectedIndex = 0;
            this.tabControlMain.Size = new System.Drawing.Size(1100, 639);
            this.tabControlMain.TabIndex = 1;

            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(39, 17);
            this.statusLabel.Text = "Ready";

            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 684);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1100, 22);
            this.statusStrip1.TabIndex = 2;

            this.lblMovieId.AutoSize = true;
            this.lblMovieId.Location = new System.Drawing.Point(15, 18);
            this.lblMovieId.Name = "lblMovieId";
            this.lblMovieId.Size = new System.Drawing.Size(110, 15);
            this.lblMovieId.Text = "Id:";

            this.txtMovieId.Location = new System.Drawing.Point(125, 15);
            this.txtMovieId.Name = "txtMovieId";
            this.txtMovieId.Size = new System.Drawing.Size(180, 23);

            this.lblMovieTitle.AutoSize = true;
            this.lblMovieTitle.Location = new System.Drawing.Point(345, 18);
            this.lblMovieTitle.Name = "lblMovieTitle";
            this.lblMovieTitle.Size = new System.Drawing.Size(110, 15);
            this.lblMovieTitle.Text = "Title:";

            this.txtMovieTitle.Location = new System.Drawing.Point(455, 15);
            this.txtMovieTitle.Name = "txtMovieTitle";
            this.txtMovieTitle.Size = new System.Drawing.Size(220, 23);

            this.lblMovieGenre.AutoSize = true;
            this.lblMovieGenre.Location = new System.Drawing.Point(15, 53);
            this.lblMovieGenre.Name = "lblMovieGenre";
            this.lblMovieGenre.Size = new System.Drawing.Size(110, 15);
            this.lblMovieGenre.Text = "Genre:";

            this.txtMovieGenre.Location = new System.Drawing.Point(125, 50);
            this.txtMovieGenre.Name = "txtMovieGenre";
            this.txtMovieGenre.Size = new System.Drawing.Size(180, 23);

            this.lblMovieReleaseYear.AutoSize = true;
            this.lblMovieReleaseYear.Location = new System.Drawing.Point(345, 53);
            this.lblMovieReleaseYear.Name = "lblMovieReleaseYear";
            this.lblMovieReleaseYear.Size = new System.Drawing.Size(110, 15);
            this.lblMovieReleaseYear.Text = "Release Year:";

            this.numMovieReleaseYear.Location = new System.Drawing.Point(455, 50);
            this.numMovieReleaseYear.Name = "numMovieReleaseYear";
            this.numMovieReleaseYear.Size = new System.Drawing.Size(220, 23);
            this.numMovieReleaseYear.Minimum = new decimal(new int[] { 1888, 0, 0, 0 });
            this.numMovieReleaseYear.Maximum = new decimal(new int[] { 2100, 0, 0, 0 });
            this.numMovieReleaseYear.Value = new decimal(new int[] { 2024, 0, 0, 0 });

            this.lblMoviePosterUrl.AutoSize = true;
            this.lblMoviePosterUrl.Location = new System.Drawing.Point(15, 88);
            this.lblMoviePosterUrl.Name = "lblMoviePosterUrl";
            this.lblMoviePosterUrl.Size = new System.Drawing.Size(110, 15);
            this.lblMoviePosterUrl.Text = "Poster URL:";

            this.txtMoviePosterUrl.Location = new System.Drawing.Point(125, 85);
            this.txtMoviePosterUrl.Name = "txtMoviePosterUrl";
            this.txtMoviePosterUrl.Size = new System.Drawing.Size(180, 23);

            this.lblMovieTmdbId.AutoSize = true;
            this.lblMovieTmdbId.Location = new System.Drawing.Point(345, 88);
            this.lblMovieTmdbId.Name = "lblMovieTmdbId";
            this.lblMovieTmdbId.Size = new System.Drawing.Size(110, 15);
            this.lblMovieTmdbId.Text = "TMDB Id:";

            this.numMovieTmdbId.Location = new System.Drawing.Point(455, 85);
            this.numMovieTmdbId.Name = "numMovieTmdbId";
            this.numMovieTmdbId.Size = new System.Drawing.Size(220, 23);
            this.numMovieTmdbId.Maximum = new decimal(new int[] { 9999999, 0, 0, 0 });

            this.chkMovieIsManual.Location = new System.Drawing.Point(15, 122);
            this.chkMovieIsManual.Name = "chkMovieIsManual";
            this.chkMovieIsManual.AutoSize = true;
            this.chkMovieIsManual.Text = "Is Manual Entry";

            this.lblMovieDescription.AutoSize = true;
            this.lblMovieDescription.Location = new System.Drawing.Point(15, 153);
            this.lblMovieDescription.Name = "lblMovieDescription";
            this.lblMovieDescription.Size = new System.Drawing.Size(110, 15);
            this.lblMovieDescription.Text = "Description:";

            this.txtMovieDescription.Location = new System.Drawing.Point(125, 150);
            this.txtMovieDescription.Name = "txtMovieDescription";
            this.txtMovieDescription.Size = new System.Drawing.Size(550, 60);
            this.txtMovieDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right))));
            this.txtMovieDescription.Multiline = true;
            this.txtMovieDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;

            // 
            // pnlMovieDetails
            // 
            this.pnlMovieDetails.Controls.AddRange(new System.Windows.Forms.Control[] {
            this.lblMovieId,
            this.txtMovieId,
            this.lblMovieTitle,
            this.txtMovieTitle,
            this.lblMovieGenre,
            this.txtMovieGenre,
            this.lblMovieReleaseYear,
            this.numMovieReleaseYear,
            this.lblMoviePosterUrl,
            this.txtMoviePosterUrl,
            this.lblMovieTmdbId,
            this.numMovieTmdbId,
            this.chkMovieIsManual,
            this.lblMovieDescription,
            this.txtMovieDescription});
            this.pnlMovieDetails.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlMovieDetails.Location = new System.Drawing.Point(0, 0);
            this.pnlMovieDetails.Name = "pnlMovieDetails";
            this.pnlMovieDetails.Padding = new System.Windows.Forms.Padding(3);
            this.pnlMovieDetails.Size = new System.Drawing.Size(1092, 228);

            // 
            // btnMovieLoadAll
            // 
            this.btnMovieLoadAll.BackColor = System.Drawing.Color.FromArgb(14, 165, 233);
            this.btnMovieLoadAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMovieLoadAll.FlatAppearance.BorderSize = 0;
            this.btnMovieLoadAll.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnMovieLoadAll.ForeColor = System.Drawing.Color.White;
            this.btnMovieLoadAll.Name = "btnMovieLoadAll";
            this.btnMovieLoadAll.Size = new System.Drawing.Size(130, 34);
            this.btnMovieLoadAll.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.btnMovieLoadAll.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnMovieLoadAll.Text = "↻ Load All";

            // 
            // btnMovieGetById
            // 
            this.btnMovieGetById.BackColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.btnMovieGetById.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMovieGetById.FlatAppearance.BorderSize = 0;
            this.btnMovieGetById.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnMovieGetById.ForeColor = System.Drawing.Color.White;
            this.btnMovieGetById.Name = "btnMovieGetById";
            this.btnMovieGetById.Size = new System.Drawing.Size(130, 34);
            this.btnMovieGetById.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.btnMovieGetById.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnMovieGetById.Text = "🔍 Get By Id";

            // 
            // btnMovieSearchGenre
            // 
            this.btnMovieSearchGenre.BackColor = System.Drawing.Color.FromArgb(139, 92, 246);
            this.btnMovieSearchGenre.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMovieSearchGenre.FlatAppearance.BorderSize = 0;
            this.btnMovieSearchGenre.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnMovieSearchGenre.ForeColor = System.Drawing.Color.White;
            this.btnMovieSearchGenre.Name = "btnMovieSearchGenre";
            this.btnMovieSearchGenre.Size = new System.Drawing.Size(140, 34);
            this.btnMovieSearchGenre.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.btnMovieSearchGenre.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnMovieSearchGenre.Text = "🎭 Search Genre";

            // 
            // btnMovieAdd
            // 
            this.btnMovieAdd.BackColor = System.Drawing.Color.FromArgb(16, 185, 129);
            this.btnMovieAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMovieAdd.FlatAppearance.BorderSize = 0;
            this.btnMovieAdd.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnMovieAdd.ForeColor = System.Drawing.Color.White;
            this.btnMovieAdd.Name = "btnMovieAdd";
            this.btnMovieAdd.Size = new System.Drawing.Size(110, 34);
            this.btnMovieAdd.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.btnMovieAdd.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnMovieAdd.Text = "+ Add";

            // 
            // btnMovieUpdate
            // 
            this.btnMovieUpdate.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            this.btnMovieUpdate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMovieUpdate.FlatAppearance.BorderSize = 0;
            this.btnMovieUpdate.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnMovieUpdate.ForeColor = System.Drawing.Color.White;
            this.btnMovieUpdate.Name = "btnMovieUpdate";
            this.btnMovieUpdate.Size = new System.Drawing.Size(110, 34);
            this.btnMovieUpdate.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.btnMovieUpdate.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnMovieUpdate.Text = "✏ Update";

            // 
            // btnMovieDelete
            // 
            this.btnMovieDelete.BackColor = System.Drawing.Color.FromArgb(239, 68, 68);
            this.btnMovieDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMovieDelete.FlatAppearance.BorderSize = 0;
            this.btnMovieDelete.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnMovieDelete.ForeColor = System.Drawing.Color.White;
            this.btnMovieDelete.Name = "btnMovieDelete";
            this.btnMovieDelete.Size = new System.Drawing.Size(110, 34);
            this.btnMovieDelete.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.btnMovieDelete.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnMovieDelete.Text = "🗑 Delete";

            // 
            // btnMovieClear
            // 
            this.btnMovieClear.BackColor = System.Drawing.Color.FromArgb(245, 158, 11);
            this.btnMovieClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMovieClear.FlatAppearance.BorderSize = 0;
            this.btnMovieClear.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnMovieClear.ForeColor = System.Drawing.Color.White;
            this.btnMovieClear.Name = "btnMovieClear";
            this.btnMovieClear.Size = new System.Drawing.Size(100, 34);
            this.btnMovieClear.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.btnMovieClear.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnMovieClear.Text = "✕ Clear";

            // 
            // pnlMovieButtons
            // 
            this.pnlMovieButtons.Controls.AddRange(new System.Windows.Forms.Control[] {
            this.btnMovieLoadAll,
            this.btnMovieGetById,
            this.btnMovieSearchGenre,
            this.btnMovieAdd,
            this.btnMovieUpdate,
            this.btnMovieDelete,
            this.btnMovieClear});
            this.pnlMovieButtons.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlMovieButtons.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.pnlMovieButtons.Location = new System.Drawing.Point(0, 0);
            this.pnlMovieButtons.Name = "pnlMovieButtons";
            this.pnlMovieButtons.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
            this.pnlMovieButtons.Size = new System.Drawing.Size(1092, 50);
            this.pnlMovieButtons.WrapContents = true;

            // 
            // flpMoviePosters
            // 
            this.flpMoviePosters.AutoScroll = true;
            this.flpMoviePosters.BackColor = System.Drawing.Color.White;
            this.flpMoviePosters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpMoviePosters.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.flpMoviePosters.Location = new System.Drawing.Point(0, 0);
            this.flpMoviePosters.Name = "flpMoviePosters";
            this.flpMoviePosters.Padding = new System.Windows.Forms.Padding(10);
            this.flpMoviePosters.Size = new System.Drawing.Size(1092, 200);
            this.flpMoviePosters.TabIndex = 0;
            this.flpMoviePosters.WrapContents = true;

            // 
            // tabMovies
            // 
            this.tabMovies.Controls.Add(this.flpMoviePosters);
            this.tabMovies.Controls.Add(this.pnlMovieButtons);
            this.tabMovies.Controls.Add(this.pnlMovieDetails);
            this.tabMovies.Location = new System.Drawing.Point(4, 24);
            this.tabMovies.Name = "tabMovies";
            this.tabMovies.Padding = new System.Windows.Forms.Padding(3);
            this.tabMovies.Size = new System.Drawing.Size(1092, 611);
            this.tabMovies.TabIndex = 0;
            this.tabMovies.Text = "Movies";
            this.tabMovies.UseVisualStyleBackColor = true;

            this.lblUserId.AutoSize = true;
            this.lblUserId.Location = new System.Drawing.Point(15, 18);
            this.lblUserId.Name = "lblUserId";
            this.lblUserId.Size = new System.Drawing.Size(110, 15);
            this.lblUserId.Text = "Id:";

            this.txtUserId.Location = new System.Drawing.Point(125, 15);
            this.txtUserId.Name = "txtUserId";
            this.txtUserId.Size = new System.Drawing.Size(180, 23);

            this.lblUserName.AutoSize = true;
            this.lblUserName.Location = new System.Drawing.Point(345, 18);
            this.lblUserName.Name = "lblUserName";
            this.lblUserName.Size = new System.Drawing.Size(110, 15);
            this.lblUserName.Text = "User Name:";

            this.txtUserName.Location = new System.Drawing.Point(455, 15);
            this.txtUserName.Name = "txtUserName";
            this.txtUserName.Size = new System.Drawing.Size(220, 23);

            this.lblUserEmail.AutoSize = true;
            this.lblUserEmail.Location = new System.Drawing.Point(15, 53);
            this.lblUserEmail.Name = "lblUserEmail";
            this.lblUserEmail.Size = new System.Drawing.Size(110, 15);
            this.lblUserEmail.Text = "Email:";

            this.txtUserEmail.Location = new System.Drawing.Point(125, 50);
            this.txtUserEmail.Name = "txtUserEmail";
            this.txtUserEmail.Size = new System.Drawing.Size(180, 23);

            this.lblUserPassword.AutoSize = true;
            this.lblUserPassword.Location = new System.Drawing.Point(345, 53);
            this.lblUserPassword.Name = "lblUserPassword";
            this.lblUserPassword.Size = new System.Drawing.Size(110, 15);
            this.lblUserPassword.Text = "Password:";

            this.txtUserPassword.Location = new System.Drawing.Point(455, 50);
            this.txtUserPassword.Name = "txtUserPassword";
            this.txtUserPassword.Size = new System.Drawing.Size(220, 23);
            this.txtUserPassword.PasswordChar = '*';

            this.lblUserDisplayName.AutoSize = true;
            this.lblUserDisplayName.Location = new System.Drawing.Point(15, 88);
            this.lblUserDisplayName.Name = "lblUserDisplayName";
            this.lblUserDisplayName.Size = new System.Drawing.Size(110, 15);
            this.lblUserDisplayName.Text = "Display Name:";

            this.txtUserDisplayName.Location = new System.Drawing.Point(125, 85);
            this.txtUserDisplayName.Name = "txtUserDisplayName";
            this.txtUserDisplayName.Size = new System.Drawing.Size(180, 23);

            // 
            // pnlUserDetails
            // 
            this.pnlUserDetails.Controls.AddRange(new System.Windows.Forms.Control[] {
            this.lblUserId,
            this.txtUserId,
            this.lblUserName,
            this.txtUserName,
            this.lblUserEmail,
            this.txtUserEmail,
            this.lblUserPassword,
            this.txtUserPassword,
            this.lblUserDisplayName,
            this.txtUserDisplayName});
            this.pnlUserDetails.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlUserDetails.Location = new System.Drawing.Point(0, 0);
            this.pnlUserDetails.Name = "pnlUserDetails";
            this.pnlUserDetails.Padding = new System.Windows.Forms.Padding(3);
            this.pnlUserDetails.Size = new System.Drawing.Size(1092, 123);

            // 
            // btnUserLoadAll
            // 
            this.btnUserLoadAll.BackColor = System.Drawing.Color.FromArgb(14, 165, 233);
            this.btnUserLoadAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUserLoadAll.FlatAppearance.BorderSize = 0;
            this.btnUserLoadAll.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnUserLoadAll.ForeColor = System.Drawing.Color.White;
            this.btnUserLoadAll.Name = "btnUserLoadAll";
            this.btnUserLoadAll.Size = new System.Drawing.Size(130, 34);
            this.btnUserLoadAll.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.btnUserLoadAll.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnUserLoadAll.Text = "↻ Load All";

            // 
            // btnUserGetById
            // 
            this.btnUserGetById.BackColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.btnUserGetById.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUserGetById.FlatAppearance.BorderSize = 0;
            this.btnUserGetById.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnUserGetById.ForeColor = System.Drawing.Color.White;
            this.btnUserGetById.Name = "btnUserGetById";
            this.btnUserGetById.Size = new System.Drawing.Size(130, 34);
            this.btnUserGetById.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.btnUserGetById.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnUserGetById.Text = "🔍 Get By Id";

            // 
            // btnUserAdd
            // 
            this.btnUserAdd.BackColor = System.Drawing.Color.FromArgb(16, 185, 129);
            this.btnUserAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUserAdd.FlatAppearance.BorderSize = 0;
            this.btnUserAdd.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnUserAdd.ForeColor = System.Drawing.Color.White;
            this.btnUserAdd.Name = "btnUserAdd";
            this.btnUserAdd.Size = new System.Drawing.Size(110, 34);
            this.btnUserAdd.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.btnUserAdd.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnUserAdd.Text = "+ Add";

            // 
            // btnUserUpdate
            // 
            this.btnUserUpdate.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            this.btnUserUpdate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUserUpdate.FlatAppearance.BorderSize = 0;
            this.btnUserUpdate.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnUserUpdate.ForeColor = System.Drawing.Color.White;
            this.btnUserUpdate.Name = "btnUserUpdate";
            this.btnUserUpdate.Size = new System.Drawing.Size(110, 34);
            this.btnUserUpdate.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.btnUserUpdate.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnUserUpdate.Text = "✏ Update";

            // 
            // btnUserClear
            // 
            this.btnUserClear.BackColor = System.Drawing.Color.FromArgb(245, 158, 11);
            this.btnUserClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUserClear.FlatAppearance.BorderSize = 0;
            this.btnUserClear.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnUserClear.ForeColor = System.Drawing.Color.White;
            this.btnUserClear.Name = "btnUserClear";
            this.btnUserClear.Size = new System.Drawing.Size(100, 34);
            this.btnUserClear.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.btnUserClear.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnUserClear.Text = "✕ Clear";

            // 
            // pnlUserButtons
            // 
            this.pnlUserButtons.Controls.AddRange(new System.Windows.Forms.Control[] {
            this.btnUserLoadAll,
            this.btnUserGetById,
            this.btnUserAdd,
            this.btnUserUpdate,
            this.btnUserClear});
            this.pnlUserButtons.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlUserButtons.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.pnlUserButtons.Location = new System.Drawing.Point(0, 0);
            this.pnlUserButtons.Name = "pnlUserButtons";
            this.pnlUserButtons.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
            this.pnlUserButtons.Size = new System.Drawing.Size(1092, 50);
            this.pnlUserButtons.WrapContents = true;

            // 
            // dgvUsers
            // 
            this.dgvUsers.AllowUserToAddRows = false;
            this.dgvUsers.AllowUserToDeleteRows = false;
            this.dgvUsers.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvUsers.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgvUsers.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dgvUsers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvUsers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvUsers.Location = new System.Drawing.Point(0, 0);
            this.dgvUsers.MultiSelect = false;
            this.dgvUsers.Name = "dgvUsers";
            this.dgvUsers.ReadOnly = true;
            this.dgvUsers.RowHeadersVisible = false;
            this.dgvUsers.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvUsers.Size = new System.Drawing.Size(1092, 200);
            this.dgvUsers.TabIndex = 0;

            // 
            // tabUsers
            // 
            this.tabUsers.Controls.Add(this.dgvUsers);
            this.tabUsers.Controls.Add(this.pnlUserButtons);
            this.tabUsers.Controls.Add(this.pnlUserDetails);
            this.tabUsers.Location = new System.Drawing.Point(4, 24);
            this.tabUsers.Name = "tabUsers";
            this.tabUsers.Padding = new System.Windows.Forms.Padding(3);
            this.tabUsers.Size = new System.Drawing.Size(1092, 611);
            this.tabUsers.TabIndex = 0;
            this.tabUsers.Text = "Users";
            this.tabUsers.UseVisualStyleBackColor = true;

            this.lblRatingId.AutoSize = true;
            this.lblRatingId.Location = new System.Drawing.Point(15, 18);
            this.lblRatingId.Name = "lblRatingId";
            this.lblRatingId.Size = new System.Drawing.Size(110, 15);
            this.lblRatingId.Text = "Id:";

            this.txtRatingId.Location = new System.Drawing.Point(125, 15);
            this.txtRatingId.Name = "txtRatingId";
            this.txtRatingId.Size = new System.Drawing.Size(180, 23);

            this.lblRatingUserId.AutoSize = true;
            this.lblRatingUserId.Location = new System.Drawing.Point(345, 18);
            this.lblRatingUserId.Name = "lblRatingUserId";
            this.lblRatingUserId.Size = new System.Drawing.Size(110, 15);
            this.lblRatingUserId.Text = "User Id:";

            this.numRatingUserId.Location = new System.Drawing.Point(455, 15);
            this.numRatingUserId.Name = "numRatingUserId";
            this.numRatingUserId.Size = new System.Drawing.Size(220, 23);
            this.numRatingUserId.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });

            this.lblRatingMovieId.AutoSize = true;
            this.lblRatingMovieId.Location = new System.Drawing.Point(15, 53);
            this.lblRatingMovieId.Name = "lblRatingMovieId";
            this.lblRatingMovieId.Size = new System.Drawing.Size(110, 15);
            this.lblRatingMovieId.Text = "Movie Id:";

            this.numRatingMovieId.Location = new System.Drawing.Point(125, 50);
            this.numRatingMovieId.Name = "numRatingMovieId";
            this.numRatingMovieId.Size = new System.Drawing.Size(180, 23);
            this.numRatingMovieId.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });

            this.lblRatingScore.AutoSize = true;
            this.lblRatingScore.Location = new System.Drawing.Point(345, 53);
            this.lblRatingScore.Name = "lblRatingScore";
            this.lblRatingScore.Size = new System.Drawing.Size(110, 15);
            this.lblRatingScore.Text = "Score (1-10):";

            this.numRatingScore.Location = new System.Drawing.Point(455, 50);
            this.numRatingScore.Name = "numRatingScore";
            this.numRatingScore.Size = new System.Drawing.Size(220, 23);
            this.numRatingScore.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numRatingScore.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            this.numRatingScore.Value = new decimal(new int[] { 1, 0, 0, 0 });

            this.lblRatingReview.AutoSize = true;
            this.lblRatingReview.Location = new System.Drawing.Point(15, 88);
            this.lblRatingReview.Name = "lblRatingReview";
            this.lblRatingReview.Size = new System.Drawing.Size(110, 15);
            this.lblRatingReview.Text = "Review:";

            this.txtRatingReview.Location = new System.Drawing.Point(125, 85);
            this.txtRatingReview.Name = "txtRatingReview";
            this.txtRatingReview.Size = new System.Drawing.Size(550, 60);
            this.txtRatingReview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right))));
            this.txtRatingReview.Multiline = true;
            this.txtRatingReview.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;

            // 
            // pnlRatingDetails
            // 
            this.pnlRatingDetails.Controls.AddRange(new System.Windows.Forms.Control[] {
            this.lblRatingId,
            this.txtRatingId,
            this.lblRatingUserId,
            this.numRatingUserId,
            this.lblRatingMovieId,
            this.numRatingMovieId,
            this.lblRatingScore,
            this.numRatingScore,
            this.lblRatingReview,
            this.txtRatingReview});
            this.pnlRatingDetails.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlRatingDetails.Location = new System.Drawing.Point(0, 0);
            this.pnlRatingDetails.Name = "pnlRatingDetails";
            this.pnlRatingDetails.Padding = new System.Windows.Forms.Padding(3);
            this.pnlRatingDetails.Size = new System.Drawing.Size(1092, 163);

            // 
            // btnRatingLoadByUser
            // 
            this.btnRatingLoadByUser.BackColor = System.Drawing.Color.FromArgb(14, 165, 233);
            this.btnRatingLoadByUser.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRatingLoadByUser.FlatAppearance.BorderSize = 0;
            this.btnRatingLoadByUser.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnRatingLoadByUser.ForeColor = System.Drawing.Color.White;
            this.btnRatingLoadByUser.Name = "btnRatingLoadByUser";
            this.btnRatingLoadByUser.Size = new System.Drawing.Size(150, 34);
            this.btnRatingLoadByUser.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.btnRatingLoadByUser.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRatingLoadByUser.Text = "↻ Load By User";

            // 
            // btnRatingAdd
            // 
            this.btnRatingAdd.BackColor = System.Drawing.Color.FromArgb(16, 185, 129);
            this.btnRatingAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRatingAdd.FlatAppearance.BorderSize = 0;
            this.btnRatingAdd.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnRatingAdd.ForeColor = System.Drawing.Color.White;
            this.btnRatingAdd.Name = "btnRatingAdd";
            this.btnRatingAdd.Size = new System.Drawing.Size(110, 34);
            this.btnRatingAdd.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.btnRatingAdd.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRatingAdd.Text = "+ Add";

            // 
            // btnRatingUpdate
            // 
            this.btnRatingUpdate.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            this.btnRatingUpdate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRatingUpdate.FlatAppearance.BorderSize = 0;
            this.btnRatingUpdate.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnRatingUpdate.ForeColor = System.Drawing.Color.White;
            this.btnRatingUpdate.Name = "btnRatingUpdate";
            this.btnRatingUpdate.Size = new System.Drawing.Size(110, 34);
            this.btnRatingUpdate.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.btnRatingUpdate.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRatingUpdate.Text = "✏ Update";

            // 
            // btnRatingDelete
            // 
            this.btnRatingDelete.BackColor = System.Drawing.Color.FromArgb(239, 68, 68);
            this.btnRatingDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRatingDelete.FlatAppearance.BorderSize = 0;
            this.btnRatingDelete.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnRatingDelete.ForeColor = System.Drawing.Color.White;
            this.btnRatingDelete.Name = "btnRatingDelete";
            this.btnRatingDelete.Size = new System.Drawing.Size(110, 34);
            this.btnRatingDelete.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.btnRatingDelete.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRatingDelete.Text = "🗑 Delete";

            // 
            // btnRatingClear
            // 
            this.btnRatingClear.BackColor = System.Drawing.Color.FromArgb(245, 158, 11);
            this.btnRatingClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRatingClear.FlatAppearance.BorderSize = 0;
            this.btnRatingClear.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnRatingClear.ForeColor = System.Drawing.Color.White;
            this.btnRatingClear.Name = "btnRatingClear";
            this.btnRatingClear.Size = new System.Drawing.Size(100, 34);
            this.btnRatingClear.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.btnRatingClear.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRatingClear.Text = "✕ Clear";

            // 
            // pnlRatingButtons
            // 
            this.pnlRatingButtons.Controls.AddRange(new System.Windows.Forms.Control[] {
            this.btnRatingLoadByUser,
            this.btnRatingAdd,
            this.btnRatingUpdate,
            this.btnRatingDelete,
            this.btnRatingClear});
            this.pnlRatingButtons.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlRatingButtons.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.pnlRatingButtons.Location = new System.Drawing.Point(0, 0);
            this.pnlRatingButtons.Name = "pnlRatingButtons";
            this.pnlRatingButtons.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
            this.pnlRatingButtons.Size = new System.Drawing.Size(1092, 50);
            this.pnlRatingButtons.WrapContents = true;

            // 
            // dgvRatings
            // 
            this.dgvRatings.AllowUserToAddRows = false;
            this.dgvRatings.AllowUserToDeleteRows = false;
            this.dgvRatings.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvRatings.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgvRatings.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dgvRatings.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvRatings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvRatings.Location = new System.Drawing.Point(0, 0);
            this.dgvRatings.MultiSelect = false;
            this.dgvRatings.Name = "dgvRatings";
            this.dgvRatings.ReadOnly = true;
            this.dgvRatings.RowHeadersVisible = false;
            this.dgvRatings.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvRatings.Size = new System.Drawing.Size(1092, 200);
            this.dgvRatings.TabIndex = 0;

            // 
            // tabRatings
            // 
            this.tabRatings.Controls.Add(this.dgvRatings);
            this.tabRatings.Controls.Add(this.pnlRatingButtons);
            this.tabRatings.Controls.Add(this.pnlRatingDetails);
            this.tabRatings.Location = new System.Drawing.Point(4, 24);
            this.tabRatings.Name = "tabRatings";
            this.tabRatings.Padding = new System.Windows.Forms.Padding(3);
            this.tabRatings.Size = new System.Drawing.Size(1092, 611);
            this.tabRatings.TabIndex = 0;
            this.tabRatings.Text = "Ratings";
            this.tabRatings.UseVisualStyleBackColor = true;

            this.lblWatchListId.AutoSize = true;
            this.lblWatchListId.Location = new System.Drawing.Point(15, 18);
            this.lblWatchListId.Name = "lblWatchListId";
            this.lblWatchListId.Size = new System.Drawing.Size(110, 15);
            this.lblWatchListId.Text = "Id:";

            this.txtWatchListId.Location = new System.Drawing.Point(125, 15);
            this.txtWatchListId.Name = "txtWatchListId";
            this.txtWatchListId.Size = new System.Drawing.Size(180, 23);

            this.lblWatchListUserId.AutoSize = true;
            this.lblWatchListUserId.Location = new System.Drawing.Point(345, 18);
            this.lblWatchListUserId.Name = "lblWatchListUserId";
            this.lblWatchListUserId.Size = new System.Drawing.Size(110, 15);
            this.lblWatchListUserId.Text = "User Id:";

            this.numWatchListUserId.Location = new System.Drawing.Point(455, 15);
            this.numWatchListUserId.Name = "numWatchListUserId";
            this.numWatchListUserId.Size = new System.Drawing.Size(220, 23);
            this.numWatchListUserId.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });

            this.lblWatchListMovieId.AutoSize = true;
            this.lblWatchListMovieId.Location = new System.Drawing.Point(15, 53);
            this.lblWatchListMovieId.Name = "lblWatchListMovieId";
            this.lblWatchListMovieId.Size = new System.Drawing.Size(110, 15);
            this.lblWatchListMovieId.Text = "Movie Id:";

            this.numWatchListMovieId.Location = new System.Drawing.Point(125, 50);
            this.numWatchListMovieId.Name = "numWatchListMovieId";
            this.numWatchListMovieId.Size = new System.Drawing.Size(180, 23);
            this.numWatchListMovieId.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });

            this.lblWatchListStatus.AutoSize = true;
            this.lblWatchListStatus.Location = new System.Drawing.Point(345, 53);
            this.lblWatchListStatus.Name = "lblWatchListStatus";
            this.lblWatchListStatus.Size = new System.Drawing.Size(110, 15);
            this.lblWatchListStatus.Text = "Status:";

            this.cmbWatchListStatus.Location = new System.Drawing.Point(455, 50);
            this.cmbWatchListStatus.Name = "cmbWatchListStatus";
            this.cmbWatchListStatus.Size = new System.Drawing.Size(220, 23);
            this.cmbWatchListStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbWatchListStatus.Items.AddRange(new object[] { "WantToWatch", "Watching", "Completed", "Dropped" });

            this.lblWatchListCurrentEpisode.AutoSize = true;
            this.lblWatchListCurrentEpisode.Location = new System.Drawing.Point(15, 88);
            this.lblWatchListCurrentEpisode.Name = "lblWatchListCurrentEpisode";
            this.lblWatchListCurrentEpisode.Size = new System.Drawing.Size(110, 15);
            this.lblWatchListCurrentEpisode.Text = "Current Episode:";

            this.numWatchListCurrentEpisode.Location = new System.Drawing.Point(125, 85);
            this.numWatchListCurrentEpisode.Name = "numWatchListCurrentEpisode";
            this.numWatchListCurrentEpisode.Size = new System.Drawing.Size(180, 23);
            this.numWatchListCurrentEpisode.Maximum = new decimal(new int[] { 99999, 0, 0, 0 });

            this.lblWatchListTotalEpisodes.AutoSize = true;
            this.lblWatchListTotalEpisodes.Location = new System.Drawing.Point(345, 88);
            this.lblWatchListTotalEpisodes.Name = "lblWatchListTotalEpisodes";
            this.lblWatchListTotalEpisodes.Size = new System.Drawing.Size(110, 15);
            this.lblWatchListTotalEpisodes.Text = "Total Episodes:";

            this.numWatchListTotalEpisodes.Location = new System.Drawing.Point(455, 85);
            this.numWatchListTotalEpisodes.Name = "numWatchListTotalEpisodes";
            this.numWatchListTotalEpisodes.Size = new System.Drawing.Size(220, 23);
            this.numWatchListTotalEpisodes.Maximum = new decimal(new int[] { 99999, 0, 0, 0 });

            // 
            // pnlWatchListDetails
            // 
            this.pnlWatchListDetails.Controls.AddRange(new System.Windows.Forms.Control[] {
            this.lblWatchListId,
            this.txtWatchListId,
            this.lblWatchListUserId,
            this.numWatchListUserId,
            this.lblWatchListMovieId,
            this.numWatchListMovieId,
            this.lblWatchListStatus,
            this.cmbWatchListStatus,
            this.lblWatchListCurrentEpisode,
            this.numWatchListCurrentEpisode,
            this.lblWatchListTotalEpisodes,
            this.numWatchListTotalEpisodes});
            this.pnlWatchListDetails.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlWatchListDetails.Location = new System.Drawing.Point(0, 0);
            this.pnlWatchListDetails.Name = "pnlWatchListDetails";
            this.pnlWatchListDetails.Padding = new System.Windows.Forms.Padding(3);
            this.pnlWatchListDetails.Size = new System.Drawing.Size(1092, 123);

            // 
            // btnWatchListLoadByUser
            // 
            this.btnWatchListLoadByUser.BackColor = System.Drawing.Color.FromArgb(14, 165, 233);
            this.btnWatchListLoadByUser.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnWatchListLoadByUser.FlatAppearance.BorderSize = 0;
            this.btnWatchListLoadByUser.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnWatchListLoadByUser.ForeColor = System.Drawing.Color.White;
            this.btnWatchListLoadByUser.Name = "btnWatchListLoadByUser";
            this.btnWatchListLoadByUser.Size = new System.Drawing.Size(150, 34);
            this.btnWatchListLoadByUser.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.btnWatchListLoadByUser.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnWatchListLoadByUser.Text = "↻ Load By User";

            // 
            // btnWatchListGetById
            // 
            this.btnWatchListGetById.BackColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.btnWatchListGetById.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnWatchListGetById.FlatAppearance.BorderSize = 0;
            this.btnWatchListGetById.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnWatchListGetById.ForeColor = System.Drawing.Color.White;
            this.btnWatchListGetById.Name = "btnWatchListGetById";
            this.btnWatchListGetById.Size = new System.Drawing.Size(130, 34);
            this.btnWatchListGetById.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.btnWatchListGetById.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnWatchListGetById.Text = "🔍 Get By Id";

            // 
            // btnWatchListAdd
            // 
            this.btnWatchListAdd.BackColor = System.Drawing.Color.FromArgb(16, 185, 129);
            this.btnWatchListAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnWatchListAdd.FlatAppearance.BorderSize = 0;
            this.btnWatchListAdd.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnWatchListAdd.ForeColor = System.Drawing.Color.White;
            this.btnWatchListAdd.Name = "btnWatchListAdd";
            this.btnWatchListAdd.Size = new System.Drawing.Size(110, 34);
            this.btnWatchListAdd.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.btnWatchListAdd.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnWatchListAdd.Text = "+ Add";

            // 
            // btnWatchListUpdate
            // 
            this.btnWatchListUpdate.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            this.btnWatchListUpdate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnWatchListUpdate.FlatAppearance.BorderSize = 0;
            this.btnWatchListUpdate.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnWatchListUpdate.ForeColor = System.Drawing.Color.White;
            this.btnWatchListUpdate.Name = "btnWatchListUpdate";
            this.btnWatchListUpdate.Size = new System.Drawing.Size(110, 34);
            this.btnWatchListUpdate.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.btnWatchListUpdate.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnWatchListUpdate.Text = "✏ Update";

            // 
            // btnWatchListDelete
            // 
            this.btnWatchListDelete.BackColor = System.Drawing.Color.FromArgb(239, 68, 68);
            this.btnWatchListDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnWatchListDelete.FlatAppearance.BorderSize = 0;
            this.btnWatchListDelete.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnWatchListDelete.ForeColor = System.Drawing.Color.White;
            this.btnWatchListDelete.Name = "btnWatchListDelete";
            this.btnWatchListDelete.Size = new System.Drawing.Size(110, 34);
            this.btnWatchListDelete.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.btnWatchListDelete.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnWatchListDelete.Text = "🗑 Delete";

            // 
            // btnWatchListClear
            // 
            this.btnWatchListClear.BackColor = System.Drawing.Color.FromArgb(245, 158, 11);
            this.btnWatchListClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnWatchListClear.FlatAppearance.BorderSize = 0;
            this.btnWatchListClear.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnWatchListClear.ForeColor = System.Drawing.Color.White;
            this.btnWatchListClear.Name = "btnWatchListClear";
            this.btnWatchListClear.Size = new System.Drawing.Size(100, 34);
            this.btnWatchListClear.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.btnWatchListClear.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnWatchListClear.Text = "✕ Clear";

            // 
            // pnlWatchListButtons
            // 
            this.pnlWatchListButtons.Controls.AddRange(new System.Windows.Forms.Control[] {
            this.btnWatchListLoadByUser,
            this.btnWatchListGetById,
            this.btnWatchListAdd,
            this.btnWatchListUpdate,
            this.btnWatchListDelete,
            this.btnWatchListClear});
            this.pnlWatchListButtons.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlWatchListButtons.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.pnlWatchListButtons.Location = new System.Drawing.Point(0, 0);
            this.pnlWatchListButtons.Name = "pnlWatchListButtons";
            this.pnlWatchListButtons.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
            this.pnlWatchListButtons.Size = new System.Drawing.Size(1092, 50);
            this.pnlWatchListButtons.WrapContents = true;

            // 
            // dgvWatchList
            // 
            this.dgvWatchList.AllowUserToAddRows = false;
            this.dgvWatchList.AllowUserToDeleteRows = false;
            this.dgvWatchList.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvWatchList.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgvWatchList.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dgvWatchList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvWatchList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvWatchList.Location = new System.Drawing.Point(0, 0);
            this.dgvWatchList.MultiSelect = false;
            this.dgvWatchList.Name = "dgvWatchList";
            this.dgvWatchList.ReadOnly = true;
            this.dgvWatchList.RowHeadersVisible = false;
            this.dgvWatchList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvWatchList.Size = new System.Drawing.Size(1092, 200);
            this.dgvWatchList.TabIndex = 0;

            // 
            // tabWatchList
            // 
            this.tabWatchList.Controls.Add(this.dgvWatchList);
            this.tabWatchList.Controls.Add(this.pnlWatchListButtons);
            this.tabWatchList.Controls.Add(this.pnlWatchListDetails);
            this.tabWatchList.Location = new System.Drawing.Point(4, 24);
            this.tabWatchList.Name = "tabWatchList";
            this.tabWatchList.Padding = new System.Windows.Forms.Padding(3);
            this.tabWatchList.Size = new System.Drawing.Size(1092, 611);
            this.tabWatchList.TabIndex = 0;
            this.tabWatchList.Text = "Watch List";
            this.tabWatchList.UseVisualStyleBackColor = true;

            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1100, 706);
            this.Controls.Add(this.tabControlMain);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.pnlTop);
            this.MinimumSize = new System.Drawing.Size(900, 600);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Movie Manager";


            this.pnlTop.ResumeLayout(false);
            this.tabControlMain.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.pnlMovieDetails.ResumeLayout(false);
            this.pnlMovieButtons.ResumeLayout(false);
            this.tabMovies.ResumeLayout(false);
            this.pnlUserDetails.ResumeLayout(false);
            this.pnlUserButtons.ResumeLayout(false);
            this.tabUsers.ResumeLayout(false);
            this.pnlRatingDetails.ResumeLayout(false);
            this.pnlRatingButtons.ResumeLayout(false);
            this.tabRatings.ResumeLayout(false);
            this.pnlWatchListDetails.ResumeLayout(false);
            this.pnlWatchListButtons.ResumeLayout(false);
            this.tabWatchList.ResumeLayout(false);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TabPage tabMovies;
        private System.Windows.Forms.Panel pnlMovieDetails;
        private System.Windows.Forms.Label lblMovieId;
        private System.Windows.Forms.TextBox txtMovieId;
        private System.Windows.Forms.Label lblMovieTitle;
        private System.Windows.Forms.TextBox txtMovieTitle;
        private System.Windows.Forms.Label lblMovieGenre;
        private System.Windows.Forms.TextBox txtMovieGenre;
        private System.Windows.Forms.Label lblMovieReleaseYear;
        private System.Windows.Forms.NumericUpDown numMovieReleaseYear;
        private System.Windows.Forms.Label lblMoviePosterUrl;
        private System.Windows.Forms.TextBox txtMoviePosterUrl;
        private System.Windows.Forms.Label lblMovieTmdbId;
        private System.Windows.Forms.NumericUpDown numMovieTmdbId;
        private System.Windows.Forms.CheckBox chkMovieIsManual;
        private System.Windows.Forms.Label lblMovieDescription;
        private System.Windows.Forms.TextBox txtMovieDescription;
        private System.Windows.Forms.FlowLayoutPanel pnlMovieButtons;
        private System.Windows.Forms.Button btnMovieLoadAll;
        private System.Windows.Forms.Button btnMovieGetById;
        private System.Windows.Forms.Button btnMovieSearchGenre;
        private System.Windows.Forms.Button btnMovieAdd;
        private System.Windows.Forms.Button btnMovieUpdate;
        private System.Windows.Forms.Button btnMovieDelete;
        private System.Windows.Forms.Button btnMovieClear;
        private System.Windows.Forms.FlowLayoutPanel flpMoviePosters;
        private System.Windows.Forms.TabPage tabUsers;
        private System.Windows.Forms.Panel pnlUserDetails;
        private System.Windows.Forms.Label lblUserId;
        private System.Windows.Forms.TextBox txtUserId;
        private System.Windows.Forms.Label lblUserName;
        private System.Windows.Forms.TextBox txtUserName;
        private System.Windows.Forms.Label lblUserEmail;
        private System.Windows.Forms.TextBox txtUserEmail;
        private System.Windows.Forms.Label lblUserPassword;
        private System.Windows.Forms.TextBox txtUserPassword;
        private System.Windows.Forms.Label lblUserDisplayName;
        private System.Windows.Forms.TextBox txtUserDisplayName;
        private System.Windows.Forms.FlowLayoutPanel pnlUserButtons;
        private System.Windows.Forms.Button btnUserLoadAll;
        private System.Windows.Forms.Button btnUserGetById;
        private System.Windows.Forms.Button btnUserAdd;
        private System.Windows.Forms.Button btnUserUpdate;
        private System.Windows.Forms.Button btnUserClear;
        private System.Windows.Forms.DataGridView dgvUsers;
        private System.Windows.Forms.TabPage tabRatings;
        private System.Windows.Forms.Panel pnlRatingDetails;
        private System.Windows.Forms.Label lblRatingId;
        private System.Windows.Forms.TextBox txtRatingId;
        private System.Windows.Forms.Label lblRatingUserId;
        private System.Windows.Forms.NumericUpDown numRatingUserId;
        private System.Windows.Forms.Label lblRatingMovieId;
        private System.Windows.Forms.NumericUpDown numRatingMovieId;
        private System.Windows.Forms.Label lblRatingScore;
        private System.Windows.Forms.NumericUpDown numRatingScore;
        private System.Windows.Forms.Label lblRatingReview;
        private System.Windows.Forms.TextBox txtRatingReview;
        private System.Windows.Forms.FlowLayoutPanel pnlRatingButtons;
        private System.Windows.Forms.Button btnRatingLoadByUser;
        private System.Windows.Forms.Button btnRatingAdd;
        private System.Windows.Forms.Button btnRatingUpdate;
        private System.Windows.Forms.Button btnRatingDelete;
        private System.Windows.Forms.Button btnRatingClear;
        private System.Windows.Forms.DataGridView dgvRatings;
        private System.Windows.Forms.TabPage tabWatchList;
        private System.Windows.Forms.Panel pnlWatchListDetails;
        private System.Windows.Forms.Label lblWatchListId;
        private System.Windows.Forms.TextBox txtWatchListId;
        private System.Windows.Forms.Label lblWatchListUserId;
        private System.Windows.Forms.NumericUpDown numWatchListUserId;
        private System.Windows.Forms.Label lblWatchListMovieId;
        private System.Windows.Forms.NumericUpDown numWatchListMovieId;
        private System.Windows.Forms.Label lblWatchListStatus;
        private System.Windows.Forms.ComboBox cmbWatchListStatus;
        private System.Windows.Forms.Label lblWatchListCurrentEpisode;
        private System.Windows.Forms.NumericUpDown numWatchListCurrentEpisode;
        private System.Windows.Forms.Label lblWatchListTotalEpisodes;
        private System.Windows.Forms.NumericUpDown numWatchListTotalEpisodes;
        private System.Windows.Forms.FlowLayoutPanel pnlWatchListButtons;
        private System.Windows.Forms.Button btnWatchListLoadByUser;
        private System.Windows.Forms.Button btnWatchListGetById;
        private System.Windows.Forms.Button btnWatchListAdd;
        private System.Windows.Forms.Button btnWatchListUpdate;
        private System.Windows.Forms.Button btnWatchListDelete;
        private System.Windows.Forms.Button btnWatchListClear;
        private System.Windows.Forms.DataGridView dgvWatchList;
        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.Label lblApiBaseUrl;
        private System.Windows.Forms.TextBox txtApiBaseUrl;
        private System.Windows.Forms.Label lblAppTitle;
        private System.Windows.Forms.TabControl tabControlMain;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
    }
}
