using Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Genetics.Mutating
{
    public abstract class PoolMutator<T>
    {
        protected readonly Random _random;
        private Dictionary<MutationMethod<T>, double> _mutators;
        private double _aggressiveness;

        public PoolMutator()
        {
            _random = new Random();
            _mutators = GetMutators();
            Aggressiveness = 0.25;
        }

        internal int MutatorCount { get { return _mutators.Count; } }

        protected abstract Dictionary<MutationMethod<T>, double> GetMutators();

        public double Aggressiveness
        {
            get
            {
                return _aggressiveness;
            }
            set
            {
                _aggressiveness = value > 1 ? 1 : value < 0 ? 0 : value;
            }
        }

        internal IEnumerable<T> MutatePool(IEnumerable<T> inputPool, int outputPoolSize)
        {
            List<T> outputPool = new List<T>(inputPool);

            while (outputPool.Count != outputPoolSize)
            {
                var availableMutators = _mutators.Where(m => m.Key.ParameterCount <= inputPool.Count());
                var probabilitySum = availableMutators.Sum(m => m.Value);

                var mutationNumber = (_random.NextDouble() * probabilitySum) * 100;
                var selectedMutator = availableMutators.SkipWhile(kv => (mutationNumber -= kv.Value * 100) > 0).First().Key;

                var amountOfParameters = selectedMutator.ParameterCount;

                T mutation = selectedMutator.Invoke(inputPool.GetRandomizedEnumerable(amountOfParameters).ToArray());

                outputPool.Add(mutation);
            }

            return outputPool;
        }
    }
}
