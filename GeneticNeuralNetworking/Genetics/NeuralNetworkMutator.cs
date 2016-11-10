using Extensions;
using Genetics.Mutating;
using NeuralNetworking;
using NeuralNetworking.Networking;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticNeuralNetworking.Genetics
{
    public class NeuralNetworkMutator<T> : PoolMutator<T> where T : NeuralNetworkMutateable<T>, new()
    {
        private readonly NeuralNetworkSettings _settings;

        public NeuralNetworkMutator(NeuralNetworkSettings settings)
        {
            _settings = settings;
        }

        protected override Dictionary<MutationMethod<T>, double> GetMutators()
        {
            return new Dictionary<MutationMethod<T>, double>()
            {
                { new MutationMethod<T>(CreateRandom), 1 },
                { new MutationMethod<T>(Mutate), 4 },
                { new MutationMethod<T>(CrossOver), 1 },
                { new MutationMethod<T>(CrossOverRandom), 2 },
            };
        }

        private T CrossOverRandom(T networkA)
        {
            return CrossOver(networkA, CreateRandom());
        }

        private ActivationFunction GetRandomActivationFunction()
        {
            return Enum.GetValues(typeof(ActivationFunction)).Cast<ActivationFunction>().GetRandomElement();
        }

        private double DoRandomMathematics(double input)
        {
            var option = _random.Next(0, 3);
            switch (option)
            {
                case 0:
                    return input + ((_random.NextDouble() - 0.5) * 2 * input);
                case 1:
                    return input * ((_random.NextDouble() - 0.5) * 3);
                case 2:
                    return input / (1 + _random.NextDouble());
                default:
                    throw new NotImplementedException();
            }
        }


        private T CreateRandom()
        {
            var newNetwork = new NeuralNetwork(_settings.NumberOfInputs, _settings.NumberOfOutputs);

            var numberOfNeuronsToAdd = _random.Next(1, 20);

            for (int i = 0; i < numberOfNeuronsToAdd; i++)
            {
                newNetwork.AddNeuron(GetRandomActivationFunction());
            }

            var fromNeurons = newNetwork.GetAllNeurons();
            var toNeurons = fromNeurons.Except(newNetwork.GetNeurons(NeuronType.Input)).ToArray();

            var numberOfSynapsesToAdd = _random.Next(20, 200);
            for (int i = 0; i < numberOfSynapsesToAdd; i++)
            {
                newNetwork.SetSynapseWeight(
                    fromNeurons.GetRandomElement(),
                    toNeurons.GetRandomElement(),
                    (_random.NextDouble() - 0.5) * 4);
            }

            var mutateableNetwork = new T();
            mutateableNetwork.NeuralNetwork = newNetwork;
            return mutateableNetwork;
        }

        private T Mutate(T otherNetwork)
        {
            var clone = otherNetwork.Clone();

            var allNeurons = clone.NeuralNetwork.GetAllNeurons();

            for (int i = 0; i < allNeurons.Count() / 10; i++)
            {
                allNeurons = clone.NeuralNetwork.GetAllNeurons();
                var toNeurons = allNeurons.Except(clone.NeuralNetwork.GetNeurons(NeuronType.Input)).ToArray();

                var neuronFrom = allNeurons.GetRandomElement();
                var neuronTo = toNeurons.GetRandomElement();

                var option = _random.Next(0, 5);

                switch (option)
                {
                    case 0: // change synapse weight
                        var synapseWeight = clone.NeuralNetwork.GetSynapseWeight(neuronFrom, neuronTo);
                        clone.NeuralNetwork.SetSynapseWeight(neuronFrom, neuronTo, DoRandomMathematics(synapseWeight));
                        break;
                    case 1: // change activation function
                        clone.NeuralNetwork.SetNeuronActivationFunction(neuronFrom, GetRandomActivationFunction());
                        break;
                    case 2: // add neuron with synapses
                        var neuronAdded = clone.NeuralNetwork.AddNeuron(GetRandomActivationFunction());

                        var numberOfSynapsesToAdd = _random.Next(1, 3);
                        for (int a = 0; a < numberOfSynapsesToAdd; a++)
                        {
                            clone.NeuralNetwork.SetSynapseWeight(
                                neuronAdded,
                                toNeurons.GetRandomElement(),
                                (_random.NextDouble() - 0.5) * 4);
                        }
                        numberOfSynapsesToAdd = _random.Next(1, 3);
                        for (int a = 0; a < numberOfSynapsesToAdd; a++)
                        {
                            clone.NeuralNetwork.SetSynapseWeight(
                                allNeurons.GetRandomElement(),
                                neuronAdded,
                                (_random.NextDouble() - 0.5) * 4);
                        }
                        break;
                    case 3: // remove neuron
                        var neurons = clone.NeuralNetwork.GetNeurons(NeuronType.Normal);
                        if (neurons.Any())
                        {
                            var neuronToRemove = neurons.GetRandomElement();
                            clone.NeuralNetwork.RemoveNeuron(neuronToRemove);
                        }
                        break;
                    case 4: // remove synapse
                        clone.NeuralNetwork.SetSynapseWeight(neuronFrom, neuronTo, 0);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            return clone;
        }

        private T CrossOver(T networkA, T networkB)
        {
            var neuronsA = networkA.NeuralNetwork.GetNeurons(NeuronType.Normal);
            var neuronsB = networkB.NeuralNetwork.GetNeurons(NeuronType.Normal);

            var ra = _random.Next(0, 1 + (neuronsA.Count() / 2));
            var rb = _random.Next(0, 1 + (neuronsB.Count() / 2));

            var newNetwork = new NeuralNetwork(_settings.NumberOfInputs, _settings.NumberOfOutputs);
            var newInputs = newNetwork.GetNeurons(NeuronType.Input);
            var newOutputs = newNetwork.GetNeurons(NeuronType.Output);

            Dictionary<NeuronKey, NeuronKey> keyMappingA = new Dictionary<NeuronKey, NeuronKey>();
            Dictionary<NeuronKey, NeuronKey> keyMappingB = new Dictionary<NeuronKey, NeuronKey>();

            foreach (var neuron in neuronsA.Take(ra))
            {
                var neuronAdded = newNetwork.AddNeuron(networkA.NeuralNetwork.GetNeuronActivationFunction(neuron));
                keyMappingA[neuronAdded] = neuron;
            }

            foreach (var neuron in neuronsB.Skip(rb))
            {
                var neuronAdded = newNetwork.AddNeuron(networkB.NeuralNetwork.GetNeuronActivationFunction(neuron));
                keyMappingB[neuronAdded] = neuron;
            }

            foreach (var neuron in newNetwork.GetAllNeurons())
            {
                foreach (var mapping in keyMappingA)
                {
                    if (newInputs.Contains(neuron))
                    {
                        var weight = networkA.NeuralNetwork.GetSynapseWeight(neuron, mapping.Value);
                        newNetwork.SetSynapseWeight(neuron, mapping.Key, weight);
                    }
                    else if (newOutputs.Contains(neuron))
                    {
                        var weight = networkA.NeuralNetwork.GetSynapseWeight(neuron, mapping.Value);
                        newNetwork.SetSynapseWeight(neuron, mapping.Key, weight);

                        weight = networkA.NeuralNetwork.GetSynapseWeight(mapping.Value, neuron);
                        newNetwork.SetSynapseWeight(mapping.Key, neuron, weight);
                    }
                    else if (keyMappingA.ContainsKey(neuron))
                    {
                        var weight = networkA.NeuralNetwork.GetSynapseWeight(keyMappingA[neuron], mapping.Value);
                        newNetwork.SetSynapseWeight(neuron, mapping.Key, weight);

                        weight = networkA.NeuralNetwork.GetSynapseWeight(mapping.Value, keyMappingA[neuron]);
                        newNetwork.SetSynapseWeight(mapping.Key, neuron, weight);
                    }
                }
                foreach (var mapping in keyMappingB)
                {
                    if (newInputs.Contains(neuron))
                    {
                        var weight = networkB.NeuralNetwork.GetSynapseWeight(neuron, mapping.Value);
                        newNetwork.SetSynapseWeight(neuron, mapping.Key, weight);
                    }
                    else if (newOutputs.Contains(neuron))
                    {
                        var weight = networkB.NeuralNetwork.GetSynapseWeight(neuron, mapping.Value);
                        newNetwork.SetSynapseWeight(neuron, mapping.Key, weight);

                        weight = networkB.NeuralNetwork.GetSynapseWeight(mapping.Value, neuron);
                        newNetwork.SetSynapseWeight(mapping.Key, neuron, weight);
                    }
                    else if (keyMappingB.ContainsKey(neuron))
                    {
                        var weight = networkB.NeuralNetwork.GetSynapseWeight(keyMappingB[neuron], mapping.Value);
                        newNetwork.SetSynapseWeight(neuron, mapping.Key, weight);

                        weight = networkB.NeuralNetwork.GetSynapseWeight(mapping.Value, keyMappingB[neuron]);
                        newNetwork.SetSynapseWeight(mapping.Key, neuron, weight);
                    }
                }
            }

            //finishing touch
            var allNeuronsA = keyMappingA.Keys;
            var allNeuronsB = keyMappingB.Keys;

            if (allNeuronsA.Count > 0 && allNeuronsB.Count > 0)
            {
                var numberOfSynapsesToAdd = _random.Next(0, allNeuronsA.Count + allNeuronsB.Count);

                for (int i = 0; i < numberOfSynapsesToAdd; i++)
                {
                    if (_random.Next(0, 2) == 0)
                    {
                        newNetwork.SetSynapseWeight(
                            allNeuronsA.GetRandomElement(),
                            allNeuronsB.GetRandomElement(),
                            (_random.NextDouble() - 0.5) * 4);
                    }
                    else
                    {
                        newNetwork.SetSynapseWeight(
                            allNeuronsB.GetRandomElement(),
                            allNeuronsA.GetRandomElement(),
                            (_random.NextDouble() - 0.5) * 4);
                    }
                }
            }

            return new T()
            {
                NeuralNetwork = newNetwork
            };
        }
    }
}
