using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
                                      LEFT JOIN Units u ON s.UnitID = u.UnitID LEFT JOIN RewardCategories c ON r.CategoryID = c.CategoryID
                                      WHERE 1=1";

                // Filters
                if (cboUnitFilter.SelectedValue != null)
                {
                    baseQuery += $" AND s.UnitID = {cboUnitFilter.SelectedValue}";
                }

                if (dpFrom.SelectedDate != null)
                {
                    baseQuery += $" AND r.DateProposed >= '{dpFrom.SelectedDate:yyyy-MM-dd}'";
                }

                if (dpTo.SelectedDate != null)
                {
                    baseQuery += $" AND r.DateProposed <= '{dpTo.SelectedDate:yyyy-MM-dd}'";
                }

                if (cboRewardType.SelectedItem is ComboBoxItem item && item.Content.ToString() != "Tất cả")
                {
                    baseQuery += $" AND c.CategoryName = '{item.Content}'";
                }

                _currentData = await _db.ExecuteQueryAsync(baseQuery);
                dgReport.ItemsSource = _currentData.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tạo báo cáo: " + ex.Message);
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
