using GeneticNeuralNetworking.Genetics;
using Simulation.Genetics.Enhancing;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace XorNeuralNetworkTests
{


    internal class XorOperatorNeuralNetworkEnhancer : GeneticEnhancer<XorOperatorMutateable>
    {
        public XorOperatorNeuralNetworkEnhancer() : base(new NeuralNetworkMutator<XorOperatorMutateable>(new NeuralNetworkSettings(2, 1)))
        {
        }

        protected override IEnumerable<FitnessFunction<XorOperatorMutateable>> FitnessFunctions
        {
            get
            {
                return new FitnessFunction<XorOperatorMutateable>[]
                {
                    (n) => {
                        double result = n.RunResults[false][false];
                        result = Clip(result);

                        return new FitnessResult(1-result);
                    },
                    (n) => {
                        double result = n.RunResults[true][false];
                        result = Clip(result);

                        return new FitnessResult(result);
                    },
                    (n) => {
                        double result = n.RunResults[false][true];
                        result = Clip(result);

                        return new FitnessResult(result);
                    },
                    (n) => {
                        double result = n.RunResults[true][true];
                        result = Clip(result);

                        return new FitnessResult(1-result);
                    },
                    (n) =>
                    {
                        double result = Math.Abs(n.RunResults.SelectMany(a => a.Value).Sum(a => Math.Abs(a.Value)));

                        return new FitnessResult(result > 2 ? -0.5 : result/5);
                    },
                    (n) =>
                    {
                        double result = -n.NeuralNetwork.GetAllNeurons().Count()/5;

                        return new FitnessResult(result);
                    }
                };
            }
        }

        private static double Clip(double result)
        {
            return result > 1 ? 1 : result < 0 ? 0 : result;
        }
    }

    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            var enhancer = new XorOperatorNeuralNetworkEnhancer();

            IEnumerable<XorOperatorMutateable> generation = Enumerable.Empty<XorOperatorMutateable>();

            for (int i = 0; i < 1000; i++)
            {
                var champions = enhancer.GetTopSelection(generation, 10);

                Debug.WriteLine($"Generation {i}:");

                generation = enhancer.CreateNewGeneration(champions, 100);

                List<Task> tasks = new List<Task>();
                foreach (var enhanceable in generation)
                {
                    tasks.Add(enhanceable.RunAsync());
                }

                await Task.WhenAll(tasks);
            }

            var winners = enhancer.GetTopSelection(generation, 3);

        }
    }
}
