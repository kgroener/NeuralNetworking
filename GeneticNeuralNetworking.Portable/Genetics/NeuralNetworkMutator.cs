using Extensions;
using Genetics.Mutating;
using NeuralNetworking.Networking;
using NeuralNetworking.Networking.Neurons;
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

            var numberOfNeuronsToAdd = _random.Next(1, 1 + (int)(20 * Aggressiveness));

            for (int i = 0; i < numberOfNeuronsToAdd; i++)
            {
                newNetwork.AddHiddenNeuron(GetRandomActivationFunction());
            }

            for (int i = 0; i < GetRandomAddCount(); i++)
            {
                newNetwork.AddConstantNeuron(GetRandomConstant());
            }

            var fromNeurons = newNetwork.GetAllNeurons();
            var toNeurons = fromNeurons.Where(n => !(n is ISupplierNeuron)).ToArray();

            var numberOfSynapsesToAdd = _random.Next(1 + (int)(20 * Aggressiveness), 1 + (int)(200 * Aggressiveness));
            for (int i = 0; i < numberOfSynapsesToAdd; i++)
            {
                newNetwork.SetSynapseWeight(
                    fromNeurons.GetRandomElement(),
                    toNeurons.GetRandomElement(),
                    GetRandomSynapseWeight());
            }

            var mutateableNetwork = new T();
            mutateableNetwork.NeuralNetwork = newNetwork;
            return mutateableNetwork;
        }

        private double GetRandomSynapseWeight()
        {
            return (_random.NextDouble() - 0.5) * 4 * (1 + Aggressiveness);
        }

        private double GetRandomConstant()
        {
            return (_random.NextDouble() - 0.5) * 10 * (1 + Aggressiveness);
        }


        private T Mutate(T otherNetwork)
        {
            var clone = otherNetwork.Clone();

            var allNeurons = clone.NeuralNetwork.GetAllNeurons();

            for (int i = 0; i < allNeurons.Count() * ((1 + Aggressiveness) / 2); i++)
            {
                allNeurons = clone.NeuralNetwork.GetAllNeurons();
                var toNeurons = allNeurons.OfType<ICalculateableNeuron>().ToArray();

                var neuronFrom = allNeurons.GetRandomElement();
                var neuronTo = toNeurons.GetRandomElement();

                var option = _random.Next(0, 6);

                switch (option)
                {
                    case 0: // change synapse weight
                        var synapseWeight = clone.NeuralNetwork.GetSynapseWeight(neuronFrom, neuronTo);
                        clone.NeuralNetwork.SetSynapseWeight(neuronFrom, neuronTo, DoRandomMathematics(synapseWeight));
                        break;
                    case 1: // change activation function
                        neuronTo.ChangeActivationFunction(GetRandomActivationFunction());
                        break;
                    case 2: // add neuron with synapses
                        {
                            var neuronAdded = clone.NeuralNetwork.AddHiddenNeuron(GetRandomActivationFunction());

                            var numberOfSynapsesToAdd = GetRandomAddCount();
                            for (int a = 0; a < numberOfSynapsesToAdd; a++)
                            {
                                clone.NeuralNetwork.SetSynapseWeight(
                                    neuronAdded,
                                    toNeurons.GetRandomElement(),
                                    GetRandomSynapseWeight());
                            }
                            numberOfSynapsesToAdd = GetRandomAddCount();
                            for (int a = 0; a < numberOfSynapsesToAdd; a++)
                            {
                                clone.NeuralNetwork.SetSynapseWeight(
                                    allNeurons.GetRandomElement(),
                                    neuronAdded,
                                    GetRandomSynapseWeight());
                            }
                        }
                        break;
                    case 3: // remove neuron
                        var removableNeurons = allNeurons.Except(clone.NeuralNetwork.GetInputs()).Except(clone.NeuralNetwork.GetOutputs()).ToArray();
                        if (removableNeurons.Any())
                        {
                            var neuronToRemove = removableNeurons.GetRandomElement();
                            clone.NeuralNetwork.RemoveNeuron(neuronToRemove);
                        }
                        break;
                    case 4: // remove synapse
                        clone.NeuralNetwork.SetSynapseWeight(neuronFrom, neuronTo, 0);
                        break;
                    case 5: // add a constant
                        {
                            var constantAdded = clone.NeuralNetwork.AddConstantNeuron(GetRandomConstant());

                            var numberOfSynapsesToAdd = GetRandomAddCount();
                            for (int a = 0; a < numberOfSynapsesToAdd; a++)
                            {
                                clone.NeuralNetwork.SetSynapseWeight(
                                    constantAdded,
                                    toNeurons.GetRandomElement(),
                                    GetRandomSynapseWeight());
                            }
                        }
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            return clone;
        }

        private int GetRandomAddCount()
        {
            return _random.Next(1, 1 + (int)(3 * Aggressiveness));
        }

        private T CrossOver(T networkA, T networkB)
        {
            var neuronsA = networkA.NeuralNetwork.GetInsideNeurons();
            var neuronsB = networkB.NeuralNetwork.GetInsideNeurons();

            var ra = _random.Next(0, 1 + (neuronsA.Count() / 2));
            var rb = ((neuronsA.Count() + neuronsB.Count()) / 2) - ra;

            var newNetwork = new NeuralNetwork(_settings.NumberOfInputs, _settings.NumberOfOutputs);
            var newInputs = newNetwork.GetInputs();
            var newOutputs = newNetwork.GetOutputs();


            Dictionary<INeuron, INeuron> keyMappingA = new Dictionary<INeuron, INeuron>();
            Dictionary<INeuron, INeuron> keyMappingB = new Dictionary<INeuron, INeuron>();

            networkA.NeuralNetwork.GetInputs().MapAction(newNetwork.GetInputs(), (old, clone) =>
            {
                keyMappingA[clone] = old;
            });
            networkA.NeuralNetwork.GetOutputs().MapAction(newNetwork.GetOutputs(), (old, clone) =>
            {
                keyMappingA[clone] = old;
            });

            networkB.NeuralNetwork.GetInputs().MapAction(newNetwork.GetInputs(), (old, clone) =>
            {
                keyMappingB[clone] = old;
            });
            networkB.NeuralNetwork.GetOutputs().MapAction(newNetwork.GetOutputs(), (old, clone) =>
            {
                keyMappingB[clone] = old;
            });

            foreach (var neuron in neuronsA.Take(ra))
            {
                var neuronAdded = newNetwork.AddNeuronCopy(neuron);
                keyMappingA[neuronAdded] = neuron;
            }

            foreach (var neuron in neuronsB.Reverse().Take(rb))
            {
                var neuronAdded = newNetwork.AddNeuronCopy(neuron);
                keyMappingB[neuronAdded] = neuron;
            }

            foreach (var neuron in newNetwork.GetAllNeurons())
            {
                if (keyMappingA.ContainsKey(neuron))
                {
                    foreach (var mapping in keyMappingA)
                    {
                        if (!(mapping.Key is ISupplierNeuron))
                        {
                            var weight = networkA.NeuralNetwork.GetSynapseWeight(keyMappingA[neuron], mapping.Value);
                            newNetwork.SetSynapseWeight(neuron, mapping.Key, weight);
                        }
                        if (!(neuron is ISupplierNeuron))
                        {
                            var weight = networkA.NeuralNetwork.GetSynapseWeight(mapping.Value, keyMappingA[neuron]);
                            newNetwork.SetSynapseWeight(mapping.Key, neuron, weight);
                        }
                    }
                }
                else
                {
                    foreach (var mapping in keyMappingB)
                    {
                        if (!(mapping.Key is ISupplierNeuron))
                        {
                            var weight = networkB.NeuralNetwork.GetSynapseWeight(keyMappingB[neuron], mapping.Value);
                            newNetwork.SetSynapseWeight(neuron, mapping.Key, weight);
                        }
                        if (!(neuron is ISupplierNeuron))
                        {
                            var weight = networkB.NeuralNetwork.GetSynapseWeight(mapping.Value, keyMappingB[neuron]);
                            newNetwork.SetSynapseWeight(mapping.Key, neuron, weight);
                        }
                    }
                }
            }

            //finishing touch
            var allNeuronsA = keyMappingA.Keys;
            var allNeuronsB = keyMappingB.Keys;

            if (allNeuronsA.Count() > 0 && allNeuronsB.Count() > 0)
            {
                var numberOfSynapsesToAdd = _random.Next(0, allNeuronsA.Count() + allNeuronsB.Count());

                for (int i = 0; i < numberOfSynapsesToAdd; i++)
                {
                    if (_random.Next(0, 2) == 0)
                    {
                        newNetwork.SetSynapseWeight(
                            allNeuronsA.GetRandomElement(),
                            allNeuronsB.Where(n => !(n is ISupplierNeuron)).GetRandomElement(),
                            GetRandomSynapseWeight());
                    }
                    else
                    {
                        newNetwork.SetSynapseWeight(
                            allNeuronsB.GetRandomElement(),
                            allNeuronsA.Where(n => !(n is ISupplierNeuron)).GetRandomElement(),
                            GetRandomSynapseWeight());
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
