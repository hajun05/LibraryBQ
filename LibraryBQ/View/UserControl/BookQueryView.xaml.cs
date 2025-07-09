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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LibraryBQ.View
{
    /// <summary>
    /// Interaction logic for BookQueryView.xaml
    /// </summary>
    public partial class BookQueryView : UserControl
    {
        private string _queryText;

        public BookQueryView()
        {
            InitializeComponent();
        }

        private void BookQueryBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(BookQueryBox.Text))
                BookQueryPromptText.Visibility = Visibility.Hidden;
            else
                BookQueryPromptText.Visibility = Visibility.Visible;
        }
    }
}
