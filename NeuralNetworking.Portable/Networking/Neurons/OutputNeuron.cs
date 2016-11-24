namespace NeuralNetworking.Networking.Neurons
{
    internal class OutputNeuron : CalculateableNeuronBase, IOutputNeuron
    {
        public OutputNeuron() : base(ActivationFunction.HyperbolicTangent)
        {
        }


        public double GetOutputValue()
        {
            return Value;
        }
    }
}