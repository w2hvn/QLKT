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
        private RewardListView _rewardListView = new RewardListView();

        public MainWindow()
        {
            InitializeComponent();
            MainContent.Content = _dashboardView;
            UpdateSubNavStyle(borderSubNavOverview, txtSubNavOverview);
            UpdateSidebarStyle(btnNavOverview);
        }

        private void UpdateSubNavStyle(Border activeBorder, TextBlock activeText)
        {
            // Reset all sub-navs
            borderSubNavOverview.BorderBrush = Brushes.Transparent;
            txtSubNavOverview.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666"));
            txtSubNavOverview.FontWeight = FontWeights.SemiBold;

            borderSubNavReward.BorderBrush = Brushes.Transparent;
            txtSubNavReward.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666"));
            txtSubNavReward.FontWeight = FontWeights.SemiBold;

            // Set active
            if (activeBorder != null && activeText != null)
            {
                activeBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#001529"));
                activeText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#001529"));
                activeText.FontWeight = FontWeights.Bold;
            }
        }

        private void UpdateSidebarStyle(Button activeButton)
        {
            btnNavOverview.Tag = "Secondary";
            btnNavRewardList.Tag = "Secondary";

            if (activeButton != null)
            {
                activeButton.Tag = "Dark";
            }
        }

        private void Nav_Dashboard_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = _dashboardView;
            UpdateSubNavStyle(borderSubNavOverview, txtSubNavOverview);
            UpdateSidebarStyle(btnNavOverview);
        }

        private void Nav_RewardList_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = _rewardListView;
            UpdateSubNavStyle(borderSubNavReward, txtSubNavReward);
            UpdateSidebarStyle(btnNavRewardList);
        }
    }
}
