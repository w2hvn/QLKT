using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using QLKT.Data;

namespace QLKT.Views
{
    public partial class ApprovalProcessView : UserControl
    {
        private readonly DatabaseContext _db;

        public ApprovalProcessView()
        {
            InitializeComponent();
            _db = new DatabaseContext();
            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                string query = @"
                    SELECT p.ProposalID, p.ProposalCode, s.FullName, u.UnitName, p.DateProposed
                    FROM Proposals p
                    JOIN Soldiers s ON p.SoldierID = s.SoldierID
                    LEFT JOIN Units u ON s.UnitID = u.UnitID
                    WHERE p.Status = 'Chờ phê duyệt'";

                DataTable dt = await _db.ExecuteQueryAsync(query);
                var pending = new List<PendingItem>();

                foreach (DataRow row in dt.Rows)
                {
                    pending.Add(new PendingItem
                    {
                        ProposalID = Convert.ToInt32(row["ProposalID"]),
                        ID = row["ProposalCode"].ToString(),
                        Name = row["FullName"].ToString(),
                        Unit = row["UnitName"].ToString(),
                        Date = Convert.ToDateTime(row["DateProposed"]).ToString("dd/MM/yyyy")
                    });
                }

                dgPending.ItemsSource = pending;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách phê duyệt: " + ex.Message);
            }
        }

        private int _selectedProposalId = -1;

        private void dgPending_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgPending.SelectedItem is PendingItem item)
            {
                _selectedProposalId = item.ProposalID;
                txtEmptyState.Visibility = Visibility.Collapsed;
                pnlDetails.Visibility = Visibility.Visible;
                txtSubject.Text = $"ĐX: Khen thưởng cho {item.Name} ({item.Unit})";
            }
            else
            {
                _selectedProposalId = -1;
                txtEmptyState.Visibility = Visibility.Visible;
                pnlDetails.Visibility = Visibility.Hidden;
            }
        }

        private async void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProposalId == -1) return;

            try
            {
                string decision = (cboDecision.SelectedItem as ComboBoxItem)?.Content.ToString();
                string status = "Chờ phê duyệt";
                if (decision == "Phê duyệt") status = "Đã phê duyệt";
                else if (decision == "Từ chối") status = "Từ chối";
                else if (decision == "Yêu cầu bổ sung") status = "Yêu cầu bổ sung";

                string query = "UPDATE Proposals SET Status = @Status WHERE ProposalID = @Id";
                var parameters = new MySqlConnector.MySqlParameter[]
                {
                    new MySqlConnector.MySqlParameter("@Status", status),
                    new MySqlConnector.MySqlParameter("@Id", _selectedProposalId)
                };

                await _db.ExecuteNonQueryAsync(query, parameters);
                MessageBox.Show("Cập nhật trạng thái thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                LoadData();
                pnlDetails.Visibility = Visibility.Hidden;
                txtEmptyState.Visibility = Visibility.Visible;
                _selectedProposalId = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật: " + ex.Message);
            }
        }
    }

    public class PendingItem
    {
        public int ProposalID { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public string Date { get; set; }
    }
}
