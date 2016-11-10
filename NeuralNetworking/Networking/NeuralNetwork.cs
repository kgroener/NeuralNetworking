using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Simulation.NeuralNetworking.Networking
{
    public sealed class NeuralNetwork : INeuralNetwork<NeuralNetwork>
    {
        private Dictionary<ulong, Dictionary<ulong, double>> _synapseWeights;
        private Dictionary<ulong, double> _neuronValues;
        private Dictionary<NeuronType, HashSet<NeuronKey>> _neurons;
        private Dictionary<ulong, ActivationFunction> _neuronActivationFunctions;
        private NeuronKeyGenerator _neuronKeyGenerator;

        public NeuralNetwork(int numberOfInputs, int numberOfOutputs)
        {
            _synapseWeights = new Dictionary<ulong, Dictionary<ulong, double>>();
            _neuronValues = new Dictionary<ulong, double>();
            _neuronActivationFunctions = new Dictionary<ulong, ActivationFunction>();
            _neurons = new Dictionary<NeuronType, HashSet<NeuronKey>>();
            _neurons.Add(NeuronType.Normal, new HashSet<NeuronKey>());
            _neurons.Add(NeuronType.Input, new HashSet<NeuronKey>());
            _neurons.Add(NeuronType.Output, new HashSet<NeuronKey>());
            _neuronKeyGenerator = new NeuronKeyGenerator();

            for (int i = 0; i < numberOfInputs; ++i)
            {
                AddNeuron(NeuronType.Input, ActivationFunction.Lineair);
            }
            for (int i = 0; i < numberOfOutputs; ++i)
            {
                AddNeuron(NeuronType.Output, ActivationFunction.Lineair);
            }
        }

        public NeuronKey AddNeuron(ActivationFunction activationFunction)
        {
            return AddNeuron(NeuronType.Normal, activationFunction);
        }

        private NeuronKey AddNeuron(NeuronType neuronType, ActivationFunction activationFunction)
        {
            NeuronKey newKey = _neuronKeyGenerator.Create();

            _neuronValues[newKey.Key] = 0;
            _synapseWeights.Add(newKey.Key, new Dictionary<ulong, double>());
            _neuronActivationFunctions[newKey.Key] = activationFunction;
            _neurons[neuronType].Add(newKey);

            return newKey;
        }

        public void ClearNeuronValues()
        {
            foreach(var key in _neuronValues.Keys.ToArray())
            {
                _neuronValues[key] = 0;
            }
        }

        public void RemoveNeuron(NeuronKey neuron)
        {
            if (!_neurons[NeuronType.Normal].Any(n => n.Key == neuron.Key))
            {
                throw new ArgumentException("Neuron is not removeable (Input/Output) or is not found");
            }
            _neurons[NeuronType.Normal].Remove(neuron);
            _neuronValues.Remove(neuron.Key);
            foreach (var synapse in _synapseWeights)
            {
                synapse.Value.Remove(neuron.Key);
            }
            _synapseWeights.Remove(neuron.Key);
        }

        public void SetNeuronInput(NeuronKey inputNeuron, double value)
        {
            if (!_neurons[NeuronType.Input].Contains(inputNeuron))
            {
                throw new ArgumentException("Supplied neuron key is not an input neuron");
            }

            _neuronValues[inputNeuron.Key] = value;
        }

        public void SetNeuronInput(NeuronKey inputNeuron, bool value)
        {
            SetNeuronInput(inputNeuron, value ? 1 : 0);
        }

        public double GetNeuronOutput(NeuronKey outputNeuron)
        {
            if (!_neurons[NeuronType.Output].Contains(outputNeuron))
            {
                throw new ArgumentException("Supplied neuron key is not an output neuron");
            }

            return _neuronValues[outputNeuron.Key];
        }

        public void SetNeuronActivationFunction(NeuronKey neuron, ActivationFunction activationFunction)
        {
            _neuronActivationFunctions[neuron.Key] = activationFunction;
        }

        public ActivationFunction GetNeuronActivationFunction(NeuronKey neuron)
        {
            return _neuronActivationFunctions[neuron.Key];
        }

        public void SetSynapseWeight(NeuronKey from, NeuronKey to, double weight)
        {
            //Inputs cannot have synapses going towards them.
            if (_neurons[NeuronType.Input].Contains(to))
            {
                throw new InvalidOperationException("NeuronKey to cannot be an input neuron");
            }

            if (weight == 0 || double.IsNaN(weight))
            {
                _synapseWeights[from.Key].Remove(to.Key);
            }
            else
            {
                _synapseWeights[from.Key][to.Key] = weight;
            }
        }

        public double GetSynapseWeight(NeuronKey from, NeuronKey to)
        {
            if (_synapseWeights[from.Key].ContainsKey(to.Key))
            {
                return _synapseWeights[from.Key][to.Key];
            }

            return 0;
        }

        public IEnumerable<NeuronKey> GetNeurons(NeuronType neuronType)
        {
            return _neurons[neuronType];
        }

        public IEnumerable<NeuronKey> GetAllNeurons()
        {
            return _neurons.SelectMany(v => v.Value);
        }

        public TimeSpan UpdateNeuronValues(int cycles = 1)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < cycles; i++)
            {
                var currentNeuronValues = new Dictionary<ulong, double>(_neuronValues);
                var accumulativeNewNeuronValues = new Dictionary<ulong, double>(_neuronValues);

                //clear new neuron values
                foreach (var key in accumulativeNewNeuronValues.Keys.ToArray())
                {
                    accumulativeNewNeuronValues[key] = 0;
                }

                //calculate new values from old ones
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

                //store new values using synapse function
                foreach (var kv in accumulativeNewNeuronValues)
                {
                    _neuronValues[kv.Key] = CalculateActivationFunction(kv.Key, kv.Value);
                }
            }

            sw.Stop();
            return sw.Elapsed;
        }

        private double CalculateActivationFunction(ulong neuronKey, double accumulativeValue)
        {
            switch (_neuronActivationFunctions[neuronKey])
            {
                case ActivationFunction.Lineair:
                    return accumulativeValue;
                case ActivationFunction.LineairTruncated:
                    return (accumulativeValue >= 1) ? 1 : (accumulativeValue <= -1) ? -1 : accumulativeValue;
                case ActivationFunction.LineairTruncatedAtZero:
                    return (accumulativeValue >= 1) ? 1 : (accumulativeValue <= 0) ? 0 : accumulativeValue;
                case ActivationFunction.Binairy:
                    return (accumulativeValue >= 0.5) ? 1 : 0;
                case ActivationFunction.PositiveNegativeBinairy:
                    return (accumulativeValue >= 0) ? 1 : -1;
                case ActivationFunction.Sigmoid:
                    return (1 / (1 + Math.Exp(-accumulativeValue)));
                case ActivationFunction.HyperbolicTangent:
                    return Math.Tanh(accumulativeValue);
                default:
                    throw new NotImplementedException($"Synapse function {_neuronActivationFunctions[neuronKey]} is not implemented.");
            }
        }

        public NeuralNetwork Clone()
        {
            var networkClone = new NeuralNetwork(_neurons[NeuronType.Input].Count, _neurons[NeuronType.Output].Count);

            var normalNeurons = _neurons[NeuronType.Normal];
            for (int i = 0; i < normalNeurons.Count; i++)
            {
                networkClone.AddNeuron(ActivationFunction.Lineair);
            }

            var currentNeurons = GetAllNeurons();
            var cloneNeurons = networkClone.GetAllNeurons();

            if (currentNeurons.Count() != cloneNeurons.Count())
            {
                throw new InvalidOperationException("Clone failed");
            }

            currentNeurons.Zip(cloneNeurons, (original, clone) =>
            {
                var activationFunction = GetNeuronActivationFunction(original);
                networkClone.SetNeuronActivationFunction(clone, activationFunction);

                return currentNeurons.Zip(cloneNeurons, (original2, clone2) =>
                {
                    if (_neurons[NeuronType.Input].Contains(original2))
                    {
                        return 0;
                    }

                    var weight = GetSynapseWeight(original, original2);
                    networkClone.SetSynapseWeight(clone, clone2, weight);

                    return weight;
                }).ToArray(); // Make sure it is executed
            }).ToArray(); // Make sure it is executed

            return networkClone;
        }


        public override string ToString()
        {
            var allNeurons = GetAllNeurons();

            StringBuilder builder = new StringBuilder();

            builder.Append($"XXXX|");

            foreach (var neuron in allNeurons)
            {
                builder.Append($"{neuron.Key:0000}|");
            }

            builder.AppendLine();
            builder.AppendLine(string.Concat(Enumerable.Repeat("____+", allNeurons.Count()+1)));

            foreach (var neuron in allNeurons)
            {
                builder.Append($"{neuron.Key:0000}|");
                foreach (var neuron2 in allNeurons)
                {
                    var weight = GetSynapseWeight(neuron, neuron2);
                    if (weight >= 0)
                    {
                        builder.Append($"{weight:0.00}|");
                    }else
                    {
                        builder.Append($"{weight:0.0}|");
                    }
                }
                builder.AppendLine();
                builder.AppendLine(string.Concat(Enumerable.Repeat("____+", allNeurons.Count()+1)));
            }

            return builder.ToString();
        }
    }
}
