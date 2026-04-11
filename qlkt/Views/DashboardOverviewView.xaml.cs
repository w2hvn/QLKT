using System.Collections.Generic;
using System.Windows.Controls;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace QLKT.Views
{
    public partial class DashboardOverviewView : UserControl
    {
        public DashboardOverviewView()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ReloadData();
        }

        private void BtnRefresh_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ReloadData();
        }

        private void ReloadData()
        {
            // In a real app, this would query the DB based on filters
            // For now, we simulate data change by slightly modifying values or just "refreshing"

            // Randomize some values to show change
            var rnd = new System.Random();
            if (TrendSeries != null && TrendSeries.Length > 0)
            {
                foreach (var series in TrendSeries)
                {
                    if (series is LineSeries<int> lineSeries)
                    {
                        var vals = new int[10];
                        for(int i=0; i<10; i++) vals[i] = rnd.Next(5, 20);
                        lineSeries.Values = vals;
                    }
                }
            }
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
