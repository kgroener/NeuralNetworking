using Simulation.Game;
using Simulation.Game.Ship;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Simulation
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        ObservableCollection<GeneticResult> results;

        public MainPage()
        {
            this.InitializeComponent();

            Task.Run(async () => await StartShipsAsync());
        }

        private async Task StartShipsAsync()
        {
            var enhancer = new ShipEnhancer();

            results = new ObservableCollection<GeneticResult>();
            results.Add(new GeneticResult() { Generation = 0, Fitness = 0 });

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
            Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                (_lineChart.Series[0] as LineSeries).ItemsSource = results;
            }).AsTask();

            IEnumerable<Ship> generation = Enumerable.Empty<Ship>();

            for (int i = 0; i < 1000000; i++)
            {
                await ClearCanvasAsync();
                await DrawTargetsAsync();

                var champions = enhancer.GetTopSelection(generation, 10);
                generation = enhancer.CreateNewGeneration(champions, 50);

                if (champions.Any())
                {
                    await AddFitnessResultToChartAsync(enhancer, i, champions);
                }

                await DrawShipsAsync(generation);

                await RunSimulationAsync(generation);
            }

            var winner = generation.First();
        }

        private async Task AddFitnessResultToChartAsync(ShipEnhancer enhancer, int i, IEnumerable<Ship> champions)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
            Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                results.Add(new GeneticResult() { Generation = i, Fitness = enhancer.CalculateFitness(champions.First()) });
            }).AsTask();
        }

        private static async Task RunSimulationAsync(IEnumerable<Ship> generation)
        {
            List<Task> tasks = new List<Task>();
            foreach (var enhanceable in generation)
            {
                tasks.Add(enhanceable.RunAsync());
            }

            await Task.WhenAll(tasks);
        }

        private async Task ClearCanvasAsync()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
            Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                _canvas.Children.Clear();
            });
        }

        private async Task DrawShipsAsync(IEnumerable<Ship> generation)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
            Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                bool isFirst = true;
                foreach (var ship in generation)
                {
                    var shipView = new Polygon();
                    shipView.Points.Add(new Windows.Foundation.Point(-5, 0));
                    shipView.Points.Add(new Windows.Foundation.Point(5, 0));
                    shipView.Points.Add(new Windows.Foundation.Point(0, 5));
                    shipView.Fill = isFirst ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Green);

                    isFirst = false;

                    _canvas.Children.Add(shipView);

                    ship.Updated += async (o, e) =>
                    {
                        try
                        {
                            await UpdateShipPositionAsync(o, ship, shipView);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                    };

                }
            });
        }

        private static async Task UpdateShipPositionAsync(object o, Ship ship, Polygon shipView)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
            Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                var s = o as Ship;
                Canvas.SetLeft(shipView, ship.Position.X);
                Canvas.SetTop(shipView, ship.Position.Y);
            });
        }

        private async Task DrawTargetsAsync()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
            Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
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
            });
        }
    }

    internal class GeneticResult
    {
        public int Generation { get; set; }
        public double Fitness { get; set; }
    }
}
