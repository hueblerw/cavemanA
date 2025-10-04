using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CavemanLand.Generators;
using CavemanLand.Models;
using CavemanLand.Utility;

public class WorldPrecipitation
{

    // Constants
    private const double HUMIDITY_MIN = 0.0;
    private const double HUMIDITY_MAX = 12.0;
    private const double HUMIDITY_CHANGE_BY = 3.0;
    private const double RAINFALL_MULTIPLIER = 4.5;
    private const int RAINFALL_POWER = 2;
    private const int HUMIDITY_LAYERS = 8;
    private const int MAX_REMAINDER = WorldDate.DAYS_PER_YEAR / HUMIDITY_LAYERS;
    private const double FLOW_RATE_MULT = 0.2;
    private const double MELT_CONST = ((3.0 * 1.8) / 25.4);
    private const double FREEZING_POINT = 32.0;

    // Variables
    public double[][,] humidities;
    public Direction.CardinalDirections[,] flowDirections;
    public List<Direction.CardinalDirections>[,] upstreamDirections;
    public double[,] flowRates;
    public double[][,] dailyPrecip;
    public double[][,] dailySnowfalls;
    public double[][,] dailySnowCover;
    public double[][,] dailySurfaceWater;
    public double[,] lastDayOfLastYearsDailySnowCover;
    public double[,] lastDayOfLastYearsDailySurfaceWater;
    public RiverBank[,] riverBanks;

    private LayerGenerator layerGenerator;
    private System.Random randy;

    public WorldPrecipitation(LayerGenerator layerGenerator, WorldTerrains terrains, int[][][,] dailyTempsPrehistory, Dictionary<string, int> requirements)
    {
        this.layerGenerator = layerGenerator;
        randy = new System.Random();

        while (!meetsRequirements(requirements))
        {
            Debug.Log("Creating Precipitation Patterns");
            humidities = generateHumidities();

            Debug.Log("Determining which way is downhill");
            flowDirections = calculateFlow(terrains.elevations, out flowRates);
            upstreamDirections = determineAllUpstreamDirections(flowDirections);

            Debug.Log("Generating first year of rain and snow");
            dailyPrecip = generateYearOfPrecipitation(dailyTempsPrehistory[0], out dailySnowfalls);
            lastDayOfLastYearsDailySnowCover = new double[World.X, World.Z];
            lastDayOfLastYearsDailySurfaceWater = new double[World.X, World.Z];
            dailySurfaceWater = calculateYearsSurfaceWaterData(dailyPrecip, dailySnowfalls, out dailySnowCover, lastDayOfLastYearsDailySnowCover, lastDayOfLastYearsDailySurfaceWater, terrains.oceanPercents, terrains.hillPercents, dailyTempsPrehistory[0]);
            riverBanks = createRiverBanks(dailySurfaceWater);
        }
    }

    public Dictionary<int, Dictionary<string, double[][,]>> generatePrehistory(WorldTerrains terrains, int[][][,] dailyTempsPrehistory, int[][,] dailyTemps)
    {
        Dictionary<int, Dictionary<string, double[][,]>> prehistory = new Dictionary<int, Dictionary<string, double[][,]>>();
        prehistory[0] = new Dictionary<string, double[][,]>();
        prehistory[0]["dailyPrecip"] = dailyPrecip;
        prehistory[0]["dailySnowfalls"] = dailySnowfalls;
        prehistory[0]["dailySnowCover"] = dailySnowCover;
        prehistory[0]["dailySurfaceWater"] = dailySurfaceWater;

        Debug.Log("Generating prehistory of rain and snow");
        for (int year = 1; year < World.NUM_OF_PREHISTORY_YEARS; year++)
        {
            generateYearOfWater(dailyTempsPrehistory[year], terrains);
            prehistory[year] = new Dictionary<string, double[][,]>();
            prehistory[year]["dailyPrecip"] = dailyPrecip;
            prehistory[year]["dailySnowfalls"] = dailySnowfalls;
            prehistory[year]["dailySnowCover"] = dailySnowCover;
            prehistory[year]["dailySurfaceWater"] = dailySurfaceWater;
        }

        generateYearOfWater(dailyTemps, terrains);
        return prehistory;
    }

