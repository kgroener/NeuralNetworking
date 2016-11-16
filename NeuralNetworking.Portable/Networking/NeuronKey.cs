namespace NeuralNetworking.Networking
{
    public class NeuronKey
    {

        internal NeuronKey(ulong key)
        {
            Key = key;
        }

        internal ulong Key { get; }
    }

    public class NeuronKeyGenerator
    {
        private ulong _keyCounter;
        private object _keyLock = new object();

        public NeuronKey Create()
        {
            lock (_keyLock)
            {
                return new NeuronKey(_keyCounter++);
            }
        }
    }

}
