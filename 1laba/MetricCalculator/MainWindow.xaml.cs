using MetricCalculator.logic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MetricCalculator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Parser parser;
        public MainWindow()
        {
            InitializeComponent();
            parser = new Parser(null, null);
            parser.LoadInformation();
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            TextRange textRange = new TextRange(CodeInput.Document.ContentStart, CodeInput.Document.ContentEnd);
            parser.Parse(textRange.Text);
            TextRange textRange2 = new TextRange(TokensOutput.Document.ContentStart, TokensOutput.Document.ContentEnd);
            textRange2.Text = parser.PrintTokens();

            BuildTable();
            Metrics.Text = parser.ResultMetrics;
        }
        

        public void BuildTable()
        {
            List<MetricItem> operators = new List<MetricItem>();
            List<MetricItem> operands = new List<MetricItem>();

            foreach (var op in parser.Operators)
            {
                operators.Add(new MetricItem(op.Key, op.Value));
            }
            foreach (var op in parser.Operands)
            {
                operands.Add(new MetricItem(op.Key, op.Value));
            }
            // Здесь пока пример
            //operators.Add(new MetricItem("+", 15));
            //operators.Add(new MetricItem("-", 7));
            //operators.Add(new MetricItem("*", 12));
            //operators.Add(new MetricItem("=", 20));
            //operators.Add(new MetricItem("if", 3));

            //operands.Add(new MetricItem("x", 10));
            //operands.Add(new MetricItem("y", 8));
            //operands.Add(new MetricItem("result", 5));
            //operands.Add(new MetricItem("10", 4));

            OperatorsTable.ItemsSource = operators;
            OperandsTable.ItemsSource = operands;
            OperatorsCountLabel1.Text = "Количество уникальных операторов: " + parser.Operators.Count;
            OperandsCountLabel1.Text = "Количество уникальных операндов: " + parser.Operands.Count;
            OperatorsCountLabel2.Text = "Общее количество операторов: " + parser.Operators.Values.Sum();
            OperandsCountLabel2.Text = "Общее количество операндов: " + parser.Operands.Values.Sum();
        }

        private void SaveFileButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void LoadFileButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }



    public class MetricItem
    {
        public string Name { get; set; }
        public int Count { get; set; }

        public MetricItem(string name, int count)
        {
            Name = name;
            Count = count;
        }
    }
}