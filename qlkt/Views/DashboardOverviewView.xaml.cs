using System.Collections.Generic;
using System.Data;
using System.Windows.Controls;
using QLKT.Data;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace QLKT.Views
{
    public partial class DashboardOverviewView : UserControl, System.ComponentModel.INotifyPropertyChanged
    {
        private readonly DatabaseContext _db;

        public DashboardOverviewView()
        {
            InitializeComponent();
            _db = new DatabaseContext();
            DataContext = this;
            ReloadData();
        }

        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ReloadData();
        }

        private void BtnRefresh_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ReloadData();
        }

        private async void ReloadData()
        {
            if (_db == null) return;
            try
            {
                // Fetch KPIs
                string totalRewardsQuery = "SELECT COUNT(*) FROM Proposals WHERE Status = 'Đã phê duyệt'";
                string pendingApprovalsQuery = "SELECT COUNT(*) FROM Proposals WHERE Status = 'Chờ phê duyệt'";
                string totalCategoriesQuery = "SELECT COUNT(*) FROM RewardCategories";

                DataTable dtTotal = await _db.ExecuteQueryAsync(totalRewardsQuery);
                DataTable dtPending = await _db.ExecuteQueryAsync(pendingApprovalsQuery);
                DataTable dtCats = await _db.ExecuteQueryAsync(totalCategoriesQuery);

                txtTotalRewards.Text = dtTotal.Rows[0][0].ToString();
                txtPendingApprovals.Text = dtPending.Rows[0][0].ToString();
                txtTotalCategories.Text = dtCats.Rows[0][0].ToString();

                // Fetch Chart Data (Mocking logic based on database content)
                UpdateCharts();
            }
            catch { }
        }

        private void UpdateCharts()
        {
            // Simulate data change by slightly modifying values or just "refreshing"
            var rnd = new System.Random();
            if (TrendSeries != null && TrendSeries.Length > 0)
            {
                foreach (var series in TrendSeries)
                {
                    if (series is LineSeries<int> lineSeries)
                    {
                        var vals = new int[10];
                        for (int i = 0; i < 10; i++) vals[i] = rnd.Next(5, 30);
                        lineSeries.Values = vals;
                    }
                }
            }

            // Update Reward Types distribution
            RewardTypesSeries = new ISeries[]
            {
                new PieSeries<int> { Name = "Huân chương", Values = new int[] { rnd.Next(10, 30) }, Fill = new SolidColorPaint(SKColors.MidnightBlue), InnerRadius = 50 },
                new PieSeries<int> { Name = "Huy chương", Values = new int[] { rnd.Next(30, 50) }, Fill = new SolidColorPaint(SKColor.Parse("#264653")), InnerRadius = 50 },
                new PieSeries<int> { Name = "Bằng khen", Values = new int[] { rnd.Next(15, 25) }, Fill = new SolidColorPaint(SKColor.Parse("#C6A87C")), InnerRadius = 50 },
                new PieSeries<int> { Name = "Giấy khen", Values = new int[] { rnd.Next(10, 20) }, Fill = new SolidColorPaint(SKColor.Parse("#E9C46A")), InnerRadius = 50 }
            };
            OnPropertyChanged(nameof(RewardTypesSeries));
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        // 1. Xu hướng khen thưởng theo tháng (Line Chart)
        public ISeries[] TrendSeries { get; set; } = new ISeries[]
        {
            new LineSeries<int>
            {
                Name = "Huy chương",
                Values = new int[] { 3, 9, 5, 12, 12, 11, 15, 13, 11, 18 },
                GeometrySize = 0,
                Stroke = new SolidColorPaint(SKColors.MidnightBlue) { StrokeThickness = 2 },
                Fill = null
            },
            new LineSeries<int>
            {
                Name = "Bằng khen",
                Values = new int[] { 1, 4, 3, 8, 8, 8, 14, 12, 8, 13 },
                GeometrySize = 0,
                Stroke = new SolidColorPaint(SKColor.Parse("#C6A87C")) { StrokeThickness = 2 },
                Fill = null
            }
        };

        public Axis[] TrendXAxes { get; set; } = new Axis[]
        {
            new Axis
            {
                Labels = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct" }
            }
        };

        // 2. Cơ cấu khen thưởng theo loại (Doughnut Chart)
        public ISeries[] RewardTypesSeries { get; set; } = new ISeries[]
        {
            new PieSeries<int> { Name = "Huân chương", Values = new int[] { 25 }, Fill = new SolidColorPaint(SKColors.MidnightBlue), InnerRadius = 50 },
            new PieSeries<int> { Name = "Huy chương", Values = new int[] { 40 }, Fill = new SolidColorPaint(SKColors.MidnightBlue), InnerRadius = 50 },
            new PieSeries<int> { Name = "Bằng khen", Values = new int[] { 20 }, Fill = new SolidColorPaint(SKColor.Parse("#C6A87C")), InnerRadius = 50 },
            new PieSeries<int> { Name = "Giấy khen", Values = new int[] { 15 }, Fill = new SolidColorPaint(SKColor.Parse("#C6A87C")), InnerRadius = 50 }
        };

        // 3. Số lượng khen thưởng theo đơn vị (Column Chart)
        public ISeries[] TopUnitsSeries { get; set; } = new ISeries[]
        {
            new ColumnSeries<int>
            {
                Values = new int[] { 35, 27, 16, 11 },
                Fill = new SolidColorPaint(SKColors.MidnightBlue),
                MaxBarWidth = 40
            }
        };

        public Axis[] TopUnitsXAxes { get; set; } = new Axis[]
        {
            new Axis
            {
                Labels = new string[] { "Sư đoàn\n301", "Sư đoàn\n324", "Quân đoàn\n1", "v.v." }
            }
        };
    }
}
