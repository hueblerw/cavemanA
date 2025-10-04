using System;
using System.Collections.Generic;
using UnityEngine;
using CavemanLand.Generators;
using CavemanLand.Models;
using CavemanLand.Utility;

namespace CavemanLand.Models
{
    public class WorldTerrains
    {
        // Elevation constants
        private const double STARTING_ELE_RANGE = 10.0;
        private const double STARTING_ELE_MIN = -5.0;
        private const double ELE_CHANGE_BY = 2.0;

        // Variables
        public double landPercentage;
        public double[,] elevations;
        public double[,] hillPercents;
        public double[,] oceanPercents;
        public Minerals[,] minerals;

        private LayerGenerator layerGenerator;
        private double maxDiff;

        public WorldTerrains(LayerGenerator layerGenerator, double[] landPercentageRestrictions, List<string> requiredMinerals)
        {
            this.layerGenerator = layerGenerator;

            while (!meetsRequirements(landPercentageRestrictions, requiredMinerals))
            {
                Debug.Log("Creating Elevations");
                elevations = generateElevations();
                maxDiff = calculateMaxDiff(elevations);
                Debug.Log("Calculating Ocean Percents");
                oceanPercents = calculateOceanPercents(elevations);
                Debug.Log("Calculating Hill Percents");
                hillPercents = calculateHillPercents(elevations, oceanPercents);
                Debug.Log("Generating Minerals");
                minerals = generateMinerals(oceanPercents, hillPercents);
                Debug.Log("Estimating the Total Land %:");
                landPercentage = estimateLand(oceanPercents);
                Debug.Log(landPercentage * 100.0 + "%");
            }
        }

        public double[,] generateElevations()
        {
            System.Random randy = new System.Random();
            double startingValue = randy.NextDouble() * STARTING_ELE_RANGE + STARTING_ELE_MIN;
            return layerGenerator.GenerateWorldLayer(World.UNLIMITED_MIN, World.UNLIMITED_MAX, ELE_CHANGE_BY, startingValue, true, LayerGenerator.mapPoles.None);
        }

        // Note this only returns accessible minerals above the Ocean:
        public List<string> getAllMineralsInWorld()
        {
            List<string> worldMinerals = new List<string>();
            for (int x = 0; x < World.X; x++)
            {
                for (int z = 0; z < World.Z; z++)
                {
                    if(oceanPercents[x, z] < 1.0)
                    {
                        Dictionary<string, double>.KeyCollection currentMineralNames = minerals[x, z].mineable.Keys;
                        foreach (string name in currentMineralNames)
                        {
                            if (!worldMinerals.Contains(name))
                            {
                                worldMinerals.Add(name);
                            }
                        }
                    }
                }
            }
            return worldMinerals;
        }

        private double calculateMaxDiff(double[,] elevations)
        {
            double max = 0.0;
            for (int x = 0; x < World.X; x++)
            {
                for (int z = 0; z < World.Z; z++)
                {
                    Coordinates coord = new Coordinates(x, z);
                    List<Coordinates> coorAround = coord.getCoordinatesAround();
                    double diffSum = 0.0;
                    foreach (Coordinates coor in coorAround)
                    {
                        diffSum += Math.Abs(elevations[coord.x, coord.z] - elevations[coor.x, coor.z]);
                    }
                    if (diffSum > max)
                    {
                        max = diffSum;
                    }
                }
            }
            return Math.Round(max, World.ROUND_TO);
        }

        private double[,] calculateOceanPercents(double[,] elevations)
        {
            double[,] oceanPercents = new double[World.X, World.Z];
            for(int x = 0; x < World.X; x++)
            {
                for(int z = 0; z < World.Z; z++)
                {
                    Coordinates coordinates = new Coordinates(x, z);
                    oceanPercents[x, z] = calculateOceanPercentage(coordinates, elevations);
                }
            }
            return oceanPercents;
        }

        private double[,] calculateHillPercents(double[,] elevations, double[,] oceanPercents)
        {
            double[,] hillPercents = new double[World.X, World.Z];
            for (int x = 0; x < World.X; x++)
            {
                for (int z = 0; z < World.Z; z++)
                {
                    Coordinates coordinates = new Coordinates(x, z);
                    hillPercents[x, z] = calculateHillPercentage(coordinates, elevations, oceanPercents[x, z]);
                }
            }
            return hillPercents;
        }

        private double calculateOceanPercentage(Coordinates coordinates, double[,] elevations)
        {
            List<Coordinates> coorAround = coordinates.getCoordinatesAround();
            double sum = 0.0;
            double negSum = 0.0;
            foreach (Coordinates coor in coorAround)
            {
                double current = elevations[coor.x, coor.z];
                if (current < 0.0)
                {
                    negSum += current;
                }
                sum += Math.Abs(current);
            }
            return Math.Round(Math.Abs(negSum / sum), World.ROUND_TO);
        }

        private double calculateHillPercentage(Coordinates coordinates, double[,] elevations, double oceanPer)
        {
            if (oceanPer >= 1.00)
            {
                return 0.0;
            }
            List<Coordinates> coorAround = coordinates.getCoordinatesAround();
            double diffSum = 0.0;
            foreach (Coordinates coor in coorAround)
            {
                double current = elevations[coor.x, coor.z];
                diffSum += Math.Abs(elevations[coordinates.x, coordinates.z] - current);
            }
            return Math.Round(Math.Abs(diffSum / maxDiff), World.ROUND_TO);
        }

        private Minerals[,] generateMinerals(double[,] oceanPercents, double[,] hillPercents)
        {
            Minerals[,] minerals = new Minerals[World.X, World.Z];
            for (int x = 0; x < World.X; x++)
            {
                for (int z = 0; z < World.Z; z++)
                {
                    minerals[x, z] = new Minerals(oceanPercents[x, z], hillPercents[x, z]);
                }
            }
            return minerals;
        }

        private double estimateLand(double[,] oceanPercents)
        {
            double average = 0.0;
            for (int x = 0; x < World.X; x++)
            {
                for (int z = 0; z < World.Z; z++)
                {
                    average += oceanPercents[x, z];
                }
            }
            return 1.0 - Math.Round(average / ((double) (World.X * World.Z)), 4);
        }

        // can i make this generalized so i just pass in the logic as a lambda?
        private bool meetsRequirements(double[] landPercentageRestrictions, List<string> requiredMinerals)
        {
            if(elevations == null)
            {
                return false;
            }
            else
            {
                // acceptable terrain requirement logic goes here:
                bool isLegal = landPercentage >= landPercentageRestrictions[0] && landPercentage <= landPercentageRestrictions[1];
                foreach(string mineral in requiredMinerals)
                {
                    isLegal = isLegal && getAllMineralsInWorld().Contains(mineral);
                }

                // log and return the result
                if (!isLegal)
                {
                    Debug.Log("Terrain Requirement was not met trying again!");
                    return false;
                }
                return true;
            }
        }
    }
}
