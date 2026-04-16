using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WPF;
using ClosedXML.Excel;
using QLKT.Data;
using MySqlConnector;

namespace QLKT.Views
{
    public partial class ReportManagement : UserControl
    {
        private readonly DatabaseContext _db;
        private DataTable _currentData;

        public ReportManagement()
        {
            InitializeComponent();
            _db = new DatabaseContext();
            LoadUnits();
            LoadSummary();
        }

        private async void LoadSummary()
        {
            try
            {
                var dtTotal = await _db.ExecuteQueryAsync("SELECT COUNT(*) FROM Proposals WHERE Status = 'Đã phê duyệt'");
                var dtMonthly = await _db.ExecuteQueryAsync("SELECT COUNT(*) FROM Proposals WHERE Status = 'Đã phê duyệt' AND MONTH(DateProposed) = MONTH(CURRENT_DATE()) AND YEAR(DateProposed) = YEAR(CURRENT_DATE())");
                var dtUnits = await _db.ExecuteQueryAsync("SELECT COUNT(DISTINCT UnitID) FROM Soldiers WHERE SoldierID IN (SELECT SoldierID FROM Proposals WHERE Status = 'Đã phê duyệt')");

                txtTotalDecisions.Text = dtTotal.Rows[0][0].ToString();
                txtMonthlyRewards.Text = dtMonthly.Rows[0][0].ToString();
                txtTotalUnits.Text = dtUnits.Rows[0][0].ToString();
            }
            catch { }
        }

        private async void LoadUnits()
        {
            try
            {
                var dt = await _db.ExecuteQueryAsync("SELECT UnitID, UnitName FROM Units");
                // Add "All" option manually if needed, or handle null selection as All
                cboUnitFilter.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải đơn vị: " + ex.Message); }
        }

        private async void Generate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Dynamic SQL Construction
                var selectedColumns = new List<string>();
                var displayHeaders = new List<string>(); // For Excel header if needed

                if (chkCode.IsChecked == true) { selectedColumns.Add("s.SoldierCode"); displayHeaders.Add("Mã QN"); }
                if (chkName.IsChecked == true) { selectedColumns.Add("s.FullName"); displayHeaders.Add("Họ tên"); }
                if (chkRank.IsChecked == true) { selectedColumns.Add("s.Rank"); displayHeaders.Add("Cấp bậc"); }
                if (chkUnit.IsChecked == true) { selectedColumns.Add("u.UnitName"); displayHeaders.Add("Đơn vị"); }
                if (chkPos.IsChecked == true) { selectedColumns.Add("s.Position"); displayHeaders.Add("Chức vụ"); }
                if (chkReward.IsChecked == true) { selectedColumns.Add("c.CategoryName"); displayHeaders.Add("Khen thưởng"); }
                if (chkReason.IsChecked == true) { selectedColumns.Add("r.Reason"); displayHeaders.Add("Lý do"); }
                if (chkDate.IsChecked == true) { selectedColumns.Add("r.DateProposed"); displayHeaders.Add("Ngày ký"); }

                if (selectedColumns.Count == 0)
                {
                    MessageBox.Show("Vui lòng chọn ít nhất một trường dữ liệu.");
                    return;
                }

                string selectClause = string.Join(", ", selectedColumns);
                string baseQuery = $@"SELECT {selectClause} 
                                      FROM Proposals r
                                      JOIN Soldiers s ON r.SoldierID = s.SoldierID
                                      LEFT JOIN Units u ON s.UnitID = u.UnitID
                                      LEFT JOIN RewardCategories c ON r.CategoryID = c.CategoryID
                                      WHERE r.Status = 'Đã phê duyệt'";

                // Filters
                var parameters = new List<MySqlParameter>();

                if (cboUnitFilter.SelectedValue != null)
                {
                    baseQuery += " AND s.UnitID = @UnitID";
                    parameters.Add(new MySqlParameter("@UnitID", cboUnitFilter.SelectedValue));
                }

                if (dpFrom.SelectedDate != null)
                {
                    baseQuery += " AND r.DateProposed >= @FromDate";
                    parameters.Add(new MySqlParameter("@FromDate", dpFrom.SelectedDate.Value.ToString("yyyy-MM-dd")));
                }

                if (dpTo.SelectedDate != null)
                {
                    baseQuery += " AND r.DateProposed <= @ToDate";
                    parameters.Add(new MySqlParameter("@ToDate", dpTo.SelectedDate.Value.ToString("yyyy-MM-dd")));
                }

                if (cboRewardType.SelectedItem is ComboBoxItem item && item.Content.ToString() != "Tất cả")
                {
                    baseQuery += " AND c.CategoryName = @RewardName";
                    parameters.Add(new MySqlParameter("@RewardName", item.Content.ToString()));
                }

                _currentData = await _db.ExecuteQueryAsync(baseQuery, parameters.ToArray());
                dgReport.ItemsSource = _currentData.DefaultView;

                UpdateAdvancedCharts();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tạo báo cáo: " + ex.Message);
            }
        }

