using Genometry.NeuralNetworking;
using Genometry.NeuralNetworking.Networking;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Diagnostics;
using System.Linq;

namespace NeuralNetworking.Tests.Networking
{
    [TestClass]
    public class NeuralNetworkTests
    {
        [TestMethod]
        public void Constructor_Parameters_NoExceptions()
        {
            var network = new NeuralNetwork(11, 9);

            Assert.AreEqual(11, network.GetNeurons(NeuronType.Input).Count());
            Assert.AreEqual(9, network.GetNeurons(NeuronType.Output).Count());
        }

        [TestMethod]
        [DataRow(ActivationFunction.Lineair)]
        [DataRow(ActivationFunction.LineairTruncated)]
        [DataRow(ActivationFunction.LineairTruncatedAtZero)]
        [DataRow(ActivationFunction.Binairy)]
        [DataRow(ActivationFunction.PositiveNegativeBinairy)]
        [DataRow(ActivationFunction.Sigmoid)]
        [DataRow(ActivationFunction.HyperbolicTangent)]
        [DataRow(ActivationFunction.Lineair)]
        [DataRow(ActivationFunction.LineairTruncated)]
        [DataRow(ActivationFunction.LineairTruncatedAtZero)]
        [DataRow(ActivationFunction.Binairy)]
        [DataRow(ActivationFunction.PositiveNegativeBinairy)]
        [DataRow(ActivationFunction.Sigmoid)]
        [DataRow(ActivationFunction.HyperbolicTangent)]
        [DataRow(ActivationFunction.Lineair)]
        [DataRow(ActivationFunction.LineairTruncated)]
        [DataRow(ActivationFunction.LineairTruncatedAtZero)]
        [DataRow(ActivationFunction.Binairy)]
        [DataRow(ActivationFunction.PositiveNegativeBinairy)]
        [DataRow(ActivationFunction.Sigmoid)]
        [DataRow(ActivationFunction.HyperbolicTangent)]
        public void AddNeuron_Parameters_NoException(ActivationFunction activationFunction)
        {
            var network = new NeuralNetwork(0, 0);
            network.AddNeuron(activationFunction);
        }

        [TestMethod]
        [DataRow(ActivationFunction.Lineair)]
        [DataRow(ActivationFunction.LineairTruncated)]
        [DataRow(ActivationFunction.LineairTruncatedAtZero)]
        [DataRow(ActivationFunction.Binairy)]
        [DataRow(ActivationFunction.PositiveNegativeBinairy)]
        [DataRow(ActivationFunction.Sigmoid)]
        [DataRow(ActivationFunction.HyperbolicTangent)]
        [DataRow(ActivationFunction.Lineair)]
        [DataRow(ActivationFunction.LineairTruncated)]
        [DataRow(ActivationFunction.LineairTruncatedAtZero)]
        [DataRow(ActivationFunction.Binairy)]
        [DataRow(ActivationFunction.PositiveNegativeBinairy)]
        [DataRow(ActivationFunction.Sigmoid)]
        [DataRow(ActivationFunction.HyperbolicTangent)]
        [DataRow(ActivationFunction.Lineair)]
        [DataRow(ActivationFunction.LineairTruncated)]
        [DataRow(ActivationFunction.LineairTruncatedAtZero)]
        [DataRow(ActivationFunction.Binairy)]
        [DataRow(ActivationFunction.PositiveNegativeBinairy)]
        [DataRow(ActivationFunction.Sigmoid)]
        [DataRow(ActivationFunction.HyperbolicTangent)]
        public void AddAndRemoveNeuron_Parameters_NoException(ActivationFunction activationFunction)
        {
            var network = new NeuralNetwork(0, 0);
            var key = network.AddNeuron(activationFunction);
            network.RemoveNeuron(key);
        }

        [TestMethod]
        public void RemoveNeuron_NonExistantNeuron_ArgumentException()
        {
            Assert.ThrowsException<ArgumentException>(() =>
            {
                var network1 = new NeuralNetwork(0, 0);
                var network2 = new NeuralNetwork(0, 0);

                var key = network1.AddNeuron(ActivationFunction.Lineair);
                network2.RemoveNeuron(key);
            });
        }

        [TestMethod]
        public void SetNeuronInput_GetInputNeurons_NoException()
        {
            var network = new NeuralNetwork(1, 0);
            var key = network.GetNeurons(NeuronType.Input).Single();

            network.SetNeuronInput(key, 13.37);
            network.SetNeuronInput(key, 0);
            network.SetNeuronInput(key, -13.37);
        }


