using Genetics.Mutating;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Genetics.Tests.Mutating
{
    internal class MutateableDummy
    {
        public string Name { get; set; }

        public MutateableDummy()
        {
        }
    }

    internal class PoolMutatorDummyTester<T> : PoolMutator<T> where T : MutateableDummy, new()
    {
        protected override Dictionary<MutationMethod<T>, double> GetMutators()
        {
            return new Dictionary<MutationMethod<T>, double>()
            {
                { new MutationMethod<T>(CreateOne), 1 },
                { new MutationMethod<T>(CreateTwo), 2 },
                { new MutationMethod<T>(CreateThree), 0.5 }
            };
        }

        private T CreateOne()
        {
            var dummy = new T();
            dummy.Name = "1";
            return dummy;
        }

        private T CreateTwo(T d1)
        {
            var dummy = new T();
            dummy.Name = "2";
            return dummy;
        }

        private T CreateThree(T d1, T d2)
        {
            var dummy = new T();
            dummy.Name = "3";
            return dummy;
        }
    }

    [TestClass]
    public class PoolMutatorTests
    {
        [TestMethod]
        public void ValidateMutator_ValidType1_NoExceptions()
        {
            var mutator = new PoolMutatorDummyTester<MutateableDummy>();

            Assert.AreEqual(3, mutator.MutatorCount);
        }
        
        [TestMethod]
        public void MutatePool_OutputPoolSize_Correct()
        {
            var inputPool = new[]
            {
                new MutateableDummy(),
                new MutateableDummy(),
                new MutateableDummy(),
                new MutateableDummy(),
                new MutateableDummy()
            };

            PoolMutator<MutateableDummy> mutator = new PoolMutatorDummyTester<MutateableDummy>();

            var mutations = mutator.MutatePool(inputPool, 100);

            Assert.AreEqual(100, mutations.Count());
            Assert.IsTrue(mutations.All(m => m.Name != null));
            Assert.IsTrue(mutations.Any(m => m.Name == "1"));
            Assert.IsTrue(mutations.Any(m => m.Name == "2"));
            Assert.IsTrue(mutations.Any(m => m.Name == "3"));
        }


    }
}
