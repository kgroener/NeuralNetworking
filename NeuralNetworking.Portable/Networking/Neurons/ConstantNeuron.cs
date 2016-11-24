using System;

namespace NeuralNetworking.Networking.Neurons
{
    internal class ConstantNeuron : ISupplierNeuron
    {
        public ConstantNeuron(double constantValue)
        {
            Value = constantValue;
        }

        public double Value { get; }

        public void Reset()
        {
            
        }
    }
}