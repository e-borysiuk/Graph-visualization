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

namespace Labirynth
{
    /// <summary>
    /// Interaction logic for CreateWindow.xaml
    /// </summary>
    public partial class EndWindow : Window
    {
        public EndWindow()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            int x, y;
            if (TbX.Text != "" && int.TryParse(TbX.Text, out x) && TbY.Text != "" && int.TryParse(TbY.Text, out y))
            {
                MainWindow.endX = x;
                MainWindow.endY = y;
                Close();
            }
            else
            {
                MessageBox.Show("Błąd!", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
