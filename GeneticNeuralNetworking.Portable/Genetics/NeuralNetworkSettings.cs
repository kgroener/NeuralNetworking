namespace GeneticNeuralNetworking.Genetics
{
    public class NeuralNetworkSettings
    {
        public NeuralNetworkSettings(int inputs, int outputs)
        {
            NumberOfInputs = inputs;
            NumberOfOutputs = outputs;
        }

        public int NumberOfInputs { get; }
        public int NumberOfOutputs { get; }
    }
}