        private void UpdateAdvancedCharts()
        {
            if (_currentData == null) return;

            // 1. Reward Distribution (Pie Chart)
            if (chkReward.IsChecked == true)
            {
                var distribution = _currentData.AsEnumerable()
                    .GroupBy(r => r.Field<string>("CategoryName"))
                    .Select(g => new PieSeries<int> { Name = g.Key, Values = new[] { g.Count() } })
                    .ToArray();

                var pieChart = new PieChart { Series = distribution, Height = 150 };
                chartDistribution.Content = pieChart;
            }

            // 2. Top Performing Units (Bar Chart)
            if (chkUnit.IsChecked == true)
            {
                var unitData = _currentData.AsEnumerable()
                    .GroupBy(r => r.Field<string>("UnitName"))
                    .Select(g => new { Unit = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(5)
                    .ToList();

                var barChart = new CartesianChart
                {
                    Series = new ISeries[] {
                        new ColumnSeries<int> { Values = unitData.Select(x => x.Count).ToArray() }
                    },
                    XAxes = new[] {
                        new Axis { Labels = unitData.Select(x => x.Unit).ToArray() }
                    },
                    Height = 150
                };
                chartUnits.Content = barChart;
            }

            // 3. Trends (Line Chart) - Simple count by date
            if (chkDate.IsChecked == true)
            {
                var trendData = _currentData.AsEnumerable()
                    .GroupBy(r => Convert.ToDateTime(r.Field<object>("DateProposed")).ToString("MM/yyyy"))
                    .Select(g => new { Month = g.Key, Count = g.Count() })
                    .OrderBy(x => x.Month)
                    .ToList();

                var lineChart = new CartesianChart
                {
                    Series = new ISeries[] {
                        new LineSeries<int> { Values = trendData.Select(x => x.Count).ToArray() }
                    },
                    XAxes = new[] {
                        new Axis { Labels = trendData.Select(x => x.Month).ToArray() }
                    },
                    Height = 150
                };
                chartTrend.Content = lineChart;
            }
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            if (_currentData == null || _currentData.Rows.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất.");
                return;
            }

            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("BaoCaoKhenThuong");

                    // Insert DataTable
                    worksheet.Cell(1, 1).InsertTable(_currentData);

                    // Style
                    worksheet.Columns().AdjustToContents();

                    // Save file dialog (Simulated here since we are in sandbox, but usually use SaveFileDialog)
                    // In real WPF:
                    // Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                    // dlg.FileName = "BaoCao_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"); 
                    // dlg.DefaultExt = ".xlsx";
                    // if (dlg.ShowDialog() == true) workbook.SaveAs(dlg.FileName);

                    // For this sandbox/demo, we'll save to a fixed path or just show message
                    string fileName = $"Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    workbook.SaveAs(fileName);
                    MessageBox.Show($"Xuất Excel thành công: {fileName}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xuất Excel: " + ex.Message);
            }
        }
    }
}
