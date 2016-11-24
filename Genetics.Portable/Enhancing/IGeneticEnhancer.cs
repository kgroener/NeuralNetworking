using System.Collections.Generic;

namespace Genetics.Enhancing
{
    public interface IGeneticEnhancer<T>
    {
        IEnumerable<T> CreateNewGeneration(IEnumerable<T> selectionPool, int generationSize);
        T GetBest(IEnumerable<T> generation);
    }
}
