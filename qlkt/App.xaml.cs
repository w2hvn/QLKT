using System.Configuration;
using System.Data;
using System.Windows;

namespace QLKT
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string CurrentUserRole { get; set; } = "Admin";
        public static string CurrentUserName { get; set; } = "Administrator";
    }

}
