using GeneticNeuralNetworking.Genetics;
using Simulation.Genetics.Enhancing;
using Simulation.NeuralNetworking.Networking;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Simulation.Game.Ship
{
    public class ShipEnhancer : GeneticEnhancer<Ship>
    {
        public ShipEnhancer() : base(new NeuralNetworkMutator<Ship>(Ship.Settings))
        {
        }

        protected override IEnumerable<FitnessFunction<Ship>> FitnessFunctions
        {
            get
            {
                return new FitnessFunction<Ship>[]
                {
                    (s) =>
                    {
                        // Get to as many targets as possible
                        return new FitnessResult(s.TargetsReached*500);
                    },
                    (s) =>
                    {
                        // Get it as close as possible to the target
                        return new FitnessResult(-s.DistanceToTarget/(s.TargetsReached+1));
                    },
                    (s) =>
                    {
                        // Try to make ship as fast as possible between waypoints
                        int totalUpdates = 0;

                            s.TargetTimestamps.Aggregate((a,b) =>
                            {
                                totalUpdates += (b-a);
                                return b;
                            });

                        if (totalUpdates == 0)
                        {
                            totalUpdates = 1000;
                        }

                        return new FitnessResult(-(totalUpdates/s.TargetTimestamps.Count()));
                    },
                    (s)=>
                    {
                        // Try to keep neuron count as low as possible
                        return new FitnessResult(-s.NeuralNetwork.GetNeurons(NeuronType.Normal).Count()/10d);
                    }
                };
            }
        }
    }
}
