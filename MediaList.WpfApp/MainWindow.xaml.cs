using System.Windows;

namespace MediaList.WpfApp;

public partial class MainWindow : Window
{
    private readonly MainViewModel viewModel = new();

    public MainWindow()
    {
        InitializeComponent();

        DataContext = viewModel;
        Loaded += async (s, e) => await viewModel.LoadAsync();
    }
}   