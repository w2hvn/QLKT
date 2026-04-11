using System;
using System.Data;
using System.Windows;
using QLKT.Data;

namespace QLKT.Views
{
    public partial class LoginWindow : Window
    {
        private readonly DatabaseContext _db;

        public LoginWindow()
        {
            InitializeComponent();
            _db = new DatabaseContext();

            LoadSavedCredentials();
        }

        private void LoadSavedCredentials()
        {
            // Simple mockup of saved credentials logic using basic Base64 encoding to avoid pure plaintext (In real app, use Windows Data Protection API - DPAPI)
            try
            {
                if (System.IO.File.Exists("user.dat"))
                {
                    string encoded = System.IO.File.ReadAllText("user.dat");
                    var bytes = Convert.FromBase64String(encoded);
                    string decoded = System.Text.Encoding.UTF8.GetString(bytes);
                    var parts = decoded.Split('|');
                    if (parts.Length == 2)
                    {
                        txtUsername.Text = parts[0];
                        txtPassword.Password = parts[1];
                        chkRememberMe.IsChecked = true;
                    }
                }
            }
            catch { }
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                lblError.Text = "Vui lòng nhập đầy đủ thông tin.";
                lblError.Visibility = Visibility.Visible;
                return;
            }

            try
            {
                // In production, use Hashing. Plaintext for this demo as requested.
                string query = "SELECT * FROM Users WHERE Username = @U AND Password = @P";
                var parameters = new MySqlConnector.MySqlParameter[]
                {
                    new MySqlConnector.MySqlParameter("@U", username),
                    new MySqlConnector.MySqlParameter("@P", password)
                };

                DataTable dt = await _db.ExecuteQueryAsync(query, parameters);

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    App.CurrentUserRole = row["Role"].ToString();
                    App.CurrentUserName = row["FullName"].ToString();

                    if (chkRememberMe.IsChecked == true)
                    {
                        string data = $"{username}|{password}";
                        string encoded = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(data));
                        System.IO.File.WriteAllText("user.dat", encoded);
                    }
                    else
                    {
                        if (System.IO.File.Exists("user.dat"))
                        {
                            System.IO.File.Delete("user.dat");
                        }
                    }

                    MainWindow main = new MainWindow();
                    main.Show();
                    this.Close();
                }
                else
                {
                    lblError.Text = "Sai tên đăng nhập hoặc mật khẩu.";
                    lblError.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                lblError.Text = "Lỗi kết nối: " + ex.Message;
                lblError.Visibility = Visibility.Visible;
            }
        }
    }
}
