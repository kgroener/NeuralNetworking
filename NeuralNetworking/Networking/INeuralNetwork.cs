using System;
using System.Collections.Generic;

namespace Genometry.NeuralNetworking.Networking
{
    public interface INeuralNetwork<T> where T : INeuralNetwork<T>
    {
        NeuronKey AddNeuron(ActivationFunction activationFunction = ActivationFunction.Lineair);
        void RemoveNeuron(NeuronKey neuron);

        void SetNeuronInput(NeuronKey inputNeuron, double value);
        void SetNeuronInput(NeuronKey inputNeuron, bool value);
        double GetNeuronOutput(NeuronKey outputNeuron);

        void SetNeuronActivationFunction(NeuronKey neuron, ActivationFunction activationFunction);
        ActivationFunction GetNeuronActivationFunction(NeuronKey neuron);

        void SetSynapseWeight(NeuronKey from, NeuronKey to, double weight);
        double GetSynapseWeight(NeuronKey from, NeuronKey to);

        IEnumerable<NeuronKey> GetNeurons(NeuronType neuronType);
        IEnumerable<NeuronKey> GetAllNeurons();

        TimeSpan UpdateNeuronValues(int cycles = 1);

        T Clone();
    }
}
