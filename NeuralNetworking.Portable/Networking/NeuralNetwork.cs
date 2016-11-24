using Extensions;
using NeuralNetworking.Networking.Neurons;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NeuralNetworking.Networking
{
    public sealed class NeuralNetwork
    {
        private HashSet<INeuron> _neurons;
        private readonly int _numberOfInputs;
        private readonly int _numberOfOutputs;
        private Dictionary<INeuron, Dictionary<INeuron, double>> _synapseWeights;

        public NeuralNetwork(int numberOfInputs, int numberOfOutputs)
        {
            _numberOfInputs = numberOfInputs;
            _numberOfOutputs = numberOfOutputs;
            _neurons = new HashSet<INeuron>();
            _synapseWeights = new Dictionary<INeuron, Dictionary<INeuron, double>>();

            for (int i = 0; i < numberOfInputs; i++)
            {
                _neurons.Add(new InputNeuron());
            }
            for (int o = 0; o < numberOfOutputs; o++)
            {
                _neurons.Add(new OutputNeuron());
            }
        }

        public INeuron AddConstantNeuron(double constantValue)
        {
            var constantNeuron = new ConstantNeuron(constantValue);
            _neurons.Add(constantNeuron);
            return constantNeuron;
        }

        public INeuron AddHiddenNeuron(ActivationFunction activationFunction)
        {
            var hiddenNeuron = new HiddenNeuron(activationFunction);
            _neurons.Add(hiddenNeuron);
            return hiddenNeuron;
        }

        public INeuron AddNeuronCopy(INeuron neuronToClone)
        {
            HiddenNeuron hn = neuronToClone as HiddenNeuron;
            if (hn != null)
            {
                return AddHiddenNeuron(hn.ActivationFunction);
            }

            ConstantNeuron cn = neuronToClone as ConstantNeuron;
            if (cn != null)
            {
                return AddConstantNeuron(cn.Value);
            }

            throw new ArgumentException("Neuron to clone is not a valid neuron type to clone");
        }

        public void RemoveNeuron(INeuron neuron)
        {
            if (!_neurons.Remove(neuron))
            {
                throw new ArgumentException($"Neuron did not exist in this neural network, and thus could not be removed");
            }

            _synapseWeights.Remove(neuron);
            foreach (var connection in _synapseWeights)
            {
                connection.Value.Remove(neuron);
            }
        }

        public IEnumerable<INeuron> GetAllNeurons()
        {
            return _neurons.ToArray();
        }

        public IEnumerable<IInputNeuron> GetInputs()
        {
            return _neurons.OfType<IInputNeuron>().ToArray();
        }

        public IEnumerable<IOutputNeuron> GetOutputs()
        {
            return _neurons.OfType<IOutputNeuron>().ToArray();
        }

        public IEnumerable<INeuron> GetInsideNeurons()
        {
            return _neurons.Where(n => !(n is IInputNeuron) && !(n is IOutputNeuron)).ToArray();
        }

        public double GetSynapseWeight(INeuron from, INeuron to)
        {
            if (!_neurons.Contains(from))
            {
                throw new ArgumentException("Neuron 'from' is not a neuron from this neural network");
            }
            if (!_neurons.Contains(to))
            {
                throw new ArgumentException("Neuron 'to' is not a neuron from this neural network");
            }

            double weight = 0;
            if (_synapseWeights.ContainsKey(from) && _synapseWeights[from].ContainsKey(to))
            {
                weight = _synapseWeights[from][to];
            }

            return weight;
        }
        public void SetSynapseWeight(INeuron from, INeuron to, double weight)
        {
            if (!_neurons.Contains(from))
            {
                throw new ArgumentException("Neuron 'from' is not a neuron from this neural network");
            }
            if (!_neurons.Contains(to))
            {
                throw new ArgumentException("Neuron 'to' is not a neuron from this neural network");
            }
            if (double.IsNaN(weight))
            {
                throw new ArgumentException("Weight is not a number (NaN)");
            }
            if (to is ISupplierNeuron)
            {
                throw new ArgumentException("Neuron 'to' is a supplier neuron, cannot create a synapse to this type of neuron");
            }

            if (!_synapseWeights.ContainsKey(from))
            {
                _synapseWeights[from] = new Dictionary<INeuron, double>();
            }

            if (weight == 0)
            {
                _synapseWeights[from].Remove(to);
            }
            else
            {
                _synapseWeights[from][to] = weight;
            }
        }

        public TimeSpan UpdateNeuronValues(int cycles = 1)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < cycles; i++)
            {
                var currentNeuronValues = _neurons.ToDictionary((n) => n, (n) => n.Value);
                var accumulativeNewNeuronValues = _neurons.ToDictionary((n) => n, (n) => 0d);

                //calculate accumulative values from synapses
                foreach (var synapseBegin in _synapseWeights)
                {
                    var neuronFrom = synapseBegin.Key;

                    foreach (var synapseEnd in synapseBegin.Value)
                    {
                        var neuronTo = synapseEnd.Key;
                        var synapseWeight = synapseEnd.Value;

                        accumulativeNewNeuronValues[neuronTo] += currentNeuronValues[neuronFrom] * synapseWeight;
                    }
                }

                foreach (var cn in _neurons.OfType<ICalculateableNeuron>())
                {
                    cn.CalculateValue(accumulativeNewNeuronValues[cn]);
                }
            }

            sw.Stop();
            return sw.Elapsed;
        }

        public NeuralNetwork Clone()
        {
            //Create clone with same amount of inputs and outputs
            var networkClone = new NeuralNetwork(_numberOfInputs, _numberOfOutputs);

            //Create mapping for current neurons and cloned neurons 
            Dictionary<INeuron, INeuron> neuronMapping = new Dictionary<INeuron, INeuron>();
            GetInputs().MapAction(networkClone.GetInputs(), (a, b) =>
            {
                neuronMapping[a] = b;
            });

            GetOutputs().MapAction(networkClone.GetOutputs(), (a, b) =>
            {
                neuronMapping[a] = b;
            });

            // clone hidden and constant neurons
            var hiddenNeurons = _neurons.OfType<HiddenNeuron>();
            foreach (var hiddenNeuron in hiddenNeurons)
            {
                neuronMapping[hiddenNeuron] = networkClone.AddHiddenNeuron(hiddenNeuron.ActivationFunction);
            }

            var constantNeurons = _neurons.OfType<ConstantNeuron>();
            foreach (var constantNeuron in constantNeurons)
            {
                neuronMapping[constantNeuron] = networkClone.AddConstantNeuron(constantNeuron.Value);
            }


            var currentNeurons = GetAllNeurons();
#if DEBUG
            var cloneNeurons = networkClone.GetAllNeurons();
            if (currentNeurons.Count() != cloneNeurons.Count())
            {
                throw new InvalidOperationException("Clone failed");
            }
#endif

            //Clone synapses
            foreach (var neuronFrom in currentNeurons)
            {
                foreach (var neuronTo in currentNeurons)
                {
                    if (!(neuronTo is ISupplierNeuron))
                    {
                        networkClone.SetSynapseWeight(neuronMapping[neuronFrom], neuronMapping[neuronTo], GetSynapseWeight(neuronFrom, neuronTo));
                    }
                }
            }

            return networkClone;
        }

        public void Reset()
        {
            foreach (var neuron in _neurons)
            {
                neuron.Reset();
            }
        }
    }
}
