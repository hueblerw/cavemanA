using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using CavemanLand.Models;
using CavemanLand.Utility;

public class WorldHabitats
{
    public Habitats[,] habitats;

    public WorldHabitats(double[,] oceanPercents, int[][][,] tempsPreHistory, Dictionary<int, Dictionary<string, double[][,]>> rainPreHistory)
    {
        Debug.Log("Creating World Habitats");
        habitats = generateHabitats(oceanPercents, tempsPreHistory, rainPreHistory);
    }

    public Dictionary<string, double> getWorldHabitatDistribution()
    {
        double[] habitatSums = calculateCurrentHabitatPercentages(habitats);
        Debug.Log("Habitat Distributions: \n" + ArrayPrinter.printList<double>(habitatSums.ToList()));
        Dictionary<int, string> mapping = Habitats.habitatMapping;
        Dictionary<string, double> distribution = new Dictionary<string, double>();
        for(int i = 0; i < habitatSums.Length; i++)
        {
            distribution[mapping[i]] = Math.Round(habitatSums[i], World.ROUND_TO);
        }

        return distribution;
    }

    public void growHabitat(Habitats habitat, int x, int z, double oceanPercent, int[][,] dailyTemps, double[][,] dailySnowCover, double[][,] dailyPrecipitation, double[][,] dailySurfaceWater)
    {
        if (oceanPercent < 1.0)
        {
            bool isIceSheet = isAlwaysSnowCovered(ArrayConverter.yearsArrayFor<double>(dailySnowCover, x, z));
            if (isIceSheet)
            {
                habitat.growHabitats(new int[WorldDate.DAYS_PER_YEAR], 0.0, 0.0, isIceSheet);
            } else
            {
                int[] todaysTemps = ArrayConverter.yearsArrayFor<int>(dailyTemps, x, z);
                double totalPrecipitation = ArrayConverter.yearsArrayFor<double>(dailyPrecipitation, x, z).Sum();
                double avgRiverLevel = ArrayConverter.yearsArrayFor<double>(dailySurfaceWater, x, z).Average();
                habitat.growHabitats(todaysTemps, totalPrecipitation, avgRiverLevel, isIceSheet);
            }
        }
    }

    private Habitats[,] generateHabitats(double[,] oceanPercents, int[][][,] tempsPreHistory, Dictionary<int, Dictionary<string, double[][,]>> rainPreHistory)
    {
        Habitats[,] habitats = new Habitats[World.X, World.Z];
        for (int x = 0; x < World.X; x++)
        {
            for (int z = 0; z < World.Z; z++)
            {
                double oceanPercent = oceanPercents[x, z];
                habitats[x, z] = new Habitats(oceanPercent);
                // then loop through the prehistory to update the habitats to their current level
                for (int year = 0; year < World.NUM_OF_PREHISTORY_YEARS; year++)
                {
                    growHabitat(habitats[x, z], x, z, oceanPercent, tempsPreHistory[year], rainPreHistory[year]["dailySnowCover"], rainPreHistory[year]["dailyPrecip"], rainPreHistory[year]["dailySurfaceWater"]);
                }
            }
        }

        return habitats;
    }

    private bool isAlwaysSnowCovered(double[] dailySnowCover)
    {
        foreach(double snowCover in dailySnowCover)
        {
            if (snowCover == 0.0)
            {
                return false;
            }
        }

        return true;
    }

    private double[] calculateCurrentHabitatPercentages(Habitats[,] habitats)
    {
        double[] typePercentAverages = new double[Habitats.NUMBER_OF_HABITATS];

        for (int x = 0; x < World.X; x++)
        {
            for (int z = 0; z < World.Z; z++)
            {
                typePercentAverages = sumArrayAtSameIndex(typePercentAverages, habitats[x, z].typePercents);
            }
        }

        return divideEachElementBy(typePercentAverages, World.X * World.Z);
    }

    private double[] sumArrayAtSameIndex(double[] typePercentAverages, int[] typePercents)
    {
        for(int i = 0; i < typePercentAverages.Length; i++)
        {
            typePercentAverages[i] += typePercents[i];
        }
        return typePercentAverages;
    }

    private double[] divideEachElementBy(double[] typePercentAverages, int divisor)
    {
        for (int i = 0; i < typePercentAverages.Length; i++)
        {
            typePercentAverages[i] = typePercentAverages[i] / divisor;
        }
        return typePercentAverages;
    }
}
