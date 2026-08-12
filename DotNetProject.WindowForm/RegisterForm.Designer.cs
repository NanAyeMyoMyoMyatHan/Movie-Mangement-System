namespace DotNetProject.WindowForm
{
    partial class RegisterForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblRegUserName = new System.Windows.Forms.Label();
            this.txtRegUserName = new System.Windows.Forms.TextBox();
            this.lblRegDisplayName = new System.Windows.Forms.Label();
            this.txtRegDisplayName = new System.Windows.Forms.TextBox();
            this.lblRegEmail = new System.Windows.Forms.Label();
            this.txtRegEmail = new System.Windows.Forms.TextBox();
            this.lblRegPassword = new System.Windows.Forms.Label();
            this.txtRegPassword = new System.Windows.Forms.TextBox();
            this.lblRegConfirmPassword = new System.Windows.Forms.Label();
            this.txtRegConfirmPassword = new System.Windows.Forms.TextBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnRegister = new System.Windows.Forms.Button();
            this.lnkGoToLogin = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();

            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(30, 25);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(300, 32);
            this.lblTitle.Text = "Create Account";

            // 
            // lblRegUserName
            // 
            this.lblRegUserName.AutoSize = true;
            this.lblRegUserName.Location = new System.Drawing.Point(30, 75);
            this.lblRegUserName.Name = "lblRegUserName";
            this.lblRegUserName.Size = new System.Drawing.Size(150, 15);
            this.lblRegUserName.Text = "Username";

            // 
            // txtRegUserName
            // 
            this.txtRegUserName.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right))));
            this.txtRegUserName.Location = new System.Drawing.Point(30, 95);
            this.txtRegUserName.Name = "txtRegUserName";
            this.txtRegUserName.Size = new System.Drawing.Size(310, 25);
            this.txtRegUserName.Font = new System.Drawing.Font("Segoe UI", 10F);

            // 
            // lblRegDisplayName
            // 
            this.lblRegDisplayName.AutoSize = true;
            this.lblRegDisplayName.Location = new System.Drawing.Point(30, 135);
            this.lblRegDisplayName.Name = "lblRegDisplayName";
            this.lblRegDisplayName.Size = new System.Drawing.Size(150, 15);
            this.lblRegDisplayName.Text = "Display Name";

            // 
            // txtRegDisplayName
            // 
            this.txtRegDisplayName.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right))));
            this.txtRegDisplayName.Location = new System.Drawing.Point(30, 155);
            this.txtRegDisplayName.Name = "txtRegDisplayName";
            this.txtRegDisplayName.Size = new System.Drawing.Size(310, 25);
            this.txtRegDisplayName.Font = new System.Drawing.Font("Segoe UI", 10F);

            // 
            // lblRegEmail
            // 
            this.lblRegEmail.AutoSize = true;
            this.lblRegEmail.Location = new System.Drawing.Point(30, 195);
            this.lblRegEmail.Name = "lblRegEmail";
            this.lblRegEmail.Size = new System.Drawing.Size(150, 15);
            this.lblRegEmail.Text = "Email";

            // 
            // txtRegEmail
            // 
            this.txtRegEmail.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right))));
            this.txtRegEmail.Location = new System.Drawing.Point(30, 215);
            this.txtRegEmail.Name = "txtRegEmail";
            this.txtRegEmail.Size = new System.Drawing.Size(310, 25);
            this.txtRegEmail.Font = new System.Drawing.Font("Segoe UI", 10F);

            // 
            // lblRegPassword
            // 
            this.lblRegPassword.AutoSize = true;
            this.lblRegPassword.Location = new System.Drawing.Point(30, 255);
            this.lblRegPassword.Name = "lblRegPassword";
            this.lblRegPassword.Size = new System.Drawing.Size(150, 15);
            this.lblRegPassword.Text = "Password";

            // 
            // txtRegPassword
            // 
            this.txtRegPassword.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right))));
            this.txtRegPassword.Location = new System.Drawing.Point(30, 275);
            this.txtRegPassword.Name = "txtRegPassword";
            this.txtRegPassword.Size = new System.Drawing.Size(310, 25);
            this.txtRegPassword.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtRegPassword.PasswordChar = '*';

            // 
            // lblRegConfirmPassword
            // 
            this.lblRegConfirmPassword.AutoSize = true;
            this.lblRegConfirmPassword.Location = new System.Drawing.Point(30, 315);
            this.lblRegConfirmPassword.Name = "lblRegConfirmPassword";
            this.lblRegConfirmPassword.Size = new System.Drawing.Size(150, 15);
            this.lblRegConfirmPassword.Text = "Confirm Password";

            // 
            // txtRegConfirmPassword
            // 
            this.txtRegConfirmPassword.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right))));
            this.txtRegConfirmPassword.Location = new System.Drawing.Point(30, 335);
            this.txtRegConfirmPassword.Name = "txtRegConfirmPassword";
            this.txtRegConfirmPassword.Size = new System.Drawing.Size(310, 25);
            this.txtRegConfirmPassword.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtRegConfirmPassword.PasswordChar = '*';

            // 
            // lblStatus
            // 
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right))));
            this.lblStatus.ForeColor = System.Drawing.Color.Crimson;
            this.lblStatus.Location = new System.Drawing.Point(30, 375)  ;
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(310, 20);
            this.lblStatus.Text = "";

            // 
            // btnRegister
            // 
            this.btnRegister.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right))));
            this.btnRegister.BackColor = System.Drawing.Color.FromArgb(41, 128, 185);
            this.btnRegister.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRegister.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnRegister.ForeColor = System.Drawing.Color.White;
            this.btnRegister.Location = new System.Drawing.Point(30, 405);
            this.btnRegister.Name = "btnRegister";
            this.btnRegister.Size = new System.Drawing.Size(310, 38);
            this.btnRegister.Text = "Sign Up";
            this.btnRegister.UseVisualStyleBackColor = false;

            // 
            // lnkGoToLogin
            // 
            this.lnkGoToLogin.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right))));
            this.lnkGoToLogin.AutoSize = true;
            this.lnkGoToLogin.LinkColor = System.Drawing.Color.FromArgb(41, 128, 185);
            this.lnkGoToLogin.Location = new System.Drawing.Point(30, 455);
            this.lnkGoToLogin.Name = "lnkGoToLogin";
            this.lnkGoToLogin.Size = new System.Drawing.Size(120, 15);
            this.lnkGoToLogin.TabStop = true;
            this.lnkGoToLogin.Text = "Already have an account? Log in";

            // 
            // RegisterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.AddRange(new System.Windows.Forms.Control[] {
            this.lblTitle,
            this.lblRegUserName,
            this.txtRegUserName,
            this.lblRegDisplayName,
            this.txtRegDisplayName,
            this.lblRegEmail,
            this.txtRegEmail,
            this.lblRegPassword,
            this.txtRegPassword,
            this.lblRegConfirmPassword,
            this.txtRegConfirmPassword,
            this.lblStatus,
            this.btnRegister,
            this.lnkGoToLogin});
            this.ClientSize = new System.Drawing.Size(400, 560);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "RegisterForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sign Up - Movie Manager";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblRegUserName;
        private System.Windows.Forms.TextBox txtRegUserName;
        private System.Windows.Forms.Label lblRegDisplayName;
        private System.Windows.Forms.TextBox txtRegDisplayName;
        private System.Windows.Forms.Label lblRegEmail;
        private System.Windows.Forms.TextBox txtRegEmail;
        private System.Windows.Forms.Label lblRegPassword;
        private System.Windows.Forms.TextBox txtRegPassword;
        private System.Windows.Forms.Label lblRegConfirmPassword;
        private System.Windows.Forms.TextBox txtRegConfirmPassword;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnRegister;
        private System.Windows.Forms.LinkLabel lnkGoToLogin;
    }
}
