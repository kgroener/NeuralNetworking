using OxyPlot;
using Simulation.Game;
using Simulation.Game.Ship;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Simulation.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<DataPoint> resultsSpecies1;
        private ObservableCollection<DataPoint> resultsSpecies2;
        private int _requiredUpdateCount;
        private double _maxFitness;
        private int _plateauSize;

        public MainWindow()
        {
            InitializeComponent();
            Task.Run(async () => await StartShipsAsync());
        }

        private async Task StartShipsAsync()
        {
            var enhancer = new ShipEnhancer();

            resultsSpecies1 = new ObservableCollection<DataPoint>();
            resultsSpecies2 = new ObservableCollection<DataPoint>();

            UIHelper.UISafeInvoke(() =>
            {
                _lineChartSpecies1.ItemsSource = resultsSpecies1;
                _lineChartSpecies2.ItemsSource = resultsSpecies2;
            });

            IEnumerable<Ship> generationSpecies1 = Enumerable.Empty<Ship>();
            IEnumerable<Ship> generationSpecies2 = Enumerable.Empty<Ship>();

            _maxFitness = double.MinValue;

            for (int i = 0; i < 1000000; i++)
            {
                var aggr = (_plateauSize / (i + 1d)) + (0.5 / (i + 1));
                enhancer.Aggressiveness = aggr;
                Console.WriteLine(aggr);

                var currentMaxFitness = double.MinValue;
                if (generationSpecies1.Any())
                {
                    var fitness = enhancer.GetBestFitness(generationSpecies1);
                    if (fitness > currentMaxFitness)
                    {
                        currentMaxFitness = fitness;
                    }

                    UIHelper.UISafeInvoke(() =>
                    {
                        resultsSpecies1.Add(new DataPoint(i, fitness));
                        _plot.InvalidatePlot();
                    });
                }
                if (generationSpecies2.Any())
                {
                    var fitness = enhancer.GetBestFitness(generationSpecies2);
                    if (fitness > currentMaxFitness)
                    {
                        currentMaxFitness = fitness;
                    }

                    UIHelper.UISafeInvoke(() =>
                    {
                        resultsSpecies2.Add(new DataPoint(i, fitness));
                        _plot.InvalidatePlot();
                    });
                }

                if (currentMaxFitness > _maxFitness)
                {
                    _plateauSize = 0;
                    _maxFitness = currentMaxFitness;
                }
                else
                {
                    _plateauSize++;
                }



                generationSpecies1 = enhancer.CreateNewGeneration(generationSpecies1, 100);
                generationSpecies2 = enhancer.CreateNewGeneration(generationSpecies2, 100);

                await RunSimulationAsync(generationSpecies1, generationSpecies2);
            }
        }

        private async Task RunSimulationAsync(IEnumerable<Ship> generationSpecies1, IEnumerable<Ship> generationSpecies2)
        {
            foreach (var enhanceable in generationSpecies1.Concat(generationSpecies2))
            {
                enhanceable.Initialize();
            }

            int updateCount = 0;
            DateTime start = DateTime.Now;
            var requiredUpdateCount = 500;
            var maxTargetsReached = 0;
            while (updateCount++ < requiredUpdateCount)
            {
                await Task.Delay(10);

                DateTime end = DateTime.Now;
                var updateTime = end - start;
                start = end;

                Update(generationSpecies1, updateTime);
                Update(generationSpecies2, updateTime);

                ClearCanvas();
                Draw(generationSpecies1, Colors.Blue, Colors.DarkOrange);
                Draw(generationSpecies2, Colors.Green, Colors.Red);

                var newMaxTargetsReached = Math.Max(generationSpecies1.Max(s => s.TargetsReached), generationSpecies2.Max(s => s.TargetsReached));
                if (newMaxTargetsReached > maxTargetsReached)
                {
                    maxTargetsReached = newMaxTargetsReached;
                    requiredUpdateCount += (300 / newMaxTargetsReached);
                    Console.WriteLine($"Required update count increased toL {requiredUpdateCount}");
                }
            }
        }

        private void ClearCanvas()
        {
            UIHelper.UISafeInvoke(() =>
            {
                _canvas.Children.Clear();
            });
        }

        private static void Update(IEnumerable<Ship> generation, TimeSpan updateTime)
        {
            foreach (var enhanceable in generation)
            {
                enhanceable.Update(updateTime);
            }
        }

        private void Draw(IEnumerable<Ship> generation, Color defaultColor, Color winnerColor)
        {
            UIHelper.UISafeInvoke(() =>
            {
                bool isFirst = true;

                foreach (var target in World.Targets)
                {
                    var targetView = new Ellipse()
                    {
                        Width = 10,
                        Height = 10,
                        Fill = new SolidColorBrush(Colors.Black)
                    };

                    Canvas.SetLeft(targetView, target.X);
                    Canvas.SetTop(targetView, target.Y);

                    _canvas.Children.Add(targetView);
                }

                foreach (var ship in generation)
                {
                    var shipView = new Polygon();
                    shipView.Points.Add(new Point(-5, 0));
                    shipView.Points.Add(new Point(5, 0));
                    shipView.Points.Add(new Point(0, 5));
                    shipView.Fill = isFirst ? new SolidColorBrush(winnerColor) : new SolidColorBrush(defaultColor);
                    isFirst = false;

                    Canvas.SetLeft(shipView, ship.Position.X);
                    Canvas.SetTop(shipView, ship.Position.Y);

                    _canvas.Children.Add(shipView);
                }
            });
        }
    }

    internal class GeneticResult
    {
        public int Generation { get; set; }
        public double Fitness { get; set; }
    }
}
