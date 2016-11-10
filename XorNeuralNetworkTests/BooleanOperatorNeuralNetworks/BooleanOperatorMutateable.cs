using GeneticNeuralNetworking.Genetics;
using NeuralNetworking.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XorNeuralNetworkTests
{
    public abstract class BooleanOperatorMutateable<T> : NeuralNetworkMutateable<T> where T : BooleanOperatorMutateable<T>
    {
        private Func<bool, bool, bool> _booleanOperator;

        public BooleanOperatorMutateable(Func<bool, bool, bool> booleanOperator) : base(new NeuralNetwork(2, 1))
        {
            _booleanOperator = booleanOperator;
            RunResults = new Dictionary<bool, Dictionary<bool, double>>();
            RunResults[false] = new Dictionary<bool, double>();
            RunResults[true] = new Dictionary<bool, double>();
        }

        internal BooleanOperatorMutateable(Func<bool, bool, bool> booleanOperator, NeuralNetwork network) : base(network)
        {
            _booleanOperator = booleanOperator;
            RunResults = new Dictionary<bool, Dictionary<bool, double>>();
            RunResults[false] = new Dictionary<bool, double>();
            RunResults[true] = new Dictionary<bool, double>();
        }

        public Dictionary<bool, Dictionary<bool, double>> RunResults { get; }

        public override Task RunAsync()
        {
            return Task.Run(() =>
            {

                var inputNeurons = NeuralNetwork.GetNeurons(NeuronType.Input);
                var inputNeuronA = inputNeurons.ElementAt(0);
                var inputNeuronB = inputNeurons.ElementAt(1);
                var outputNeuron = NeuralNetwork.GetNeurons(NeuronType.Output).Single();

                foreach (var inputA in new[] { false, true })
                {
                    foreach (var inputB in new[] { false, true })
                    {
                        NeuralNetwork.SetNeuronInput(inputNeuronA, inputA);
                        NeuralNetwork.SetNeuronInput(inputNeuronB, inputB);

                        NeuralNetwork.UpdateNeuronValues(10);
                        double result = NeuralNetwork.GetNeuronOutput(outputNeuron);

                        RunResults[inputA][inputB] = result;

                        NeuralNetwork.ClearNeuronValues();
                    }
                }
            });

        }
    }
}
