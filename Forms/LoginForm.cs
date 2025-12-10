using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MotorPartsInventory.Database;
using MySql.Data.MySqlClient;

namespace MotorPartsInventory.Forms
{
    public partial class LoginForm : Form
    {
        private DBConnection db;

        public LoginForm()
        {
            InitializeComponent();

            db = new DBConnection();

            this.Load += LoginForm_Load;
            btnLogin.Click += BtnLogin_Click;
            cbShowPass.CheckedChanged += CbShowPass_CheckedChanged;

            txtPassword.UseSystemPasswordChar = true;
            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            EnsureAdminAccount();
        }

        private void EnsureAdminAccount()
        {
            try
            {
                var conn = db.GetConnection();

                string checkSql = "SELECT COUNT(*) FROM Users WHERE ROLE = 'Admin'";
                using (var cmd = new MySqlCommand(checkSql, conn))
                {
                    var result = cmd.ExecuteScalar();
                    int count = 0;
                    if (result != null && int.TryParse(result.ToString(), out count))
                    {
                        // ok
                    }

                    if (count == 0)
                    {
                        string insertSql = "INSERT INTO Users (Username, PASSWORD, Firstname, LastName, ROLE, STATUS) VALUES (@u, @p, @f, @l, @r, @s)";
                        using (var insert = new MySqlCommand(insertSql, conn))
                        {
                            insert.Parameters.AddWithValue("@u", "admin");
                            insert.Parameters.AddWithValue("@p", "admin123");
                            insert.Parameters.AddWithValue("@f", "Admin");
                            insert.Parameters.AddWithValue("@l", "User");
                            insert.Parameters.AddWithValue("@r", "Admin");
                            insert.Parameters.AddWithValue("@s", "Active");

                            insert.ExecuteNonQuery();
                        }

                        MessageBox.Show("Default admin account created. Username: 'admin' Password: 'admin123'", "Admin Created", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

                db.CloseConnection();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error ensuring admin account: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                try { db.CloseConnection(); } catch { }
            }
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;
            string role = comboBox1.SelectedItem?.ToString() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(role))
            {
                MessageBox.Show("Please enter username, password and select role.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var conn = db.GetConnection();

                string sql = "SELECT UserID, Username, Firstname, LastName, ROLE FROM Users WHERE Username = @u AND PASSWORD = @p AND ROLE = @r AND STATUS = 'Active' LIMIT 1";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@u", username);
                    cmd.Parameters.AddWithValue("@p", password);
                    cmd.Parameters.AddWithValue("@r", role);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string first = reader["Firstname"]?.ToString();
                            string last = reader["LastName"]?.ToString();
                            string displayName = (first + " " + last).Trim();
                            MessageBox.Show($"Welcome {displayName} ({role})", "Login Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Open dashboard for admin
                            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
                            {
                                var dash = new Dashboard();
                                dash.FormClosed += (s, args) => this.Close();
                                dash.Show();
                                this.Hide();
                            }
                            else
                            {
                                // For other roles, just hide login for now; you can route to other forms later
                                this.Hide();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Invalid username, password, role, or account is inactive.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }

                db.CloseConnection();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Login error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                try { db.CloseConnection(); } catch { }
            }
        }

        private void CbShowPass_CheckedChanged(object sender, EventArgs e)
        {
            // toggle system password char
            txtPassword.UseSystemPasswordChar = !cbShowPass.Checked;
        }
    }
}
