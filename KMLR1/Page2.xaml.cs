using EcoMonitoringIS.Classes;
using KMLR1.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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

namespace EcoMonitoringIS.View
{
    /// <summary>
    /// Interaction logic for Page2.xaml
    /// </summary>
    public partial class Page2 : Page
    {
        public Page2()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var path = FileControl.OpenFileNameDialog();
            PathTextBox1.Text = path;
            //DisplayTable(path);
        }

        private void PathTextBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            DisplayTable(PathTextBox1.Text);
        }

        public void DisplayTable(string path)
        {

            if (File.Exists(path))
            {
                var Collection = ExcelControl.ExelToTableColl(path);
                DataTable table = Collection[0];
                DBGrid2.ItemsSource = table.DefaultView;
                ///-------------------------------------------------------------------------------------
                var dataMatrix = TableToDoubleMatrix(table);
                SplitMatrixToColumns(dataMatrix, out double[] col2, out double[] col3, out double[] col4, out double[] col5);
                String CorRes = "Розкриті злочини - Кількість поліцейських на 100 жителів:  " + MathCalculator.CalculateCorrelation(col5,col2);
                CorRes += "\n\nРозкриті злочини - Кількість поліцейських в одному патрулі:  " + MathCalculator.CalculateCorrelation(col5, col3);
                CorRes += "\n\nРозкриті злочини - Кількість патрулів:  " + MathCalculator.CalculateCorrelation(col5, col4);
                CorRes += "\n\nКількість поліцейських на 100 жителів - Кількість патрулів:  " + MathCalculator.CalculateCorrelation(col2, col4);
                CorRes += "\n\nКількість поліцейських на 100 жителів - Кількість поліцейських в одному патрулі:  " + MathCalculator.CalculateCorrelation(col5, col3);
                CorRes += "\n\nКількість патрулів - Кількість поліцейських в одному патрулі:  " + MathCalculator.CalculateCorrelation(col4, col3);
                TextBox1.Text = CorRes;

                var dataMatrix2 = RemoveFirstColumn(dataMatrix);
                MathCalculator.CalculateCoefficients(dataMatrix2, out double b0, out double b1, out double b2, out double b3);
                TextBox2.Text = $"{b0} + {b1}*x1 + {b2}*x2 + {b3}*x3";

                double err = MathCalculator.CalculateRMSE(dataMatrix2, b0, b1, b2, b3);
                double fis = MathCalculator.CalculateFisherCriterion(dataMatrix2, b0, b1, b2, b3);
                double fisCons = MathCalculator.GetFisherConst(dataMatrix2, 0.01);
                TextBox3.Text = $" Середньоквадратична похибка:  {err}\n Критерій Фішера:  {fis}\n Критерій Фішера(табличне значеня): {fisCons}";
            }
        }

        public double[,] TableToDoubleMatrix(DataTable dataTable)
        {
            int rowCount = dataTable.Rows.Count;
            int colCount = dataTable.Columns.Count;

            double[,] data = new double[rowCount, colCount];

            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < colCount; col++)
                {
                    if (double.TryParse(dataTable.Rows[row][col].ToString(), out double value))
                    {
                        data[row, col] = value;
                    }
                }
            }
            return data;
        }

        public void SplitMatrixToColumns(double[,] matrix, out double[] col2, out double[] col3, out double[] col4, out double[] col5)
        {
            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);

            col2 = new double[rowCount];
            col3 = new double[rowCount];
            col4 = new double[rowCount];
            col5 = new double[rowCount];

            for (int row = 0; row < rowCount; row++)
            {
                col2[row] = matrix[row, 1];
                col3[row] = matrix[row, 2];
                col4[row] = matrix[row, 3];
                col5[row] = matrix[row, 4];
            }
        }

        public double[,] RemoveFirstColumn(double[,] originalMatrix)
        {
            int rowCount = originalMatrix.GetLength(0);
            int colCount = originalMatrix.GetLength(1);

            if (colCount <= 1)
            {
                // Матриця має лише один стовпець, і його не можна прибрати.
                return originalMatrix;
            }

            double[,] resultMatrix = new double[rowCount, colCount - 1];

            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 1; col < colCount; col++)
                {
                    resultMatrix[row, col - 1] = originalMatrix[row, col];
                }
            }

            return resultMatrix;
        }


    }
}
