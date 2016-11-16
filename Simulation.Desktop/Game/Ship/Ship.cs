using GeneticNeuralNetworking.Genetics;
using NeuralNetworking.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Simulation.Game.Ship
{
    public class Ship : NeuralNetworkMutateable<Ship>
    {
        private double _speed;
        private double _steering;
        private int _targetsReached;
        private float _distanceToTarget;
        private static NeuralNetworkSettings _settings = new NeuralNetworkSettings(2, 2);
        private List<int> _targetTimestamps;
        private double _directionAngle;
        private NeuronKey _inputDistanceToTarget;
        private NeuronKey _inputAngleDifference;
        private NeuronKey _outputThrottle;
        private NeuronKey _outputSteering;
        private int _updates;
        private int _targetIndex;
        private bool _targetDirectionForward;

        public Ship() : this(new NeuralNetwork(_settings.NumberOfInputs, _settings.NumberOfOutputs))
        {

        }

        public static NeuralNetworkSettings Settings => _settings;

        private Ship(NeuralNetwork network) : base(network)
        {


            Initialize();
        }

        public Vector2 Position { get; private set; }

        public int TargetsReached => _targetsReached;
        public float DistanceToTarget => _distanceToTarget;
        public IEnumerable<int> TargetTimestamps => _targetTimestamps;

        public void Initialize()
        {
            NeuralNetwork.ClearNeuronValues();

            var inputNeurons = NeuralNetwork.GetNeurons(NeuronType.Input);
            _inputDistanceToTarget = inputNeurons.ElementAt(0);
            _inputAngleDifference = inputNeurons.ElementAt(1);

            var outputNeurons = NeuralNetwork.GetNeurons(NeuronType.Output);
            _outputThrottle = outputNeurons.ElementAt(0);
            _outputSteering = outputNeurons.ElementAt(1);

            Position = new Vector2(0, 0);
            _speed = 0;
            _steering = 0;
            _directionAngle = 0d;
            _targetTimestamps = new List<int>();

            _targetIndex = 0;
            _targetsReached = 0;
            _targetTimestamps.Add(0);
            _updates = 0;
            _targetDirectionForward = true;
        }

        public override void Update(TimeSpan lastUpdateDuration)
        {
            _updates++;

            _distanceToTarget = (Position - World.Targets[_targetIndex]).Length();

            if (_distanceToTarget < 50)
            {
                _targetIndex += (_targetDirectionForward ? 1 : -1);
                _targetsReached++;
                _targetTimestamps.Add(_updates);
                if (_targetIndex > World.Targets.Count() - 1 || _targetIndex < 0)
                {
                    _targetDirectionForward = !_targetDirectionForward;
                    _targetIndex += (_targetDirectionForward ? 2 : -2);
                }

                _distanceToTarget = (Position - World.Targets[_targetIndex]).Length();

            }

            var target = World.Targets[_targetIndex];

            var angleToTarget = RadianToDegree(Math.Atan2(target.X - Position.X, target.Y - Position.Y));

            var angleDifference = angleToTarget - _directionAngle;
            if (angleDifference > 180)
            {
                angleDifference -= 360;
            }
            else if (angleDifference < -180)
            {
                angleDifference += 360;
            }

            NeuralNetwork.SetNeuronInput(_inputDistanceToTarget, Clip(_distanceToTarget / 100d, -10, 10));
            NeuralNetwork.SetNeuronInput(_inputAngleDifference, angleDifference / 18d);

            NeuralNetwork.UpdateNeuronValues();

            var throttle = Clip(NeuralNetwork.GetNeuronOutput(_outputThrottle), -10, 10);
            _speed += throttle * 20;
            _speed = Clip(_speed, -5000, 5000);

            _steering = Clip(NeuralNetwork.GetNeuronOutput(_outputSteering), -10, 10);

            _directionAngle += _steering;
            _directionAngle %= 360;
            if (_directionAngle < 0)
            {
                _directionAngle += 360;
            }

            double radians = DegreeToRadian(_directionAngle);
            var direction = new Vector2(
                (float)Math.Cos(radians),
                -(float)Math.Sin(radians));

            Position += Vector2.Multiply(direction, (float)(_speed / 1000.0));

            _distanceToTarget = (Position - target).Length();
        }

        private double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        private double RadianToDegree(double angle)
        {
            return angle * 180 / Math.PI;
        }

        private double Clip(double value, double min, double max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public override Ship Clone()
        {
            return new Ship(NeuralNetwork.Clone());
        }
    }
}
