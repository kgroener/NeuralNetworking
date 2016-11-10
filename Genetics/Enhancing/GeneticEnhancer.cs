using Genetics.Mutating;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Genetics.Enhancing
{
    public class FitnessResult
    {
        public FitnessResult(double value)
        {
            Value = value;
        }
        public double Value { get; }
    }

    public delegate FitnessResult FitnessFunction<T>(T obj);

    public abstract class GeneticEnhancer<T> : IGeneticEnhancer<T>
    {
        private readonly PoolMutator<T> _poolMutator;

        public GeneticEnhancer(PoolMutator<T> mutator)
        {
            _poolMutator = mutator;
        }

        protected abstract IEnumerable<FitnessFunction<T>> FitnessFunctions { get; }

        public IEnumerable<T> CreateNewGeneration(IEnumerable<T> selectionPool, int generationSize)
        {
            return _poolMutator.MutatePool(selectionPool, generationSize);
        }

        public IEnumerable<T> GetTopSelection(IEnumerable<T> generation, int topAmount)
        {
            var orderedByFitness = generation.OrderByDescending(CalculateFitness).ToArray();
            var best = orderedByFitness.Take(topAmount).ToArray();

#if DEBUG
            if (best.Any())
            {
                Debug.WriteLine($"Fitness: {CalculateFitness(best.First())}");
            }
#endif

            return best;
        }

        public double CalculateFitness(T obj)
        {
            return FitnessFunctions.Sum((f) => f(obj).Value);
        }
    }
}
