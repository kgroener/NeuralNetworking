using Simulation.Game;
using Simulation.Game.Ship;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                var champions = enhancer.GetTopSelection(generation, 10);
                generation = enhancer.CreateNewGeneration(champions, 100);

                if (champions.Any())
                {
                    await AddFitnessResultToChartAsync(enhancer, i, champions);
                }

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

        private async Task RunSimulationAsync(IEnumerable<Ship> generation)
        {
            foreach (var enhanceable in generation)
            {
                enhanceable.Initialize();
            }

            int updateCount = 0;
            DateTime start = DateTime.Now;
            while (updateCount++ < 1000)
            {
                await Task.Delay(10);
                
                DateTime end = DateTime.Now;
                var updateTime = end - start;
                start = end;

                Update(generation, updateTime);

                await Draw(generation);
            }
        }

        private static void Update(IEnumerable<Ship> generation, TimeSpan updateTime)
        {
            foreach (var enhanceable in generation)
            {
                enhanceable.Update(updateTime);
            }
        }

        private async Task Draw(IEnumerable<Ship> generation)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                            Windows.UI.Core.CoreDispatcherPriority.High, () =>
                            {
                                bool isFirst = true;

                                _canvas.Children.Clear();

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
                                    shipView.Points.Add(new Windows.Foundation.Point(-5, 0));
                                    shipView.Points.Add(new Windows.Foundation.Point(5, 0));
                                    shipView.Points.Add(new Windows.Foundation.Point(0, 5));
                                    shipView.Fill = isFirst ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Green);

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
