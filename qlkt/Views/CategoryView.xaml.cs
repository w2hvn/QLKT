using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using QLKT.Data;

namespace QLKT.Views
{
    public partial class CategoryView : UserControl
    {
        private readonly DatabaseContext _db;
        private int _selectedCategoryId = -1;

        public CategoryView()
        {
            InitializeComponent();
            _db = new DatabaseContext();
            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                string query = "SELECT * FROM RewardCategories";
                DataTable dt = await _db.ExecuteQueryAsync(query);
                dgCategories.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh mục: " + ex.Message);
            }
        }

        private void dgCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgCategories.SelectedItem is DataRowView row)
            {
                _selectedCategoryId = Convert.ToInt32(row["CategoryID"]);
                txtName.Text = row["CategoryName"].ToString();
                txtDescription.Text = row["Description"].ToString();

                txtFormTitle.Text = "Cập nhật danh mục";
                btnDelete.Visibility = Visibility.Visible;
            }
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên loại khen thưởng.");
                return;
            }

            try
            {
                string query;
                MySqlConnector.MySqlParameter[] parameters;

                if (_selectedCategoryId == -1)
                {
                    query = "INSERT INTO RewardCategories (CategoryName, Description) VALUES (@Name, @Desc)";
                    parameters = new MySqlConnector.MySqlParameter[]
                    {
                        new MySqlConnector.MySqlParameter("@Name", txtName.Text.Trim()),
                        new MySqlConnector.MySqlParameter("@Desc", txtDescription.Text.Trim())
                    };
                }
                else
                {
                    query = "UPDATE RewardCategories SET CategoryName = @Name, Description = @Desc WHERE CategoryID = @Id";
                    parameters = new MySqlConnector.MySqlParameter[]
                    {
                        new MySqlConnector.MySqlParameter("@Name", txtName.Text.Trim()),
                        new MySqlConnector.MySqlParameter("@Desc", txtDescription.Text.Trim()),
                        new MySqlConnector.MySqlParameter("@Id", _selectedCategoryId)
                    };
                }

                await _db.ExecuteNonQueryAsync(query, parameters);
                MessageBox.Show("Đã lưu thông tin thành công.");
                ResetForm();
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu: " + ex.Message);
            }
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCategoryId == -1) return;

            if (MessageBox.Show("Bạn có chắc chắn muốn xóa danh mục này?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    string query = "DELETE FROM RewardCategories WHERE CategoryID = @Id";
                    var parameters = new MySqlConnector.MySqlParameter[]
                    {
                        new MySqlConnector.MySqlParameter("@Id", _selectedCategoryId)
                    };

                    await _db.ExecuteNonQueryAsync(query, parameters);
                    MessageBox.Show("Đã xóa thành công.");
                    ResetForm();
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xóa (có thể đang có đề xuất sử dụng danh mục này): " + ex.Message);
                }
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            ResetForm();
        }

        private void ResetForm()
        {
            _selectedCategoryId = -1;
            txtName.Text = "";
            txtDescription.Text = "";
            txtFormTitle.Text = "Thêm mới danh mục";
            btnDelete.Visibility = Visibility.Collapsed;
            dgCategories.SelectedItem = null;
        }
    }
}
