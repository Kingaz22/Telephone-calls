using TelephoneCallsBTK.ViewModel;

namespace TelephoneCallsBTK
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext =
                new ApplicationViewModel(new DefaultDialogService(), new CsvFileService());

        }

    }
}
