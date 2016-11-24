namespace NeuralNetworking.Networking.Neurons
{
    internal class InputNeuron : IInputNeuron
    {
        public InputNeuron()
        {

        }

        public double Value { get; private set; }

        public void Reset()
        {
            SetInputValue(0);
        }

        public void SetInputValue(double value)
        {
            Value = value;
        }
    }
}
