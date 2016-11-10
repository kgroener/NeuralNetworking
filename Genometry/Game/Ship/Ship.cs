using GeneticNeuralNetworking.Genetics;
using Genometry.NeuralNetworking.Networking;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace Genometry.Game.Ship
{
    public class Ship : NeuralNetworkMutateable<Ship>
    {
        private double _speed;
        private double _steering;
        private int _targetsReached;
        private float _distanceToTarget;
        private static NeuralNetworkSettings _settings = new NeuralNetworkSettings(8, 2);
        private TimeSpan _runDuration;
        private List<int> _targetTimestamps;

        public event EventHandler Updated;

        public Ship() : this(new NeuralNetwork(_settings.NumberOfInputs, _settings.NumberOfOutputs))
        {

        }

        public static NeuralNetworkSettings Settings => _settings;

        private Ship(NeuralNetwork network) : base(network)
        {
            Position = new Vector2(0, 0);
            Direction = new Vector2(0, 0);
        }

        public Vector2 Position { get; private set; }
        public Vector2 Direction { get; private set; }
        public Vector2 Target { get; private set; }

        public int TargetsReached => _targetsReached;
        public float DistanceToTarget => _distanceToTarget;
        public TimeSpan RunDuration => _runDuration;
        public IEnumerable<int> TargetTimestamps => _targetTimestamps;

        public override Task RunAsync()
        {
            return Task.Run(async () =>
            {
                NeuralNetwork.ClearNeuronValues();
                Position = new Vector2(0, 0);
                Direction = new Vector2(0, 0);
                _speed = 0;
                _steering = 0;
                var directionAngle = 0d;
                _targetTimestamps = new List<int>();

                var inputNeurons = NeuralNetwork.GetNeurons(NeuronType.Input);
                var inputPositionX = inputNeurons.ElementAt(0);
                var inputPositionY = inputNeurons.ElementAt(1);
                var inputDirectionX = inputNeurons.ElementAt(2);
                var inputDirectionY = inputNeurons.ElementAt(3);
                var inputDistanceToTarget = inputNeurons.ElementAt(4);
                var inputAngleDifference = inputNeurons.ElementAt(5);
                var inputTargetX = inputNeurons.ElementAt(6);
                var inputTargetY = inputNeurons.ElementAt(7);

                var outputNeurons = NeuralNetwork.GetNeurons(NeuronType.Output);
                var outputThrottle = outputNeurons.ElementAt(0);
                var outputSteering = outputNeurons.ElementAt(1);

                _targetsReached = 0;

                _targetTimestamps.Add(0);

                int updates = 0;
                foreach (var target in World.Targets)
                {
                    Target = target;

                    NeuralNetwork.SetNeuronInput(inputTargetX, target.X);
                    NeuralNetwork.SetNeuronInput(inputTargetY, target.Y);

                    _distanceToTarget = (Position - target).Length();

                    while (_distanceToTarget > 50)
                    {
                        var angleToTarget = RadianToDegree(Math.Atan2(Target.X - Position.X, Target.Y - Position.Y));

                        var angleDifference = angleToTarget - directionAngle;
                        if (angleDifference > 180)
                        {
                            angleDifference -= 360;
                        }
                        else if (angleDifference < -180)
                        {
                            angleDifference += 360;
                        }

                        NeuralNetwork.SetNeuronInput(inputPositionX, Position.X);
                        NeuralNetwork.SetNeuronInput(inputPositionY, Position.Y);
                        NeuralNetwork.SetNeuronInput(inputDirectionX, Direction.X);
                        NeuralNetwork.SetNeuronInput(inputDirectionY, Direction.X);
                        NeuralNetwork.SetNeuronInput(inputDistanceToTarget, _distanceToTarget);
                        NeuralNetwork.SetNeuronInput(inputAngleDifference, angleDifference);

                        NeuralNetwork.UpdateNeuronValues();

                        var throttle = Clip(NeuralNetwork.GetNeuronOutput(outputThrottle), -200, 200);
                        _speed += throttle;
                        _speed = Clip(_speed, -5000, 5000);

                        _steering = Clip(NeuralNetwork.GetNeuronOutput(outputSteering), -100, 100);

                        directionAngle += _steering / 10.0;
                        directionAngle %= 360;
                        if (directionAngle < 0)
                        {
                            directionAngle += 360;
                        }

                        double radians = DegreeToRadian(directionAngle);
                        Direction = new Vector2(
                            (float)Math.Cos(radians),
                            -(float)Math.Sin(radians));

                        Position += Vector2.Multiply(Direction, (float)(_speed / 1000.0));

                        //Position = new Vector2((float)Clip(Position.X, -500, 1000), (float)Clip(Position.Y, -500, 1000));

                        _distanceToTarget = (Position - target).Length();

                        Updated?.Invoke(this, EventArgs.Empty);

                        await Task.Delay(15);

                        if (++updates >= 1000)
                        {
                            return;
                        }
                    }

                    _targetsReached++;
                    _targetTimestamps.Add(updates);
                }
            });
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
