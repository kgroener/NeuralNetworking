using GeneticNeuralNetworking.Genetics;
using NeuralNetworking.Networking;
using NeuralNetworking.Networking.Neurons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Simulation.Game.Ship
{
    public class Ship : NeuralNetworkMutateable<Ship>
    {
        private static NeuralNetworkSettings _settings = new NeuralNetworkSettings(5, 2);

        private double _speed;
        private double _steering;
        private int _targetsReached;
        private float _distanceToTarget;
        private List<int> _targetTimestamps;
        private double _directionAngle;
        private IInputNeuron _inputDistanceToTarget;
        private IInputNeuron _inputDistanceToNextTarget;
        private IInputNeuron _inputAngleDifference;
        private IInputNeuron _inputAngleDifferenceNextTarget;
        private IInputNeuron _inputCurrentSteeringForce;
        private IOutputNeuron _outputThrottle;
        private IOutputNeuron _outputSteering;
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
            NeuralNetwork.Reset();

            var inputNeurons = NeuralNetwork.GetInputs();
            _inputDistanceToTarget = inputNeurons.ElementAt(0);
            _inputAngleDifference = inputNeurons.ElementAt(1);
            _inputDistanceToNextTarget = inputNeurons.ElementAt(2);
            _inputAngleDifferenceNextTarget = inputNeurons.ElementAt(3);
            _inputCurrentSteeringForce = inputNeurons.ElementAt(4);

            var outputNeurons = NeuralNetwork.GetOutputs();
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

        private Vector2 GetNextTarget(int index)
        {
            if (_targetDirectionForward && index == World.Targets.Count() - 1)
            {
                return World.Targets[index - 1];
            }
            else if (!_targetDirectionForward && index == 0)
            {
                return World.Targets[1];
            }
            else
            {
                return World.Targets[index + (_targetDirectionForward ? 1 : -1)];
            }
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

            var nextTarget = GetNextTarget(_targetIndex);
            var angleToNextTarget = RadianToDegree(Math.Atan2(nextTarget.X - Position.X, nextTarget.Y - Position.Y));
            var distanceToNextTarget = (Position - nextTarget).Length();

            var angleDifference = angleToTarget - _directionAngle;
            if (angleDifference > 180)
            {
                angleDifference -= 360;
            }
            else if (angleDifference < -180)
            {
                angleDifference += 360;
            }

            var angleDifferenceToNextTarget = angleToNextTarget - _directionAngle;
            if (angleDifferenceToNextTarget > 180)
            {
                angleDifferenceToNextTarget -= 360;
            }
            else if (angleDifferenceToNextTarget < -180)
            {
                angleDifferenceToNextTarget += 360;
            }

            _inputDistanceToTarget.SetInputValue(Clip(_distanceToTarget / 100d, -10, 10));
            _inputDistanceToNextTarget.SetInputValue(Clip(distanceToNextTarget / 100d, -10, 10));
            _inputAngleDifference.SetInputValue(angleDifference / 18d);
            _inputAngleDifferenceNextTarget.SetInputValue(angleDifferenceToNextTarget / 18d);
            _inputCurrentSteeringForce.SetInputValue(_steering);

            NeuralNetwork.UpdateNeuronValues();

            var throttle = _outputThrottle.GetOutputValue();
            _speed += throttle * 20;
            _speed = Clip(_speed, -100, 5000);

            var dSteering = _outputSteering.GetOutputValue();

            _steering = Clip(_steering + (dSteering), -5, 5);
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
