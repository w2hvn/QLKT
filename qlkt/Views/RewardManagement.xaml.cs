using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using MilitaryRewardApp.Data;
using MySqlConnector;

namespace MilitaryRewardApp.Views
{
    public partial class RewardManagement : UserControl
    {
        private readonly DatabaseContext _db;
        private int? _selectedSoldierId = null;
        private int? _selectedRewardId = null;

        public RewardManagement()
        {
            InitializeComponent();
            _db = new DatabaseContext();
            LoadSoldiers();
        }

        private async void LoadSoldiers(string search = "")
        {
            try
            {
                string query = "SELECT SoldierID, FullName FROM Soldiers";
                if (!string.IsNullOrEmpty(search) && search != "Nhập tên tìm kiếm...")
                {
                    query += $" WHERE FullName LIKE '%{search}%'";
                }
                var dt = await _db.ExecuteQueryAsync(query);
                lstSoldiers.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void txtSearchSoldier_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadSoldiers(txtSearchSoldier.Text);
        }

        private void lstSoldiers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstSoldiers.SelectedItem is DataRowView row)
            {
                _selectedSoldierId = Convert.ToInt32(row["SoldierID"]);
                LoadRewards();
            }
        }

        private async void LoadRewards()
        {
            if (_selectedSoldierId == null) return;
            try
            {
                var dt = await _db.ExecuteQueryAsync($"SELECT * FROM Rewards WHERE SoldierID = {_selectedSoldierId}");
                dgRewards.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải khen thưởng: " + ex.Message); }
        }

        private void dgRewards_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgRewards.SelectedItem is DataRowView row)
            {
                _selectedRewardId = Convert.ToInt32(row["RewardID"]);
                txtDecision.Text = row["DecisionNumber"].ToString();
                cboType.Text = row["RewardType"].ToString();
                txtReason.Text = row["Reason"].ToString();
                if (row["DateSigned"] != DBNull.Value)
                {
                    dpDate.SelectedDate = Convert.ToDateTime(row["DateSigned"]);
                }
            }
            else
            {
                _selectedRewardId = null;
                txtDecision.Text = "";
                cboType.SelectedIndex = -1;
                txtReason.Text = "";
                dpDate.SelectedDate = null;
            }
        }

        private async void Add_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedSoldierId == null)
            {
                MessageBox.Show("Vui lòng chọn quân nhân trước.");
                return;
            }
            try
            {
                string type = (cboType.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "";
                string date = dpDate.SelectedDate?.ToString("yyyy-MM-dd");

                string query = @"INSERT INTO Rewards (SoldierID, DecisionNumber, RewardType, Reason, DateSigned) 
                                 VALUES (@Sid, @Dec, @Type, @Reason, @Date)";

                var param = new MySqlParameter[]
                {
                    new MySqlParameter("@Sid", _selectedSoldierId),
                    new MySqlParameter("@Dec", txtDecision.Text),
                    new MySqlParameter("@Type", type),
                    new MySqlParameter("@Reason", txtReason.Text),
                    new MySqlParameter("@Date", date)
                };

                await _db.ExecuteNonQueryAsync(query, param);
                LoadRewards();
                ClearForm();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi thêm: " + ex.Message); }
        }

        private async void Update_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedRewardId == null) return;
            try
            {
                string type = (cboType.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "";
                string date = dpDate.SelectedDate?.ToString("yyyy-MM-dd");

                string query = @"UPDATE Rewards SET DecisionNumber=@Dec, RewardType=@Type, Reason=@Reason, DateSigned=@Date 
                                 WHERE RewardID=@Rid";

                var param = new MySqlParameter[]
                {
                    new MySqlParameter("@Dec", txtDecision.Text),
                    new MySqlParameter("@Type", type),
                    new MySqlParameter("@Reason", txtReason.Text),
                    new MySqlParameter("@Date", date),
                    new MySqlParameter("@Rid", _selectedRewardId)
                };

                await _db.ExecuteNonQueryAsync(query, param);
                LoadRewards();
                ClearForm();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi sửa: " + ex.Message); }
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedRewardId == null) return;
            if (MessageBox.Show("Xóa quyết định này?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    await _db.ExecuteNonQueryAsync($"DELETE FROM Rewards WHERE RewardID={_selectedRewardId}");
                    LoadRewards();
                    ClearForm();
                }
                catch (Exception ex) { MessageBox.Show("Lỗi xóa: " + ex.Message); }
            }
        }

        private void ClearForm()
        {
            _selectedRewardId = null;
            txtDecision.Text = "";
            cboType.SelectedIndex = -1;
            txtReason.Text = "";
            dpDate.SelectedDate = null;
        }
    }
}
