using Microsoft.UI.Xaml;

namespace WinUIExample
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
        }

        void ClickExit(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
