using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace KMLR1.Classes
{
    internal class MathCalculator
    {
        public static double CalculateCorrelation(double[] column1, double[] column2)
        {
            return Correlation.Pearson(column1, column2);
        }

        public static void CalculateCoefficients(double[,] matrix, out double b0, out double b1, out double b2, out double b3)
        {
            int rowCount = matrix.GetLength(0);

            // Створіть матрицю A та вектор b для системи лінійних рівнянь
            Matrix<double> A = Matrix<double>.Build.Dense(rowCount, 4);
            Vector<double> b = Vector<double>.Build.Dense(rowCount);

            for (int row = 0; row < rowCount; row++)
            {
                A[row, 0] = 1.0;         // b0 coefficient
                A[row, 1] = matrix[row, 0]; // b1 coefficient (x1)
                A[row, 2] = matrix[row, 1]; // b2 coefficient (x2)
                A[row, 3] = matrix[row, 2]; // b3 coefficient (x3)
                b[row] = matrix[row, 3];   // y
            }

            // Обчисліть коефіцієнти за допомогою методу найменших квадратів
            var result = A.Solve(b);

            // Виділіть коефіцієнти з результату
            b0 = result[0];
            b1 = result[1];
            b2 = result[2];
            b3 = result[3];
        }

        public static double CalculateRMSE(double[,] matrix, double b0, double b1, double b2, double b3)
        {
            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);

            if (colCount < 4)
            {
                throw new ArgumentException("Matrix must have at least 4 columns for x1, x2, x3, and y values.");
            }

            double rmse = 0.0;

            for (int row = 0; row < rowCount; row++)
            {
                double x1 = matrix[row, 0]; // Перший стовпець
                double x2 = matrix[row, 1]; // Другий стовпець
                double x3 = matrix[row, 2]; // Третій стовпець
                double y = matrix[row, colCount - 1]; // Останній стовпець (y)

                double predictedY = b0 + b1 * x1 + b2 * x2 + b3 * x3;
                double error = y - predictedY;
                rmse += error * error;
            }

            rmse = Math.Sqrt(rmse / rowCount);
            return rmse;
        }

        public static double CalculateFisherCriterion(double[,] matrix, double b0, double b1, double b2, double b3)
        {
            int n = matrix.GetLength(0); // Кількість спостережень (рядків)
            int k = 3; // Кількість коефіцієнтів (регресорів) у вашій моделі

            double YAvg = CalculateYAvg(matrix);

            double numerator = 0.0; // Чисельник
            double denominator = 0.0; // Знаменник

            for (int i = 0; i < n; i++)
            {
                double yPredicted = b0 + b1 * matrix[i, 0] + b2 * matrix[i, 1] + b3 * matrix[i, 2];
                numerator += Math.Pow((YAvg - yPredicted), 2);
                denominator += Math.Pow(matrix[i, 3] - yPredicted, 2);
            }

            double fStatistic = (numerator * (n - k - 1)) / (denominator * k);

            return fStatistic;
        }

        public static double CalculateYAvg(double[,] matrix)
        {
            int n = matrix.GetLength(0);
            double yTotal = 0.0;

            for (int i = 0; i < n; i++)
            {
                yTotal += matrix[i, 3];
            }

            return yTotal / n;
        }

        public static double GetFisherConst(double[,] matrix, double alpha)
        {

            int df1 = matrix.GetLength(1)-1; // Ступені свободи чисельника
            int df2 = matrix.GetLength(0)-df1-1; // Ступені свободи знаменника

            // Знайти табличне значення критерію Фішера
            double criticalValue = FisherSnedecor.InvCDF(df1, df2, 1 - alpha);
            return criticalValue;
        }
        public static double[] CalculateSignificanceCoefficients(double[,] matrix, double b0, double b1, double b2, double b3)
        {
            int n = matrix.GetLength(0); // Кількість спостережень
            int k = 4; // Кількість параметрів (b0, b1, b2, b3)

            // Розраховуємо стандартну помилку моделі
            double sse = 0.0; // Сума квадратів залишкових помилок
            for (int i = 0; i < n; i++)
            {
                double yActual = matrix[i, 3]; // Остання колонка містить значення y
                double yPredicted = b0 + b1 * matrix[i, 0] + b2 * matrix[i, 1] + b3 * matrix[i, 2];
                sse += Math.Pow(yActual - yPredicted, 2);
            }
            double mse = sse / (n - k); // Середньоквадратична помилка

            // Розраховуємо стандартну помилку параметрів моделі
            double[] standardErrors = new double[k];
            standardErrors[0] = Math.Sqrt(mse); // Параметр b0
            for (int j = 1; j < k; j++)
            {
                double xSumOfSquares = 0.0;
                for (int i = 0; i < n; i++)
                {
                    xSumOfSquares += Math.Pow(matrix[i, j - 1], 2);
                }
                standardErrors[j] = Math.Sqrt(mse / (xSumOfSquares * n));
            }

            // Розраховуємо значення t-критерію Стьюдента
            double[] tValues = new double[k];
            for (int j = 0; j < k; j++)
            {
                tValues[j] = b0 / standardErrors[j];
            }

            // Розраховуємо t-критичне (за вибраною рівнем значущості alpha)
            double alpha = 0.05; // Рівень значущості (0.05 для 95% довіри)
            //StudentTDistribution tDistribution = new StudentTDistribution(n - k);
            StudentT tDistribution = new StudentT();
            double tCritical = tDistribution.InverseCumulativeDistribution(1 - alpha / 2);

            return tValues;
        }


    }
}
