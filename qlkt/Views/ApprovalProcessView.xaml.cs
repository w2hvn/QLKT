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

        private void dgPending_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgPending.SelectedItem is PendingItem item)
            {
                txtEmptyState.Visibility = Visibility.Collapsed;
                pnlDetails.Visibility = Visibility.Visible;
                txtSubject.Text = $"ĐX: Khen thưởng cho {item.Name} ({item.Unit})";
            }
            else
            {
                txtEmptyState.Visibility = Visibility.Visible;
                pnlDetails.Visibility = Visibility.Hidden;
            }
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Cập nhật trạng thái thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    public class PendingItem
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public string Date { get; set; }
    }
}
