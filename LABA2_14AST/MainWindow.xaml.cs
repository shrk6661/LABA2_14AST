using System.Windows;

namespace LABA2_14AST
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            
            DataContext = new MainViewModelThread();
        }
    }
}