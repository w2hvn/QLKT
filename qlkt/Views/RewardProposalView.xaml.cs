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

        private int _currentPage = 1;
        private int _pageSize = 10;
        private int _totalItems = 0;
        private int _totalPages = 1;

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
            _currentPage = 1;
            LoadData();
        }

        private async void LoadData()
        {
            if (_db == null) return;
            try
            {
                // Update stats (Global stats irrespective of current filter/page for KPI cards)
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

                // Load DataGrid with dynamic filters
                string whereClause = " WHERE 1=1";
                var parameters = new List<MySqlConnector.MySqlParameter>();

                if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    whereClause += " AND (s.FullName LIKE @Search OR p.ProposalCode LIKE @Search)";
                    parameters.Add(new MySqlConnector.MySqlParameter("@Search", $"%{txtSearch.Text.Trim()}%"));
                }

                if (cboUnitFilter.SelectedValue != null && (int)cboUnitFilter.SelectedValue != -1)
                {
                    whereClause += " AND s.UnitID = @UnitID";
                    parameters.Add(new MySqlConnector.MySqlParameter("@UnitID", cboUnitFilter.SelectedValue));
                }

                if (cboStatusFilter.SelectedItem is ComboBoxItem statusItem && statusItem.Content.ToString() != "Tất cả trạng thái")
                {
                    whereClause += " AND p.Status = @Status";
                    parameters.Add(new MySqlConnector.MySqlParameter("@Status", statusItem.Content.ToString()));
                }

                if (cboYearFilter.SelectedItem is ComboBoxItem yearItem && yearItem.Content.ToString() != "Tất cả thời gian")
                {
                    int year = DateTime.Now.Year;
                    if (yearItem.Content.ToString() == "Năm ngoái") year--;
                    whereClause += " AND YEAR(p.DateProposed) = @Year";
                    parameters.Add(new MySqlConnector.MySqlParameter("@Year", year));
                }

                // Get Filtered Count
                string countFilteredQ = "SELECT COUNT(*) FROM Proposals p JOIN Soldiers s ON p.SoldierID = s.SoldierID" + whereClause;
                DataTable dtCount = await _db.ExecuteQueryAsync(countFilteredQ, parameters.ToArray());
                _totalItems = Convert.ToInt32(dtCount.Rows[0][0]);
                _totalPages = (int)Math.Ceiling((double)_totalItems / _pageSize);
                if (_totalPages == 0) _totalPages = 1;

                // Load Current Page
                string query = @"
                    SELECT p.ProposalID, p.ProposalCode, s.FullName, u.UnitName, c.CategoryName, p.DateProposed, p.Status
                    FROM Proposals p
                    JOIN Soldiers s ON p.SoldierID = s.SoldierID
                    LEFT JOIN Units u ON s.UnitID = u.UnitID
                    LEFT JOIN RewardCategories c ON p.CategoryID = c.CategoryID" +
                    whereClause +
                    " ORDER BY p.DateProposed DESC LIMIT @PageSize OFFSET @Offset";

                parameters.Add(new MySqlConnector.MySqlParameter("@PageSize", _pageSize));
                parameters.Add(new MySqlConnector.MySqlParameter("@Offset", (_currentPage - 1) * _pageSize));

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
                UpdatePaginationUI();
            }
            catch (Exception ex)
            {
                // Silence or log
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private void UpdatePaginationUI()
        {
            int start = (_currentPage - 1) * _pageSize + 1;
            int end = Math.Min(_currentPage * _pageSize, _totalItems);
            txtPaginationSummary.Text = _totalItems > 0 ? $"Hiển thị {start}-{end} trên tổng số {_totalItems} kết quả" : "Hiển thị 0 kết quả";

            btnPrevPage.IsEnabled = _currentPage > 1;
            btnNextPage.IsEnabled = _currentPage < _totalPages;

            pnlPageNumbers.Children.Clear();

            int startPage = Math.Max(1, _currentPage - 2);
            int endPage = Math.Min(_totalPages, startPage + 4);
            if (endPage - startPage < 4) startPage = Math.Max(1, endPage - 4);

            for (int i = startPage; i <= endPage; i++)
            {
                Button btn = new Button
                {
                    Content = i.ToString(),
                    Width = 40,
                    Height = 30,
                    Margin = new Thickness(0, 0, 8, 0),
                    Tag = i,
                    Style = (Style)Application.Current.Resources["Button"]
                };

                if (i == _currentPage)
                {
                    btn.Tag = "Dark";
                    btn.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#001529"));
                    btn.Foreground = System.Windows.Media.Brushes.White;
                }
                else
                {
                    btn.Tag = "Secondary";
                    btn.Background = System.Windows.Media.Brushes.White;
                }

                btn.Click += (s, e) =>
                {
                    _currentPage = (int)((Button)s).Tag;
                    LoadData();
                };

                pnlPageNumbers.Children.Add(btn);
            }
        }

        private void BtnPrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                LoadData();
            }
        }

        private void BtnNextPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                LoadData();
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
