using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using QLKT.Data;
using MySqlConnector;

namespace QLKT.Views
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
            LoadAllRewards();
        }

        private async void LoadAllRewards()
        {
            try
            {
                string query = @"SELECT r.*, s.FullName, s.Rank, u.UnitName
                                 FROM Rewards r
                                 JOIN Soldiers s ON r.SoldierID = s.SoldierID
                                 LEFT JOIN Units u ON s.UnitID = u.UnitID
                                 ORDER BY r.RewardID DESC";
                var dt = await _db.ExecuteQueryAsync(query);
                dgRewards.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private async void txtSearchSoldier_TextChanged(object sender, TextChangedEventArgs e)
        {
            string search = txtSearchSoldier.Text;
            if (string.IsNullOrWhiteSpace(search)) return;

            try
            {
                string query = "SELECT s.*, u.UnitName FROM Soldiers s LEFT JOIN Units u ON s.UnitID = u.UnitID WHERE s.FullName LIKE @search LIMIT 1";
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@search", $"%{search}%")
                };

                var dt = await _db.ExecuteQueryAsync(query, parameters);
                if (dt.Rows.Count > 0)
                {
                    var row = dt.Rows[0];
                    _selectedSoldierId = Convert.ToInt32(row["SoldierID"]);
                    txtRank.Text = row["Rank"].ToString();
                    txtUnit.Text = row["UnitName"].ToString();
                }
            }
            catch { }
        }

        private void dgRewards_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgRewards.SelectedItem is DataRowView row)
            {
                _selectedRewardId = Convert.ToInt32(row["RewardID"]);
            }
        }

        private async void Add_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedSoldierId == null)
            {
                MessageBox.Show("Vui lòng tìm và chọn quân nhân.");
                return;
            }
            try
            {
                string type = (cboType.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "";
                string date = dpDate.SelectedDate?.ToString("yyyy-MM-dd") ?? DateTime.Now.ToString("yyyy-MM-dd");

                string query = @"INSERT INTO Rewards (SoldierID, DecisionNumber, RewardType, Reason, DateSigned) 
                                 VALUES (@Sid, @Dec, @Type, @Reason, @Date)";

                var param = new MySqlParameter[]
                {
                    new MySqlParameter("@Sid", _selectedSoldierId),
                    new MySqlParameter("@Dec", "PROPOSAL-" + DateTime.Now.Ticks.ToString().Substring(10)),
                    new MySqlParameter("@Type", type),
                    new MySqlParameter("@Reason", txtReason.Text),
                    new MySqlParameter("@Date", date)
                };

                await _db.ExecuteNonQueryAsync(query, param);
                MessageBox.Show("Đã gửi đề xuất thành công!");
                LoadAllRewards();
                ClearForm();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void ClearForm()
        {
            _selectedSoldierId = null;
            txtSearchSoldier.Text = "";
            txtRank.Text = "";
            txtUnit.Text = "";
            txtReason.Text = "";
            dpDate.SelectedDate = null;
            cboType.SelectedIndex = -1;
        }
    }
}