    public void generateYearOfWater(int[][,] dailyTemps, WorldTerrains terrains)
    {
        advanceLastDayOfLastYearsStats(dailySnowCover[WorldDate.DAYS_PER_YEAR - 1], dailySurfaceWater[WorldDate.DAYS_PER_YEAR - 1]);
        dailyPrecip = generateYearOfPrecipitation(dailyTemps, out dailySnowfalls);
        dailySurfaceWater = calculateYearsSurfaceWaterData(dailyPrecip, dailySnowfalls, out dailySnowCover, lastDayOfLastYearsDailySnowCover, lastDayOfLastYearsDailySurfaceWater, terrains.oceanPercents, terrains.hillPercents, dailyTemps);
        erodeRiverBanks(dailySurfaceWater);
    }

    private void advanceLastDayOfLastYearsStats(double[,] lastSnowOfYear, double[,] lastWaterOfYear)
    {
        lastDayOfLastYearsDailySnowCover = lastSnowOfYear;
        lastDayOfLastYearsDailySurfaceWater = lastWaterOfYear;
    }

    private double[][,] generateHumidities()
    {
        double[][,] humidities = new double[HUMIDITY_LAYERS][,];
        for (int section = 0; section < HUMIDITY_LAYERS; section++)
        {
            double startingValue = randy.NextDouble() * (HUMIDITY_MAX - HUMIDITY_MIN) + HUMIDITY_MIN;
            humidities[section] = layerGenerator.GenerateWorldLayer(HUMIDITY_MIN, HUMIDITY_MAX, HUMIDITY_CHANGE_BY, startingValue, false, LayerGenerator.mapPoles.None);
        }

        return humidities;
    }

    private Direction.CardinalDirections[,] calculateFlow(double[,] elevations, out double[,] flowRates)
    {
        Direction.CardinalDirections[,] downstreams = new Direction.CardinalDirections[World.X, World.Z];
        flowRates = new double[World.X, World.Z];

        for (int x = 0; x < World.X; x++)
        {
            for (int z = 0; z < World.Z; z++)
            {
                Coordinates myPosition = new Coordinates(x, z);
                List<Direction.CardinalDirections> directionsAround = myPosition.getCardinalDirectionsAround();

                // set default values if all tiles around this tile are higher
                Direction.CardinalDirections flowTo = Direction.CardinalDirections.none;
                double lowest = elevations[x, z];

                if (lowest >= 0.0)
                {
                    foreach (Direction.CardinalDirections direction in directionsAround)
                    {
                        Coordinates coor = myPosition.findCoordinatesInCardinalDirection(direction);
                        if (elevations[coor.x, coor.z] < lowest)
                        {
                            lowest = elevations[coor.x, coor.z];
                            flowTo = direction;
                        }
                    }
                }

                downstreams[x, z] = flowTo;
                flowRates[x, z] = calculateFlowRate(flowTo, elevations[x, z], lowest);
            }
        }

        return downstreams;
    }

    public double[][,] generateYearOfPrecipitation(int[][,] dailyTemps, out double[][,] dailySnowfalls)
    {
        double[][,] dailyPrecips = new double[WorldDate.DAYS_PER_YEAR][,];
        dailySnowfalls = new double[WorldDate.DAYS_PER_YEAR][,];
        for (int day = 0; day < WorldDate.DAYS_PER_YEAR; day++)
        {
            double [,] snowfall;
            dailyPrecips[day] = generateDayOfPrecipitation(day, dailyTemps[day], out snowfall);
            dailySnowfalls[day] = snowfall;
        }

        return dailyPrecips;
    }

    private double[,] generateDayOfPrecipitation(int day, int[,] todaysDailyTemps, out double[,] snowfall)
    {
        double startingValue = randy.NextDouble() * HUMIDITY_MAX;
        double[,] dailyRandomMap = layerGenerator.GenerateWorldLayer(0.0, HUMIDITY_MAX, RAINFALL_MULTIPLIER, startingValue, true, LayerGenerator.mapPoles.None);
        double[,] precips = new double[World.X, World.Z];
        snowfall = new double[World.X, World.Z];
        for (int x = 0; x < World.X; x++)
        {
            for (int z = 0; z < World.Z; z++)
            {
                double humidityDiff = (dailyRandomMap[x, z] + getHumidity(day, x, z) - HUMIDITY_MAX) / HUMIDITY_MAX;
                if (humidityDiff > 0.0)
                {
                    double precipitation = Math.Round(Math.Pow(humidityDiff, RAINFALL_POWER) * RAINFALL_MULTIPLIER, World.ROUND_TO);
                    if (todaysDailyTemps[x, z] > FREEZING_POINT)
                    {
                        precips[x, z] = precipitation;
                    }
                    else
                    {
                        snowfall[x, z] = precipitation;
                    }
                }
            }
        }

        return precips;
    }

