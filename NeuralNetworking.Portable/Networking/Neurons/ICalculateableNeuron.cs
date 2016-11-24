namespace NeuralNetworking.Networking.Neurons
{
    public interface ICalculateableNeuron : INeuron
    {
        ActivationFunction ActivationFunction { get; }

        void CalculateValue(double input);

        void ChangeActivationFunction(ActivationFunction newActivationFunction);
    }
}
