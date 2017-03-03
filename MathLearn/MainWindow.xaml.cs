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

namespace MathLearn
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void testButton_Click(object sender, RoutedEventArgs e)
        {
            postfixTextBox.Text = "";
            string input = infixTextBox.Text;
            List<string> output = Postfixer.ConvertRPN(input);
            double result = Postfixer.CalculateExpression(input);
            foreach (string t in output)
            {
                postfixTextBox.Text += t + " ";
            }
            resultTextBox.Text = result.ToString();
        }

        private void equotRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            evalPanel.IsEnabled = false;
            equotPanel.IsEnabled = true;
        }

        private void evalRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            evalPanel.IsEnabled = true;
            equotPanel.IsEnabled = false;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            leftTextBox.Text = "";
            rightTextBox.Text = "";
            string[] parts = equotTextBox.Text.Split('=');
            List<string> left = Postfixer.ConvertRPN(parts[0]);
            List<string> right = Postfixer.ConvertRPN(parts[1]);
            foreach (var s in right)
                rightTextBox.Text += s + " ";
            foreach (var s in left)
                leftTextBox.Text += s + " ";
        }

        private void MainWindow_Load(object sender, RoutedEventArgs e)
        {
            evalRadioButton.IsChecked = true;
        }
    }
}