    private double[][,] calculateYearsSurfaceWaterData(double[][,] rainDays, double[][,] snowfall, out double[][,] snowCover, double[,] lastSnowOfYear, double[,] lastWaterOfYear, double[,] oceanPercents, double[,] hillPercents, int[][,] dailyTemps)
    {
        double[][,] surfaceWater = new double[WorldDate.DAYS_PER_YEAR][,];
        snowCover = new double[WorldDate.DAYS_PER_YEAR][,];
        for (int day = 1; day <= WorldDate.DAYS_PER_YEAR; day++)
        {
            surfaceWater[day - 1] = new double[World.X, World.Z];
            snowCover[day - 1] = new double[World.X, World.Z];
            for (int x = 0; x < World.X; x++)
            {
                for (int z = 0; z < World.Z; z++)
                {
                    if (oceanPercents[x, z] < 1.0)
                    {
                        double yesterdaysSnow;
                        double yesterdaysWater;

                        if (day == 1)
                        {
                            yesterdaysSnow = lastSnowOfYear[x, z];
                            yesterdaysWater = lastWaterOfYear[x, z];
                        }
                        else
                        {
                            yesterdaysSnow = snowCover[WorldDate.getYesterday(day) - 1][x, z];
                            yesterdaysWater = surfaceWater[WorldDate.getYesterday(day) - 1][x, z];
                        }

                        int todaysTemp = dailyTemps[day - 1][x, z];

                        double snowMelt = Math.Min(Math.Max((todaysTemp - FREEZING_POINT) * MELT_CONST, 0.0), yesterdaysSnow);
                        snowCover[day - 1][x, z] = Math.Round(Math.Max(yesterdaysSnow + snowfall[day - 1][x, z] - snowMelt, 0.0), World.ROUND_TO);
                        double humid = getHumidity(day - 1, x, z);
                        double flowAway = 0.0;
                        if (flowDirections[x, z] != Direction.CardinalDirections.none)
                        {
                            flowAway = flowRates[x, z];
                        }

                        double waterRemoved = calculateSoilAbsorption(oceanPercents[x, z], hillPercents[x, z]) + flowAway + calculateEvaporation(yesterdaysWater, todaysTemp, humid, determineWeather(rainDays[day - 1][x, z], humid));
                        double waterGained = rainDays[day - 1][x, z] + getUpstreamFlow(day, x, z, surfaceWater, lastWaterOfYear) + snowMelt;
                        surfaceWater[day - 1][x, z] = Math.Round(Math.Max(yesterdaysWater - waterRemoved + waterGained, 0.0), World.ROUND_TO);
                    }
                }
            }
        }
        return surfaceWater;
    }

    private List<Direction.CardinalDirections>[,] determineAllUpstreamDirections(Direction.CardinalDirections[,] downstreamDirections)
    {
        List<Direction.CardinalDirections>[,] upstreams = new List<Direction.CardinalDirections>[World.X, World.Z];
        for (int x = 0; x < World.X; x++)
        {
            for (int z = 0; z < World.Z; z++)
            {
                upstreams[x, z] = getUpstreamFromDownstream(x, z, downstreamDirections);
            }
        }

        return upstreams;
    }

    private List<Direction.CardinalDirections> getUpstreamFromDownstream(int x, int z, Direction.CardinalDirections[,] downstreamDirections)
    {
        List<Direction.CardinalDirections> upstream = new List<Direction.CardinalDirections>();
        Coordinates myPosition = new Coordinates(x, z);
        List<Direction.CardinalDirections> directionAroundMe = myPosition.getCardinalDirectionsAround();
        foreach (Direction.CardinalDirections direction in directionAroundMe)
        {
            Coordinates coor = myPosition.findCoordinatesInCardinalDirection(direction);
            if (Direction.isOpposite(downstreamDirections[coor.x, coor.z], direction))
            {
                upstream.Add(direction);
            }
        }

        return upstream;
    }

    private double calculateSoilAbsorption(double oceanPercent, double hillPercent)
    {
        return ((randy.NextDouble() + .2) * (1.0 - oceanPercent) * (1.0 - hillPercent));
    }

