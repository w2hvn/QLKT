using System.Windows;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Controls;
using System.Windows.Media;
using QLKT.Data;

namespace QLKT.Views
{
    public partial class RewardProposalView : UserControl
    {
        public event EventHandler OnCreateNewProposalRequested;
        private readonly DatabaseContext _db;

        public RewardProposalView()
        {
            InitializeComponent();
            _db = new DatabaseContext();
            LoadUnits();
            LoadData();
        }

        private async void LoadUnits()
        {
            try
            {
                var dt = await _db.ExecuteQueryAsync("SELECT UnitID, UnitName FROM Units");

                // Add "All" option
                DataRow dr = dt.NewRow();
                dr["UnitID"] = -1;
                dr["UnitName"] = "Tất cả đơn vị";
                dt.Rows.InsertAt(dr, 0);

                cboUnitFilter.ItemsSource = dt.DefaultView;
                cboUnitFilter.SelectedIndex = 0;
            }
            catch { }
        }

        private void BtnCreateNew_Click(object sender, RoutedEventArgs e)
        {
            OnCreateNewProposalRequested?.Invoke(this, EventArgs.Empty);
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadData();
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadData();
        }

        private async void LoadData()
        {
            if (_db == null) return;
            try
            {
                // Update stats
                string totalQ = "SELECT COUNT(*) FROM Proposals";
                string pendingQ = "SELECT COUNT(*) FROM Proposals WHERE Status = 'Chờ phê duyệt'";
                string approvedQ = "SELECT COUNT(*) FROM Proposals WHERE Status = 'Đã phê duyệt'";
                string rejectedQ = "SELECT COUNT(*) FROM Proposals WHERE Status = 'Từ chối'";

                var dtTotal = await _db.ExecuteQueryAsync(totalQ);
                var dtPending = await _db.ExecuteQueryAsync(pendingQ);
                var dtApproved = await _db.ExecuteQueryAsync(approvedQ);
                var dtRejected = await _db.ExecuteQueryAsync(rejectedQ);

                txtTotalProposals.Text = dtTotal.Rows[0][0].ToString();
                txtPendingProposals.Text = dtPending.Rows[0][0].ToString();
                txtApprovedProposals.Text = dtApproved.Rows[0][0].ToString();
                txtRejectedProposals.Text = dtRejected.Rows[0][0].ToString();

                // Load DataGrid
                string query = @"
                    SELECT p.ProposalID, p.ProposalCode, s.FullName, u.UnitName, c.CategoryName, p.DateProposed, p.Status
                    FROM Proposals p
                    JOIN Soldiers s ON p.SoldierID = s.SoldierID
                    LEFT JOIN Units u ON s.UnitID = u.UnitID
                    LEFT JOIN RewardCategories c ON p.CategoryID = c.CategoryID
                    WHERE 1=1";

                var parameters = new List<MySqlConnector.MySqlParameter>();

                if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    query += " AND (s.FullName LIKE @Search OR p.ProposalCode LIKE @Search)";
                    parameters.Add(new MySqlConnector.MySqlParameter("@Search", $"%{txtSearch.Text.Trim()}%"));
                }

                if (cboUnitFilter.SelectedValue != null && (int)cboUnitFilter.SelectedValue != -1)
                {
                    query += " AND s.UnitID = @UnitID";
                    parameters.Add(new MySqlConnector.MySqlParameter("@UnitID", cboUnitFilter.SelectedValue));
                }

                if (cboStatusFilter.SelectedItem is ComboBoxItem statusItem && statusItem.Content.ToString() != "Tất cả trạng thái")
                {
                    query += " AND p.Status = @Status";
                    parameters.Add(new MySqlConnector.MySqlParameter("@Status", statusItem.Content.ToString()));
                }

                if (cboYearFilter.SelectedItem is ComboBoxItem yearItem && yearItem.Content.ToString() != "Tất cả thời gian")
                {
                    int year = DateTime.Now.Year;
                    if (yearItem.Content.ToString() == "Năm ngoái") year--;
                    query += " AND YEAR(p.DateProposed) = @Year";
                    parameters.Add(new MySqlConnector.MySqlParameter("@Year", year));
                }

                query += " ORDER BY p.DateProposed DESC";

                DataTable dt = await _db.ExecuteQueryAsync(query, parameters.ToArray());
                var proposals = new List<ProposalItem>();

                foreach (DataRow row in dt.Rows)
                {
                    string status = row["Status"].ToString();
                    int statusCode = 0; // Default pending
                    if (status == "Đã phê duyệt") statusCode = 1;
                    else if (status == "Từ chối" || status == "Đã từ chối") statusCode = 2;
                    else if (status == "Yêu cầu bổ sung") statusCode = 3;

                    proposals.Add(new ProposalItem
                    {
                        Id = row["ProposalCode"].ToString(),
                        FullName = row["FullName"].ToString(),
                        Unit = row["UnitName"]?.ToString() ?? "",
                        RewardType = row["CategoryName"]?.ToString() ?? "N/A",
                        Date = Convert.ToDateTime(row["DateProposed"]).ToString("dd/MM/yyyy"),
                        Status = status,
                        StatusCode = statusCode
                    });
                }

                ProposalsDataGrid.ItemsSource = proposals;
            }
            catch (Exception ex)
            {
                // Silence or log
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
    }

    public class ProposalItem
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Unit { get; set; }
        public string RewardType { get; set; }
        public string Date { get; set; }
        public string Status { get; set; }
        public int StatusCode { get; set; }

        public SolidColorBrush StatusBackground
        {
            get
            {
                if (StatusCode == 1) return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8F5E9")); // Green light
                if (StatusCode == 2) return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEBEE")); // Red light
                if (StatusCode == 3) return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3F2FD")); // Blue light (Need more info)
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF3E0")); // Orange light (Pending)
            }
        }

        public SolidColorBrush StatusForeground
        {
            get
            {
                if (StatusCode == 1) return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E7D32")); // Green
                if (StatusCode == 2) return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C62828")); // Red
                if (StatusCode == 3) return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1565C0")); // Blue
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E65100")); // Orange
            }
        }
    }
}
