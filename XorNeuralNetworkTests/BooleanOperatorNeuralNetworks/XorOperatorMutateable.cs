using Genometry.NeuralNetworking.Networking;
using System.Linq;

namespace XorNeuralNetworkTests
{
    class XorOperatorMutateable : BooleanOperatorMutateable<XorOperatorMutateable>
    {
        public XorOperatorMutateable() : base(Operator)
        {
        }

        private XorOperatorMutateable(NeuralNetwork network) : base(Operator, network)
        {

        }

        private static bool Operator(bool a, bool b)
        {
            return a ^ b;
        }

        public override XorOperatorMutateable Clone()
        {
            return new XorOperatorMutateable(NeuralNetwork.Clone());
        }

        public override string ToString()
        {
            return this.RunResults.Sum(r => r.Value.Sum(k => k.Value)).ToString();
        }
    }
}
