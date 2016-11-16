using System;

namespace Genetics.Mutating
{
    public delegate T MultiMutatorDelegate<T>(T[] args);
    public delegate T SingleMutatorDelegate<T>();

    public sealed class MutationMethod<T>
    {
        private object _function;

        public MutationMethod(Func<T> function)
        {
            ParameterCount = 0;
            _function = function;
        }

        public MutationMethod(Func<T, T> function)
        {
            ParameterCount = 1;
            _function = function;
        }

        public MutationMethod(Func<T, T, T> function)
        {
            ParameterCount = 2;
            _function = function;
        }

        internal uint ParameterCount { get; }

        public T Invoke(params T[] args)
        {
            switch (ParameterCount)
            {
                case 0:
                    return (_function as Func<T>).Invoke();
                case 1:
                    return (_function as Func<T, T>).Invoke(args[0]);
                case 2:
                    return (_function as Func<T, T, T>).Invoke(args[0], args[1]);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
