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

        // 1. Thống kê khen thưởng theo tháng (Column Chart)
        public ISeries[] MonthlyRewardsSeries { get; set; } = new ISeries[]
        {
            new ColumnSeries<int>
            {
                Name = "Huy chương Chiến công",
                Values = new int[] { 7, 2, 4, 7, 5, 5, 8, 6, 5, 5, 8, 7 },
                Fill = new SolidColorPaint(SKColors.MidnightBlue)
            },
            new ColumnSeries<int>
            {
                Name = "Bằng khen",
                Values = new int[] { 5, 3, 2, 7, 3, 5, 6, 6, 5, 6, 3, 5 },
                Fill = new SolidColorPaint(SKColor.Parse("#E6B800"))
            },
            new ColumnSeries<int>
            {
                Name = "Huân chương",
                Values = new int[] { 8, 2, 6, 5, 5, 8, 8, 6, 5, 4, 6, 6 },
                Fill = new SolidColorPaint(SKColors.LightGray)
            }
        };

        public Axis[] MonthlyRewardsXAxes { get; set; } = new Axis[]
        {
            new Axis
            {
                Labels = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Okt", "Nov", "Dec" }
            }
        };

        // 2. Cơ cấu khen thưởng theo loại (Pie Chart)
        public ISeries[] RewardTypesSeries { get; set; } = new ISeries[]
        {
            new PieSeries<int> { Name = "Huy chương", Values = new int[] { 45 }, Fill = new SolidColorPaint(SKColors.MidnightBlue) },
            new PieSeries<int> { Name = "Bằng khen", Values = new int[] { 30 }, Fill = new SolidColorPaint(SKColor.Parse("#E6B800")) },
            new PieSeries<int> { Name = "Huân chương", Values = new int[] { 15 }, Fill = new SolidColorPaint(SKColors.LightGray) },
            new PieSeries<int> { Name = "Trung loại", Values = new int[] { 15 }, Fill = new SolidColorPaint(SKColors.DarkGray) },
            // ... omitting smaller ones for clarity, matching pie sections roughly
        };

        // 3. Xu hướng đề xuất và phê duyệt (Line Chart)
        public ISeries[] TrendSeries { get; set; } = new ISeries[]
        {
            new LineSeries<int>
            {
                Name = "Đề xuất mới",
                Values = new int[] { 10, 18, 15, 40, 15, 25, 20, 25, 30, 25, 35 },
                GeometrySize = 0,
                Stroke = new SolidColorPaint(SKColor.Parse("#E6B800")) { StrokeThickness = 3 },
                Fill = null
            },
            new LineSeries<int>
            {
                Name = "Đã phê duyệt",
                Values = new int[] { 8, 5, 8, 25, 35, 30, 20, 18, 22, 18, 45 },
                GeometrySize = 0,
                Stroke = new SolidColorPaint(SKColors.MidnightBlue) { StrokeThickness = 3 },
                Fill = null
            }
        };

        public Axis[] TrendXAxes { get; set; } = new Axis[]
        {
            new Axis
            {
                Labels = new string[] { "Quan", "Tháng", "Mith", "Hồng", "Thương", "Thiết", "2023", "Mạnh", "Toản", "Tranh", "Năm" } // Approximation from image
            }
        };

        // 4. Top đơn vị xuất sắc (Horizontal Bar Chart)
        public ISeries[] TopUnitsSeries { get; set; } = new ISeries[]
        {
            new RowSeries<int>
            {
                Values = new int[] { 14, 15, 10, 8, 3 },
                Fill = new SolidColorPaint(SKColors.MidnightBlue)
            }
        };

        public Axis[] TopUnitsYAxes { get; set; } = new Axis[]
        {
            new Axis
            {
                Labels = new string[] { "Sư đoàn 301", "Sư đoàn 316", "Sư đoàn 317", "Sư đoàn 213", "Sư đoàn 120" }
            }
        };
    }
}
