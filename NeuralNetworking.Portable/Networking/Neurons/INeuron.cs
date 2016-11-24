using NeuralNetworking.Networking;
using NeuralNetworking.Networking.Neurons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetworking.Networking.Neurons
{
    public interface INeuron
    {
        double Value { get; }

        void Reset();
    }
}
