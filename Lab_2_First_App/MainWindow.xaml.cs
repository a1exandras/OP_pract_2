using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Lab_2_First_App
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static int PopulationCount = 5;
        static int PointCount = 10;
        static double MutationProb = 0.5;
        static int IterationCount = 1000;

        Random rnd = new Random();
        static int Radius = 20;
        static Polygon myPolygon = new Polygon();
        static List<Ellipse> EllipseArray = new List<Ellipse>();
        static PointCollection pC = new PointCollection();

        public MainWindow()
        {

            InitializeComponent();
            InitPoints();
            InitPolygon();

        }

        private void mainProgram()
        {
            int[][] ways = new int[2 * PopulationCount][];

            //make array with non-reappearing numbers
            int[] arr = new int[PointCount];
            for (int i = 0; i < PointCount; i++)
                arr[i] = i + 1;

            for (int i = 0; i < PopulationCount; i++)
            {
                ways[i] = randomizeArray(arr);
            }

            double[] Dists = new double[2 * PopulationCount];

            for (int j = 0; j < IterationCount; j++)
            {
                for (int k = 0; k < PopulationCount; k++)
                    genChildren(ways, k);

                Dispatcher.Invoke((Action)(() =>
                {
                    for (int k = 0; k < 2 * PopulationCount; k++)
                        Dists[k] = calcDistance(ways[k]);
                }));
                
                Array.Sort(Dists, ways);

                Dispatcher.Invoke((Action)(() =>
                { 
                    IterCounterLabel.Content = j.ToString(); 
                }));

                Dispatcher.Invoke((Action)(() =>
                {
                    OneStep(ways[0]);
                }));
                Thread.Sleep(10);
            }
        }

        private double calcDistance(int[] way)
        {
            double s = 0;
            int n = way.Length;

            for (int i = 1; i < n; i++)
            {
                s += Math.Sqrt(Math.Pow(pC[way[i] - 1].X - pC[way[i - 1] - 1].X, 2) +
                               Math.Pow(pC[way[i] - 1].Y - pC[way[i - 1] - 1].Y, 2));
            }
            s += Math.Sqrt(Math.Pow(pC[way[n - 1] - 1].X - pC[way[0] - 1].X, 2) +
                           Math.Pow(pC[way[n - 1] - 1].Y - pC[way[0] - 1].Y, 2));
            return s;
        }

        private void genChildren(int[][] ways, int iter)
        {
            int Ind1 = rnd.Next(PopulationCount);
            int Ind2 = rnd.Next(PopulationCount);
            int breakPoint = rnd.Next(PointCount);

            HashSet<int> firstSh = new HashSet<int>();
            HashSet<int> secondSh = new HashSet<int>();
            
            for(int i = 0; i < breakPoint; i++)//first part
            {
                firstSh.Add(ways[Ind1][i]);
                secondSh.Add(ways[Ind2][i]);
            }

            for (int i = breakPoint; i < PointCount; i++)//second part
            {
                firstSh.Add(ways[Ind2][i]);
                secondSh.Add(ways[Ind1][i]);
            }

            for (int i = 0; i < breakPoint; i++)//polishing w\first part
            {
                firstSh.Add(ways[Ind2][i]);
                secondSh.Add(ways[Ind1][i]);
            }

            for (int i = breakPoint; i < PointCount; i++)//polishing w\second part
            {
                firstSh.Add(ways[Ind1][i]);
                secondSh.Add(ways[Ind2][i]);
            }

            if(rnd.Next(2) == 0)
                ways[iter + PopulationCount] = firstSh.ToArray();
            else
                ways[iter + PopulationCount] = secondSh.ToArray();

            if (rnd.NextDouble() < MutationProb)
            {
                int mut1 = rnd.Next(PointCount);
                int mut2 = rnd.Next(PointCount);

                if (mut1 < mut2)
                {
                    int[] temp = new int[mut2 - mut1];

                    for (int j = mut1; j < mut2; j++)
                    {
                        temp[j - mut1] = ways[iter + PopulationCount][j];
                    }

                    for (int j = mut1; j < mut2; j++)
                    {
                        ways[iter + PopulationCount][j] = temp[mut2 - 1 - j];
                    }
                }
                else if (mut2 < mut1)
                {
                    int[] temp = new int[mut1 - mut2];

                    for (int j = mut2; j < mut1; j++)
                    {
                        temp[j - mut2] = ways[iter + PopulationCount][j];
                    }

                    for (int j = mut2; j < mut1; j++)
                    {
                        ways[iter + PopulationCount][j] = temp[mut1 - 1 - j];
                    }
                }
            }
        }

        private void InitPoints()
        {
            Random rnd = new Random();
            pC.Clear();
            EllipseArray.Clear();

            for (int i = 0; i < PointCount; i++)
            {
                Point p = new Point();

                p.X = rnd.Next(Radius, (int)(0.75*MainWin.Width)-3*Radius);
                p.Y = rnd.Next(Radius, (int)(0.90*MainWin.Height-3*Radius));                
                pC.Add(p);
            }

            for (int i = 0; i < PointCount; i++)
            { 
                Ellipse el = new Ellipse();

                el.StrokeThickness = 2;
                el.Height = el.Width = Radius;
                el.Stroke = Brushes.Black;
                el.Fill = Brushes.Red;
                EllipseArray.Add(el); 
            }            
        }

        private void InitPolygon()
        {
            myPolygon.Stroke = System.Windows.Media.Brushes.Black;            
            myPolygon.StrokeThickness = 2;            
        }

        private void PlotPoints()
        {            
            for (int i=0; i<PointCount; i++)
            {
                Canvas.SetLeft(EllipseArray[i], pC[i].X - Radius/2);
                Canvas.SetTop(EllipseArray[i], pC[i].Y - Radius/2);
                MyCanvas.Children.Add(EllipseArray[i]);
            }
        }

        private void PlotWay(int[] BestWayIndex)
        {
            PointCollection Points = new PointCollection();

            for (int i = 0; i < BestWayIndex.Length; i++)
                Points.Add(pC[BestWayIndex[i] - 1]);

            myPolygon.Points = Points;
            MyCanvas.Children.Add(myPolygon);
        }

        public async void StopStart_Click(object sender, RoutedEventArgs e)
        {
            NumElemCB.IsEnabled = false;
            NumIterCB.IsEnabled = false;
            MutProbCB.IsEnabled = false;
            PopulCountCB.IsEnabled = false;
            StopStart.IsEnabled = false;

            await Task.Run(() => mainProgram());

            NumElemCB.IsEnabled = true;
            NumIterCB.IsEnabled = true;
            MutProbCB.IsEnabled = true;
            PopulCountCB.IsEnabled = true;
            StopStart.IsEnabled = true;
        }

        private void OneStep(int[] BestWay)
        {
            MyCanvas.Children.Clear();
            PlotPoints();
            PlotWay(BestWay);
        }

        private void NumElemCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox CB = (ComboBox)e.Source;
            ListBoxItem item = (ListBoxItem)CB.SelectedItem;

            PointCount = Convert.ToInt32(item.Content);
            InitPoints();
            InitPolygon();
        }
        
        private void NumIterCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox CB = (ComboBox)e.Source;
            ListBoxItem item = (ListBoxItem)CB.SelectedItem;

            IterationCount = Convert.ToInt32(item.Content);
            InitPoints();
            InitPolygon();
        }
        
        private void MutProbCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox CB = (ComboBox)e.Source;
            ListBoxItem item = (ListBoxItem)CB.SelectedItem;

            MutationProb = Convert.ToDouble(item.Content);
            InitPoints();
            InitPolygon();
        }

        private int[] randomizeArray(int[] arr)
        {
            int[] randomizedArr = arr.OrderBy(x => rnd.Next()).ToArray();
            return randomizedArr;
        }

        private void PopulCountCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox CB = (ComboBox)e.Source;
            ListBoxItem item = (ListBoxItem)CB.SelectedItem;

            PopulationCount = Convert.ToInt32(item.Content);
            InitPoints();
            InitPolygon();
        }
    }
}