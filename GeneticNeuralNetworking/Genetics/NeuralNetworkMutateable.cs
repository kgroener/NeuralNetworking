using NeuralNetworking.Networking;
using System;

namespace GeneticNeuralNetworking.Genetics
{
    public abstract class NeuralNetworkMutateable<T> where T : NeuralNetworkMutateable<T>
    {
        public NeuralNetworkMutateable(NeuralNetwork neuralNetwork)
        {
            NeuralNetwork = neuralNetwork;
        }
        public NeuralNetwork NeuralNetwork { get; internal set; }

        public abstract T Clone();

        public abstract void Update(TimeSpan lastUpdateDuration);
    }
}
