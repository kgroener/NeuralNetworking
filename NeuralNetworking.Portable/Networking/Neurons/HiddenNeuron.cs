using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetworking.Networking.Neurons
{
    internal class HiddenNeuron : CalculateableNeuronBase
    {
        public HiddenNeuron(ActivationFunction activationFunction) : base(activationFunction)
        {
        }
    }
}
