using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CavemanLand.Utility;


public class ArrayConverter
{

    public static T[] yearsArrayFor<T>(T[][,] array, int x, int z)
    {
        T[] arrayResult = new T[WorldDate.DAYS_PER_YEAR];

        for (int day = 0; day < WorldDate.DAYS_PER_YEAR; day++)
        {
            arrayResult[day] = array[day][x, z];
        }
        return arrayResult;
    }

}
