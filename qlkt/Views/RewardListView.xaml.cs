using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QLKT.Data;

namespace QLKT.Views
{
    public partial class RewardListView : UserControl
    {
        public ObservableCollection<RewardItem> Rewards { get; set; }
        private readonly DatabaseContext _db;
        public event EventHandler<int> OnSoldierSelected;

        private int _currentPage = 1;
        private int _pageSize = 10;
        private int _totalItems = 0;
        private int _totalPages = 1;

        public RewardListView()
        {
            InitializeComponent();
            _db = new DatabaseContext();
            Rewards = new ObservableCollection<RewardItem>();
            dgRewardList.ItemsSource = Rewards;
            LoadFilters();
            LoadData();
        }

        private async void LoadFilters()
        {
            try
            {
                // Units
                var dtUnits = await _db.ExecuteQueryAsync("SELECT UnitID, UnitName FROM Units");
                DataRow drUnit = dtUnits.NewRow();
                drUnit["UnitID"] = -1;
                drUnit["UnitName"] = "Tất cả đơn vị";
                dtUnits.Rows.InsertAt(drUnit, 0);
                cboUnitFilter.ItemsSource = dtUnits.DefaultView;
                cboUnitFilter.SelectedIndex = 0;

                // Categories
                var dtCats = await _db.ExecuteQueryAsync("SELECT CategoryID, CategoryName FROM RewardCategories");
                DataRow drCat = dtCats.NewRow();
                drCat["CategoryID"] = -1;
                drCat["CategoryName"] = "Tất cả danh hiệu";
                dtCats.Rows.InsertAt(drCat, 0);
                cboCategoryFilter.ItemsSource = dtCats.DefaultView;
                cboCategoryFilter.SelectedIndex = 0;
            }
            catch { }
        }

        private void Filter_Changed(object sender, EventArgs e)
        {
            if (txtSearchPlaceholder != null)
                txtSearchPlaceholder.Visibility = string.IsNullOrEmpty(txtSearch.Text) ? Visibility.Visible : Visibility.Collapsed;

            _currentPage = 1;
            LoadData();
        }

        private async void LoadData()
        {
            if (_db == null) return;

            try
            {
                string whereClause = " WHERE p.Status = 'Đã phê duyệt'";
                var parameters = new System.Collections.Generic.List<MySqlConnector.MySqlParameter>();

                if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    whereClause += " AND (s.FullName LIKE @Search OR s.SoldierCode LIKE @Search)";
                    parameters.Add(new MySqlConnector.MySqlParameter("@Search", $"%{txtSearch.Text.Trim()}%"));
                }

                if (cboUnitFilter.SelectedValue != null && (int)cboUnitFilter.SelectedValue != -1)
                {
                    whereClause += " AND s.UnitID = @UnitID";
                    parameters.Add(new MySqlConnector.MySqlParameter("@UnitID", cboUnitFilter.SelectedValue));
                }

                if (cboCategoryFilter.SelectedValue != null && (int)cboCategoryFilter.SelectedValue != -1)
                {
                    whereClause += " AND p.CategoryID = @CatID";
                    parameters.Add(new MySqlConnector.MySqlParameter("@CatID", cboCategoryFilter.SelectedValue));
                }

                // Get Total Count
                string countQuery = "SELECT COUNT(*) FROM Proposals p JOIN Soldiers s ON p.SoldierID = s.SoldierID " + whereClause;
                DataTable dtCount = await _db.ExecuteQueryAsync(countQuery, parameters.ToArray());
                _totalItems = Convert.ToInt32(dtCount.Rows[0][0]);
                _totalPages = (int)Math.Ceiling((double)_totalItems / _pageSize);
                if (_totalPages == 0) _totalPages = 1;

                // Load Current Page
                string query = @"
                    SELECT s.SoldierID, s.FullName, s.Rank, s.Position, u.UnitName, c.CategoryName, p.DateProposed
                    FROM Proposals p
                    JOIN Soldiers s ON p.SoldierID = s.SoldierID
                    LEFT JOIN Units u ON s.UnitID = u.UnitID
                    LEFT JOIN RewardCategories c ON p.CategoryID = c.CategoryID" +
                    whereClause +
                    " ORDER BY p.DateProposed DESC LIMIT @PageSize OFFSET @Offset";

                parameters.Add(new MySqlConnector.MySqlParameter("@PageSize", _pageSize));
                parameters.Add(new MySqlConnector.MySqlParameter("@Offset", (_currentPage - 1) * _pageSize));

                DataTable dt = await _db.ExecuteQueryAsync(query, parameters.ToArray());
                Rewards.Clear();
                int stt = (_currentPage - 1) * _pageSize + 1;

                foreach (DataRow row in dt.Rows)
                {
                    Rewards.Add(new RewardItem
                    {
                        STT = stt++,
                        SoldierID = Convert.ToInt32(row["SoldierID"]),
                        Name = row["FullName"].ToString(),
                        Rank = row["Rank"].ToString(),
                        Position = row["Position"]?.ToString() ?? "",
                        Unit = row["UnitName"]?.ToString() ?? "",
                        CategoryName = row["CategoryName"]?.ToString() ?? "N/A",
                        Date = Convert.ToDateTime(row["DateProposed"]).ToString("dd/MM/yyyy")
                    });
                }

                UpdatePaginationUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách khen thưởng: " + ex.Message);
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

            // Show up to 5 page buttons around current page
            int startPage = Math.Max(1, _currentPage - 2);
            int endPage = Math.Min(_totalPages, startPage + 4);
            if (endPage - startPage < 4) startPage = Math.Max(1, endPage - 4);

            for (int i = startPage; i <= endPage; i++)
            {
                Button btn = new Button
                {
                    Content = i.ToString(),
                    Width = 40,
                    Height = 35,
                    Margin = new Thickness(0, 0, 8, 0),
                    Tag = i
                };

                if (i == _currentPage)
                {
                    btn.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#001529"));
                    btn.Foreground = System.Windows.Media.Brushes.White;
                }
                else
                {
                    btn.Background = System.Windows.Media.Brushes.White;
                    btn.BorderBrush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#E0E0E0"));
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

        private void dgRewardList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgRewardList.SelectedItem is RewardItem item)
            {
                OnSoldierSelected?.Invoke(this, item.SoldierID);
            }
        }
    }

    public class RewardItem
    {
        public int STT { get; set; }
        public int SoldierID { get; set; }
        public string Name { get; set; }
        public string Rank { get; set; }
        public string Position { get; set; }
        public string Unit { get; set; }
        public string CategoryName { get; set; }
        public string Date { get; set; }
    }
}
