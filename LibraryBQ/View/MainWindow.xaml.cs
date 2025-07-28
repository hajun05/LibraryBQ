using LibraryBQ.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LibraryBQ.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // App.xaml.cs에서 DI 컨테이너가 직접 생성 및 생성자 주입
        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            // 생성자 인수를 통한 ViewModel - View 매핑
            DataContext = viewModel;
        }

        // 종료 버튼
        private void Closebtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // 최소화 버튼
        private void Minimizebtn_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // 타이틀바 잡고 이동
        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}
