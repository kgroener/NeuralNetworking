using System;

namespace NeuralNetworking.Networking.Neurons
{
    public abstract class CalculateableNeuronBase : ICalculateableNeuron
    {
        public CalculateableNeuronBase(ActivationFunction activationFunction)
        {
            ActivationFunction = activationFunction;
        }

        public ActivationFunction ActivationFunction { get; private set; }

        public double Value { get; private set; }

        public void CalculateValue(double accumulativeSynapseInput)
        {
            Value = CalculateValue(ActivationFunction, accumulativeSynapseInput);
        }

        private static double CalculateValue(ActivationFunction activationFunction, double accumulativeSynapseInput)
        {
            switch (activationFunction)
            {
                case ActivationFunction.Lineair:
                    return accumulativeSynapseInput;
                case ActivationFunction.LineairTruncated:
                    return (accumulativeSynapseInput >= 1) ? 1 : (accumulativeSynapseInput <= -1) ? -1 : accumulativeSynapseInput;
                case ActivationFunction.LineairTruncatedAtZero:
                    return (accumulativeSynapseInput >= 1) ? 1 : (accumulativeSynapseInput <= 0) ? 0 : accumulativeSynapseInput;
                case ActivationFunction.Binairy:
                    return (accumulativeSynapseInput >= 0.5) ? 1 : 0;
                case ActivationFunction.PositiveNegativeBinairy:
                    return (accumulativeSynapseInput >= 0) ? 1 : -1;
                case ActivationFunction.Sigmoid:
                    return (1 / (1 + Math.Exp(-accumulativeSynapseInput)));
                case ActivationFunction.HyperbolicTangent:
                    return Math.Tanh(accumulativeSynapseInput);
                default:
                    throw new NotImplementedException($"Synapse function {activationFunction} is not implemented.");
            }
        }

        public void Reset()
        {
            Value = 0;
        }

        public void ChangeActivationFunction(ActivationFunction newActivationFunction)
        {
            ActivationFunction = newActivationFunction;
        }
    }
}
