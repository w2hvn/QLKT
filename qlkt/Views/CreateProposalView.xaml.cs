using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using QLKT.Data;

namespace QLKT.Views
{
    public partial class CreateProposalView : UserControl
    {
        private readonly DatabaseContext _db;

        public CreateProposalView()
        {
            InitializeComponent();
            _db = new DatabaseContext();
            LoadCategories();
        }

        private int _selectedSoldierId = 0;

        public void SetSoldier(int soldierId)
        {
            _selectedSoldierId = soldierId;
            LoadSoldierDetails(soldierId);
        }

        private async void LoadSoldierDetails(int soldierId)
        {
            try
            {
                string query = @"
                    SELECT s.SoldierID, s.FullName, s.Rank, u.UnitName, s.SoldierCode
                    FROM Soldiers s
                    LEFT JOIN Units u ON s.UnitID = u.UnitID
                    WHERE s.SoldierID = @Id";

                var parameters = new MySqlConnector.MySqlParameter[]
                {
                    new MySqlConnector.MySqlParameter("@Id", soldierId)
                };

                DataTable dt = await _db.ExecuteQueryAsync(query, parameters);
                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    txtSearchSoldier.Text = row["FullName"].ToString();
                    txtRank.Text = row["Rank"].ToString();
                    txtUnit.Text = row["UnitName"].ToString();
                    lblSearchHint.Visibility = Visibility.Collapsed;
                }
            }
            catch { }
        }

        private async void LoadCategories()
        {
            try
            {
                string query = "SELECT CategoryID, CategoryName FROM RewardCategories";
                DataTable dt = await _db.ExecuteQueryAsync(query);
                cboRewardCategory.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh mục khen thưởng: " + ex.Message);
            }
        }

        private async void txtSearchSoldier_TextChanged(object sender, TextChangedEventArgs e)
        {
            string keyword = txtSearchSoldier.Text.Trim();

            if (string.IsNullOrEmpty(keyword))
            {
                lblSearchHint.Visibility = Visibility.Visible;
                txtRank.Text = "";
                txtUnit.Text = "";
                _selectedSoldierId = 0;
                return;
            }

            lblSearchHint.Visibility = Visibility.Collapsed;

            // Only search if length >= 3 to avoid too many queries
            if (keyword.Length >= 3)
            {
                try
                {
                    string query = @"
                        SELECT s.SoldierID, s.FullName, s.Rank, u.UnitName
                        FROM Soldiers s
                        LEFT JOIN Units u ON s.UnitID = u.UnitID
                        WHERE s.SoldierCode LIKE @Keyword OR s.FullName LIKE @Keyword LIMIT 1";

                    var parameters = new MySqlConnector.MySqlParameter[]
                    {
                        new MySqlConnector.MySqlParameter("@Keyword", $"%{keyword}%")
                    };

                    DataTable dt = await _db.ExecuteQueryAsync(query, parameters);
                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        txtRank.Text = row["Rank"].ToString();
                        txtUnit.Text = row["UnitName"].ToString();
                        _selectedSoldierId = Convert.ToInt32(row["SoldierID"]);
                    }
                    else
                    {
                        txtRank.Text = "";
                        txtUnit.Text = "";
                        _selectedSoldierId = 0;
                    }
                }
                catch { }
            }
        }

        private async void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedSoldierId == 0)
            {
                MessageBox.Show("Vui lòng chọn chiến sĩ hợp lệ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cboRewardCategory.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn hình thức khen thưởng.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtReason.Text))
            {
                MessageBox.Show("Vui lòng nhập lý do.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (dpDate.SelectedDate == null)
            {
                MessageBox.Show("Vui lòng chọn ngày đề xuất.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Note: Normally we'd fetch the UserID dynamically from `App.CurrentUserId`.
                // Using 1 (Admin) as fallback for this demo setup.
                string query = @"
                    INSERT INTO Proposals (ProposalCode, SoldierID, CategoryID, Reason, DateProposed, ProposedByUserID, Status)
                    VALUES (@Code, @Soldier, @Category, @Reason, @Date, @User, 'Chờ phê duyệt')";

                string code = "DX-" + DateTime.Now.ToString("yyyyMMddHHmmss");

                var parameters = new MySqlConnector.MySqlParameter[]
                {
                    new MySqlConnector.MySqlParameter("@Code", code),
                    new MySqlConnector.MySqlParameter("@Soldier", _selectedSoldierId),
                    new MySqlConnector.MySqlParameter("@Category", cboRewardCategory.SelectedValue),
                    new MySqlConnector.MySqlParameter("@Reason", txtReason.Text),
                    new MySqlConnector.MySqlParameter("@Date", dpDate.SelectedDate.Value),
                    new MySqlConnector.MySqlParameter("@User", 1) // Assuming UserID 1 for now
                };

                await _db.ExecuteNonQueryAsync(query, parameters);
                MessageBox.Show("Đã gửi đề xuất thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                // Reset form
                txtSearchSoldier.Text = "";
                txtReason.Text = "";
                cboRewardCategory.SelectedIndex = -1;
                dpDate.SelectedDate = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu đề xuất: " + ex.Message, "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
