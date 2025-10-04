using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using CavemanLand.Generators;
using CavemanLand.Models;
using CavemanLand.Utility;

public class WorldTemps
{
    // Temperature constants
    private const int LOW_TEMP_MIN = -25;
    private const int LOW_TEMP_MAX = 75;
    private const int HIGH_TEMP_MIN = 15;
    private const int HIGH_TEMP_MAX = 115;
    private const int TEMP_CHANGE_BY = 2;
    private const int STARTING_HIGH_TEMP_MAX = 90;
    private const int STARTING_HIGH_TEMP_MIN = 60;
    private const int STARTING_LOW_TEMP_MAX = 42;
    private const int STARTING_LOW_TEMP_MIN = 12;
    private const int SUMMER_LENGTH_MIN = 36;
    private const int SUMMER_LENGTH_MAX = 84;
    private const double VARIANCE_MIN = 0.0;
    private const double VARIANCE_MAX = 12.0;
    private const double VARIANCE_CHANGE_BY = 1.0;
    private const int STARTING_SUMMER_LENGTH_MAX = 60;
    private const int STARTING_SUMMER_LENGTH_MIN = 40;

    // Variables
    public int[,] highTemps;
    public int[,] lowTemps;
    public int[,] summerLengths;
    public double[,] variances;
    public TemperatureEquation[,] tempEquations;
    public int[][,] dailyTemps;
    public int[][,] lastYearsDailyTemps;

    private LayerGenerator layerGenerator;
    private LayerGenerator intLayerGenerator;
    private System.Random randy;

    public WorldTemps(LayerGenerator layerGenerator, LayerGenerator.mapPoles mapPole, Dictionary<string, int> requirements)
    {
        this.layerGenerator = layerGenerator;
        intLayerGenerator = new LayerGenerator(World.X, World.Z, 0);
        randy = new System.Random();

        while (!meetsRequirements(requirements))
        {
            Debug.Log("Creating Permanent Climates");
            lowTemps = generateLowTemps(mapPole);
            highTemps = generateHighTemps(mapPole, lowTemps);
            summerLengths = generateSummerLengths(mapPole);
            variances = generateVariance();
            Debug.Log("Creating Temperature Equations");
            tempEquations = populateTemperatureEquations();
        }

        // Generate CurrentYear of Temps
        dailyTemps = generateYearOfTemps();
    }

    public int[][][,] generatePrehistory()
    {
        // Generate NUM_OF_PREHISTORY_YEARS years of temps
        int[][][,] dailyTempsPreHistory = new int[World.NUM_OF_PREHISTORY_YEARS][][,];
        for (int y = 0; y < World.NUM_OF_PREHISTORY_YEARS; y++)
        {
            dailyTempsPreHistory[y] = generateYearOfTemps();
        }

        lastYearsDailyTemps = dailyTempsPreHistory[World.NUM_OF_PREHISTORY_YEARS - 1];
        return dailyTempsPreHistory;
    }

    public int[][,] generateYearOfTemps()
    {
        int[][,] dailyTemps = new int[WorldDate.DAYS_PER_YEAR][,];
        for (int day = 0; day < WorldDate.DAYS_PER_YEAR; day++)
        {
            dailyTemps[day] = new int[World.X, World.Z];
            for (int x = 0; x < World.X; x++)
            {
                for (int z = 0; z < World.Z; z++)
                {
                    dailyTemps[day][x, z] = tempEquations[x, z].getTodaysTemp(day);
                }
            }
        }

        return dailyTemps;
    }

    private int[,] generateLowTemps(LayerGenerator.mapPoles mapPole) {
        int startingValue = randy.Next(STARTING_LOW_TEMP_MIN, STARTING_LOW_TEMP_MAX);
        return intLayerGenerator.GenerateIntLayer(LOW_TEMP_MIN, LOW_TEMP_MAX, TEMP_CHANGE_BY, startingValue, false, mapPole);
    }

    private int[,] generateHighTemps(LayerGenerator.mapPoles mapPole, int[,] lowTemps)
    {
        int startingValue = randy.Next(STARTING_HIGH_TEMP_MIN, STARTING_HIGH_TEMP_MAX);
        return intLayerGenerator.GenerateIntLayer(HIGH_TEMP_MIN, HIGH_TEMP_MAX, TEMP_CHANGE_BY, startingValue, false, mapPole, lowTemps);
    }

    private int[,] generateSummerLengths(LayerGenerator.mapPoles mapPole)
    {
        int startingValue = randy.Next(STARTING_SUMMER_LENGTH_MIN, STARTING_SUMMER_LENGTH_MAX);
        return intLayerGenerator.GenerateIntLayer(SUMMER_LENGTH_MIN, SUMMER_LENGTH_MAX, TEMP_CHANGE_BY, startingValue, false, mapPole);
    }

    private double[,] generateVariance()
    {
        double startingValue = randy.NextDouble() * (VARIANCE_MAX - VARIANCE_MIN) + VARIANCE_MIN;
        return layerGenerator.GenerateWorldLayer(VARIANCE_MIN, VARIANCE_MAX, VARIANCE_CHANGE_BY, startingValue, false, LayerGenerator.mapPoles.None);
    }

    private TemperatureEquation[,] populateTemperatureEquations()
    {
        TemperatureEquation[,] tempEquations = new TemperatureEquation[World.X, World.Z];
        for (int x = 0; x < World.X; x++)
        {
            for (int z = 0; z < World.Z; z++)
            {
                tempEquations[x, z] = new TemperatureEquation(lowTemps[x, z], highTemps[x, z], summerLengths[x, z], variances[x, z]);
            }
        }

        return tempEquations;
    }

    private bool isTempAbove(Dictionary<string, int> requirements)
    {
        foreach (int temp in highTemps)
        {
            if (temp > requirements["high_above"]) { return true; }
        }
        return false;
    }

    private bool isTempBelow(Dictionary<string, int> requirements)
    {
        foreach (int temp in lowTemps)
        {
            if (temp < requirements["low_below"]) { return true; }
        }
        return false;
    }

    private bool meetsRequirements(Dictionary<string, int> requirements)
    {
        if (highTemps == null)
        {
            return false;
        }
        else
        {
            // return isTempAbove(requirements) && isTempBelow(requirements);
            return true;
        }
    }

}
