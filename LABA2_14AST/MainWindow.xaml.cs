using System.Windows;

namespace LABA2_14AST
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Устанавливаем ViewModel как DataContext
            DataContext = new MainViewModelThread();
        }
    }
}