        [TestMethod]
        [DataRow(NeuronType.Input, NeuronType.Output, false)]
        [DataRow(NeuronType.Input, NeuronType.Normal, false)]
        [DataRow(NeuronType.Normal, NeuronType.Output, false)]
        [DataRow(NeuronType.Output, NeuronType.Normal, false)]
        [DataRow(NeuronType.Output, NeuronType.Input, true)]
        [DataRow(NeuronType.Normal, NeuronType.Input, true)]
        public void SetSynapseWeight_FromToNeurons_NoException(NeuronType from, NeuronType to, bool expectException)
        {
            var network = new NeuralNetwork(1, 1);
            network.AddNeuron(ActivationFunction.Lineair);

            var fromKey = network.GetNeurons(from).Single();
            var toKey = network.GetNeurons(to).Single();

            bool gotException = false;
            try
            {
                network.SetSynapseWeight(fromKey, toKey, 1);
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.ToLower().Contains("cannot be an input neuron"))
                {
                    gotException = true;
                }
            }

            Assert.AreEqual(expectException, gotException);
        }


        [TestMethod]
        public void UpdateNeuronValues_FromInputToOutputNeuron_NoException()
        {
            var network = new NeuralNetwork(1, 1);
            var input = network.GetNeurons(NeuronType.Input).Single();
            var output = network.GetNeurons(NeuronType.Output).Single();

            network.SetSynapseWeight(input, output, 1);
            network.SetNeuronInput(input, 13.37);

            var duration = network.UpdateNeuronValues();

            if (duration.TotalMilliseconds > 10)
            {
                Assert.Inconclusive("Update neuron values took very long...");
            }

            Assert.AreEqual(13.37, network.GetNeuronOutput(output));
        }

        [TestMethod]
        [DataRow(10, 1, 10)]
        [DataRow(10, 0.5, 5)]
        [DataRow(10, 2, 20)]
        [DataRow(0, 1, 0)]
        [DataRow(-10, 1, -10)]
        [DataRow(-10, 0.5, -5)]
        [DataRow(-10, 2, -20)]
        public void UpdateNeuronValues_SynapseWeight_CorrectResult(double inputValue, double weight, double expectedOutput)
        {
            var network = new NeuralNetwork(1, 1);
            var input = network.GetNeurons(NeuronType.Input).Single();
            var output = network.GetNeurons(NeuronType.Output).Single();

            network.SetSynapseWeight(input, output, weight);
            network.SetNeuronInput(input, inputValue);

            var duration = network.UpdateNeuronValues();

            if (duration.TotalMilliseconds > 10)
            {
                Assert.Inconclusive("Update neuron values took very long...");
            }

            Assert.AreEqual(expectedOutput, network.GetNeuronOutput(output));
        }

        [TestMethod]
        [DataRow(0, ActivationFunction.Lineair, 0)]
        [DataRow(10, ActivationFunction.Lineair, 10)]
        [DataRow(-10, ActivationFunction.Lineair, -10)]

        [DataRow(0, ActivationFunction.LineairTruncated, 0)]
        [DataRow(10, ActivationFunction.LineairTruncated, 1)]
        [DataRow(-10, ActivationFunction.LineairTruncated, -1)]

        [DataRow(0, ActivationFunction.LineairTruncatedAtZero, 0)]
        [DataRow(10, ActivationFunction.LineairTruncatedAtZero, 1)]
        [DataRow(-10, ActivationFunction.LineairTruncatedAtZero, 0)]

        [DataRow(0, ActivationFunction.Binairy, 0)]
        [DataRow(10, ActivationFunction.Binairy, 1)]
        [DataRow(0.5, ActivationFunction.Binairy, 1)]
        [DataRow(-10, ActivationFunction.Binairy, 0)]

        [DataRow(0, ActivationFunction.PositiveNegativeBinairy, 1)]
        [DataRow(10, ActivationFunction.PositiveNegativeBinairy, 1)]
        [DataRow(-10, ActivationFunction.PositiveNegativeBinairy, -1)]

        [DataRow(0, ActivationFunction.Sigmoid, 0.5)]
        [DataRow(10, ActivationFunction.Sigmoid, 0.999954602131297565605495223767236510544906307995273931368)]
        [DataRow(-10, ActivationFunction.Sigmoid, 0.000045397868702434394504776232763489455093692004726068631)]

        [DataRow(0, ActivationFunction.HyperbolicTangent, 0)]
        [DataRow(10, ActivationFunction.HyperbolicTangent, 0.999999995877692763619592837138275741050814618495019962261)]
        [DataRow(-10, ActivationFunction.HyperbolicTangent, -0.99999999587769276361959283713827574105081461849501996226)]
        public void UpdateNeuronValues_SetNeuronActivationFunction_CorrectResult(double inputValue, ActivationFunction activationFunction, double expectedOutput)
        {
            var network = new NeuralNetwork(1, 1);
            var input = network.GetNeurons(NeuronType.Input).Single();
            var output = network.GetNeurons(NeuronType.Output).Single();

            network.SetSynapseWeight(input, output, 1);
            network.SetNeuronInput(input, inputValue);
            network.SetNeuronActivationFunction(output, activationFunction);

            var duration = network.UpdateNeuronValues();

            Debug.WriteLine($"Duration with activation function {activationFunction}: {duration}");
            if (duration.TotalMilliseconds > 10)
            {
                Assert.Inconclusive("Update neuron values took very long...");
            }

            Assert.AreEqual(expectedOutput, network.GetNeuronOutput(output));
        }


    }
}