    private double calculateEvaporation(double currentWater, int temp, double humidity, string weather)
    {
        double xs;
        double x;
        double pws;
        double pw;
        double evaporation;
        double multiplier = 1.0;

        switch (weather)
        {
            case "cloudy":
                multiplier = 16.0;
                break;
            case "sunny":
                multiplier = 48.0;
                break;
        }
        pws = Math.Exp(77.345 + 0.0057 * ((temp + 459.67) * (5.0 / 9.0)) - 7235.0 / ((temp + 459.67) * (5.0 / 9.0))) / (Math.Pow((temp + 459.67) * (5.0 / 9.0), 8.2));
        xs = 0.62198 * pws / (101325.0 - pws);
        if (weather != "rainy")
        {
            pw = (humidity / multiplier) * pws;
        }
        else
        {
            pw = pws;
        }
        x = 0.62198 * pw / (101325.0 - pw);
        evaporation = (float)(((1260.0 * 24.0 * Math.Sqrt(2.0 * currentWater * 23.6) / Math.Pow(6.0, .25)) * (xs - x)) / 23600.0);

        return evaporation;
    }

    private string determineWeather(double rainfall, double humidity)
    {
        string weather;
        if (rainfall > 0)
        {
            weather = "rainy";
        }
        else
        {
            weather = determineCloudy(humidity);
        }

        return weather;
    }

    // Determine if it is cloudy or not
    private string determineCloudy(double humidity)
    {
        string weather;
        double prob = Math.Pow(0.5, humidity / 5.0);

        if (randy.NextDouble() < prob)
        {
            weather = "sunny";
        }
        else
        {
            weather = "cloudy";
        }

        return weather;
    }

    private double getUpstreamFlow(int day, int x, int z, double[][,] surfaceWater, double[,] lastWaterOfYear)
    {
        double upstreamFlow = 0.0;
        List<Direction.CardinalDirections> directions = upstreamDirections[x, z];
        Coordinates myPosition = new Coordinates(x, z);
        foreach (Direction.CardinalDirections direction in directions)
        {
            Coordinates coordinates = myPosition.findCoordinatesInCardinalDirection(direction);
            double flow = flowRates[coordinates.x, coordinates.z];
            double yesterdayUpstreamWater;

            if (day == 1)
            {
                yesterdayUpstreamWater = lastWaterOfYear[coordinates.x, coordinates.z];
            }
            else
            {
                yesterdayUpstreamWater = surfaceWater[WorldDate.getYesterday(day) - 1][coordinates.x, coordinates.z];
            }

            if (yesterdayUpstreamWater > flow)
            {
                upstreamFlow += flow;
            }
            else
            {
                upstreamFlow += yesterdayUpstreamWater;
            }
        }

        return upstreamFlow;
    }

    private double calculateFlowRate(Direction.CardinalDirections flowTo, double elevation, double lowest)
    {
        if (flowTo == Direction.CardinalDirections.none)
        {
            return 0.0;
        } else
        {
            return Math.Round((elevation - lowest) * FLOW_RATE_MULT, World.ROUND_TO);
        }
    }

    private double getHumidity(int day, int x, int z)
    {
        int section = (int) day / MAX_REMAINDER;
        int remainder = day % MAX_REMAINDER;
        int nextSection = section + 1;
        if (section == (HUMIDITY_LAYERS - 1))
        {
            nextSection = 0;
        }

        double maxRemainder = (double) MAX_REMAINDER;
        return Math.Round(humidities[section][x, z] * ((MAX_REMAINDER - remainder) / maxRemainder) + humidities[nextSection][x, z] * (remainder / maxRemainder), World.ROUND_TO);
    }

    private void erodeRiverBanks(double[][,] dailySurfaceWater)
    {
        for (int x = 0; x < World.X; x++)
        {
            for(int z = 0; z < World.Z; z++)
            {
                double[] surfaceWater = ArrayConverter.yearsArrayFor<double>(dailySurfaceWater, x, z);
                riverBanks[x, z].erodeBanks(surfaceWater);
            }
        }
    }

    private RiverBank[,] createRiverBanks(double[][,] dailySurfaceWater)
    {
        RiverBank[,] riverBanks = new RiverBank[World.X, World.Z];
        for (int x = 0; x < World.X; x++)
        {
            for (int z = 0; z < World.Z; z++)
            {
                double[] surfaceWater = ArrayConverter.yearsArrayFor<double>(dailySurfaceWater, x, z);
                riverBanks[x, z] = new RiverBank(surfaceWater);
            }
        }

        return riverBanks;
    }

    private bool meetsRequirements(Dictionary<string, int> requirements)
    {
        if (humidities == null)
        {
            return false;
        } else
        {
            return true;
        }
    }

}
