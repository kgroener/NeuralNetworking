using System.Collections.Generic;

namespace Simulation.Genetics.Enhancing
{
    public interface IGeneticEnhancer<T>
    {
        IEnumerable<T> CreateNewGeneration(IEnumerable<T> selectionPool, int generationSize);
        IEnumerable<T> GetTopSelection(IEnumerable<T> generation, int topAmount);
    }
}
