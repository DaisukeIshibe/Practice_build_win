using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CsvHelper;
using CsvHelper.Configuration;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Random;

namespace ViewTimeSeries
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<StockData> stockDataList = new List<StockData>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenCsvMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                LoadCsv(openFileDialog.FileName);
            }
        }

        private void GenerateTestDataMenuItem_Click(object sender, RoutedEventArgs e)
        {
            stockDataList = GenerateTestData();
            DrawPointFigureChart();
        }

        private void LoadCsv(string filePath)
        {
            try
            {
                using (var reader = new StreamReader(filePath))
                using (var csv = new CsvReader(reader, new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)))
                {
                    var records = csv.GetRecords<dynamic>().ToList();
                    if (records.Count == 0)
                    {
                        MessageBox.Show("CSVファイルが空です。");
                        return;
                    }

                    var headers = ((IDictionary<string, object>)records[0]).Keys.ToList();
                    if (!headers.Contains("Date") || !headers.Contains("Open") || !headers.Contains("High") || !headers.Contains("Low") || !headers.Contains("Close"))
                    {
                        MessageBox.Show("CSVフォーマットが不適合です。Date, Open, High, Low, Closeの列が必要です。");
                        return;
                    }

                    stockDataList = records.Select(r =>
                    {
                        var dict = (IDictionary<string, object>)r;
                        return new StockData
                        {
                            Date = DateTime.Parse(dict["Date"].ToString()),
                            Open = double.Parse(dict["Open"].ToString()),
                            High = double.Parse(dict["High"].ToString()),
                            Low = double.Parse(dict["Low"].ToString()),
                            Close = double.Parse(dict["Close"].ToString())
                        };
                    }).ToList();
                }
                DrawPointFigureChart();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"CSV読み込みエラー: {ex.Message}");
            }
        }

        private List<StockData> GenerateTestData()
        {
            var data = new List<StockData>();
            var random = new MersenneTwister();
            var normal = new Normal(0, 1, random);
            double price = 100.0; // 初期価格
            double trend = 0.001; // トレンド (日次上昇率)
            DateTime startDate = new DateTime(2005, 1, 1);
            DateTime endDate = new DateTime(2025, 1, 1);

            for (DateTime date = startDate; date < endDate; date = date.AddDays(1))
            {
                double change = trend + normal.Sample() * 2.0; // トレンド + ランダム
                double open = price;
                double high = open + Math.Abs(normal.Sample()) * 5.0;
                double low = open - Math.Abs(normal.Sample()) * 5.0;
                double close = open + change;
                if (close < 0) close = 0; // 負の価格を避ける

                data.Add(new StockData { Date = date, Open = open, High = high, Low = low, Close = close });
                price = close;
            }
            return data;
        }

        private void DrawPointFigureChart()
        {
            ChartCanvas.Children.Clear();
            if (stockDataList.Count == 0) return;

            double boxSize = 1.0; // ボックスサイズ
            int reversal = 3; // リバーサル数
            var pfData = GeneratePointFigureData(stockDataList, boxSize, reversal);

            double xStep = 20; // X方向のステップ
            double yStep = 10; // Y方向のステップ
            double startY = 1900; // Y軸開始位置

            foreach (var point in pfData)
            {
                var textBlock = new TextBlock
                {
                    Text = point.Type,
                    FontSize = 12,
                    Foreground = point.Type == "X" ? Brushes.Green : Brushes.Red
                };
                Canvas.SetLeft(textBlock, point.Column * xStep);
                Canvas.SetTop(textBlock, startY - point.Row * yStep);
                ChartCanvas.Children.Add(textBlock);
            }
        }

        private List<PointFigurePoint> GeneratePointFigureData(List<StockData> data, double boxSize, int reversal)
        {
            var pfPoints = new List<PointFigurePoint>();
            double currentPrice = data.First().Close;
            double high = currentPrice;
            double low = currentPrice;
            string direction = "up"; // 初期方向
            int column = 0;
            var levels = new Dictionary<double, int>();

            foreach (var stock in data)
            {
                double price = stock.Close;
                if (direction == "up")
                {
                    if (price >= high + boxSize)
                    {
                        while (price >= high + boxSize)
                        {
                            high += boxSize;
                            int row = (int)((high - low) / boxSize);
                            if (!levels.ContainsKey(high)) levels[high] = column;
                            pfPoints.Add(new PointFigurePoint { Row = row, Column = column, Type = "X" });
                        }
                    }
                    else if (price <= high - boxSize * reversal)
                    {
                        direction = "down";
                        low = high - boxSize * reversal;
                        column++;
                        while (price <= low - boxSize)
                        {
                            low -= boxSize;
                            int row = (int)((high - low) / boxSize);
                            pfPoints.Add(new PointFigurePoint { Row = row, Column = column, Type = "O" });
                        }
                    }
                }
                else // down
                {
                    if (price <= low - boxSize)
                    {
                        while (price <= low - boxSize)
                        {
                            low -= boxSize;
                            int row = (int)((high - low) / boxSize);
                            pfPoints.Add(new PointFigurePoint { Row = row, Column = column, Type = "O" });
                        }
                    }
                    else if (price >= low + boxSize * reversal)
                    {
                        direction = "up";
                        high = low + boxSize * reversal;
                        column++;
                        while (price >= high + boxSize)
                        {
                            high += boxSize;
                            int row = (int)((high - low) / boxSize);
                            pfPoints.Add(new PointFigurePoint { Row = row, Column = column, Type = "X" });
                        }
                    }
                }
            }
            return pfPoints;
        }
    }

    public class StockData
    {
        public DateTime Date { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
    }

    public class PointFigurePoint
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public string Type { get; set; } // "X" or "O"
    }
}