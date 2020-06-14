using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace TelephoneCallsBTK.Window
{
    /// <summary>
    /// Логика взаимодействия для About.xaml
    /// </summary>
    public partial class About : System.Windows.Window
    {
        public About()
        {
            InitializeComponent();
            Block.Text = "Версия сборки: " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
