using Genetics.Mutating;
using System;
using System.Collections.Generic;
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

        public double Aggressiveness
        {
            get { return _poolMutator.Aggressiveness; }
            set
            {
                _poolMutator.Aggressiveness = value;
            }
        }

        protected abstract IEnumerable<FitnessFunction<T>> FitnessFunctions { get; }

        public IEnumerable<T> CreateNewGeneration(IEnumerable<T> generation, int generationSize)
        {
            int top = Math.Max(1, (int)(Aggressiveness * generation.Count()*0.5));
            Console.WriteLine($"Take top {top}");
            var selectionPool = GetTopSelection(generation, top);

            return _poolMutator.MutatePool(selectionPool, generationSize);
        }

        private IEnumerable<T> GetTopSelection(IEnumerable<T> generation, int topAmount)
        {
            var orderedByFitness = generation.OrderByDescending(CalculateFitness).ToArray();
            return orderedByFitness.Take(topAmount).ToArray();
        }

        private double CalculateFitness(T obj)
        {
            return FitnessFunctions.Sum((f) => f(obj).Value);
        }

        public T GetBest(IEnumerable<T> generation)
        {
            return generation.OrderByDescending(CalculateFitness).First();
        }

        public double GetBestFitness(IEnumerable<T> generation)
        {
            return generation.Select(CalculateFitness).OrderByDescending(f => f).First();
        }
    }
}
