using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using CavemanLand.Models;
using CavemanLand.Utility;


public class RiverBank
{
    public double dryBedDepth;
    private const double FLOOD_STAGE_PERCENTILE = 0.8;
    private const double DROUGHT_STAGE_PERCENTILE = 0.4;
    private const double FLOOD_EROSION_RATE = 0.05;
    private const double DROUGHT_EROSION_RATE = 0.02;


    public RiverBank(double[] yearOfSurfaceWater)
    {
        Array.Sort(yearOfSurfaceWater);
        int percentile80Index = (int)FLOOD_STAGE_PERCENTILE * WorldDate.DAYS_PER_YEAR;
        dryBedDepth = findDepthFromVolume(yearOfSurfaceWater[percentile80Index]);
    }


    public void erodeBanks(double[] yearOfSurfaceWater)
    {
        double change = 0.0;
        Array.Sort(yearOfSurfaceWater);
        // Carve more depth
        int floodStageIndex = Array.IndexOf(yearOfSurfaceWater, dryBedDepth);
        if (floodStageIndex < WorldDate.DAYS_PER_YEAR && floodStageIndex >= 0)
        {
            for (int i = floodStageIndex; i < WorldDate.DAYS_PER_YEAR; i++)
            {
                change += (yearOfSurfaceWater[i] - dryBedDepth) * FLOOD_EROSION_RATE;
            }
        }
        // Fill in empty bed
        int droughtStageIndex = Array.IndexOf(yearOfSurfaceWater, dryBedDepth * DROUGHT_STAGE_PERCENTILE);
        if (droughtStageIndex > 0)
        {
            for (int i = 0; i < droughtStageIndex; i++)
            {
                change -= (dryBedDepth - yearOfSurfaceWater[i]) * DROUGHT_EROSION_RATE;
            }
        }

        dryBedDepth += change;
    }


    // Given a volume V and a river length L in feet this equation gives the SA available for evaporation:
    // SA = 6 * SQRT(6 * V * L) / L
    // So we want the height H.  H is defined as 1/3 of the river width.
    // So given the river is of length L => SA = L * 3 * H
    // So H = SA / (3 * L) => H = 2 * SQRT( 6 * V * L) / L ^ 2
    // L is the length of the river which is simplified as being the length of the tile.
    private double findDepthFromVolume(double volume)
    {
        int length = World.TILE_SIDE_LENGTH;
        return Math.Round((2.0 * Math.Sqrt(6 * volume * length)) / Math.Pow(length, 2), World.ROUND_TO);
    }


    // Given H = 2 * SQRT( 6 * V * L) / L ^2
    // Then V = (H ^ 2 * L ^ 3) / 24
    private double getFloodVolumeFromRiverBedDepth(double riverBedDepth)
    {
        int length = World.TILE_SIDE_LENGTH;
        return (Math.Pow(riverBedDepth, 2) * Math.Pow(length, 3)) / 24.0;
    }

    public override string ToString()
    {
        return dryBedDepth.ToString();
    }

}
