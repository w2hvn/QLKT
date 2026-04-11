using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using QLKT.Views;

namespace QLKT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DashboardOverviewView _dashboardView = new DashboardOverviewView();
        private SoldierProfileView _soldierProfileView = new SoldierProfileView();
        private RewardProposalView _rewardProposalView = new RewardProposalView();

        public MainWindow()
        {
            InitializeComponent();
            NavigateTo(_dashboardView, borderDashboardOverview);
        }

        private void ResetNavStyles()
        {
            borderDashboardOverview.Background = Brushes.Transparent;
            ((TextBlock)borderDashboardOverview.Child).Foreground = (Brush)Application.Current.Resources["Brush.Text.Secondary"];

            borderSoldierProfile.Background = Brushes.Transparent;
            ((TextBlock)borderSoldierProfile.Child).Foreground = (Brush)Application.Current.Resources["Brush.Text.Secondary"];

            borderRewardManagement.Background = Brushes.Transparent;
            ((TextBlock)borderRewardManagement.Child).Foreground = (Brush)Application.Current.Resources["Brush.Text.Secondary"];
        }

        private void NavigateTo(UserControl view, Border activeBorder)
        {
            ResetNavStyles();

            // Set active style
            var color = (Color)ColorConverter.ConvertFromString("#001529");
            activeBorder.Background = new SolidColorBrush(color);
            ((TextBlock)activeBorder.Child).Foreground = Brushes.White;

            MainContent.Content = view;
        }

        private void Nav_Dashboard_Click(object sender, MouseButtonEventArgs e)
        {
            NavigateTo(_dashboardView, borderDashboardOverview);
        }

        private void Nav_SoldierProfile_Click(object sender, MouseButtonEventArgs e)
        {
            NavigateTo(_soldierProfileView, borderSoldierProfile);
        }

        private void Nav_RewardManagement_Click(object sender, MouseButtonEventArgs e)
        {
            NavigateTo(_rewardProposalView, borderRewardManagement);
        }
    }
}
