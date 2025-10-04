using System.Collections.Generic;

namespace CavemanLand.Utility
{
    public class ArrayPrinter
    {
        public static string printDoubleArray(double[,] array)
        {
            string output = "";
            for (int x = 0; x < array.GetUpperBound(0); x++)
            {
                for (int z = 0; z < array.GetUpperBound(1); z++)
                {
                    output += array[x, z];
                    if (z < array.GetUpperBound(1) - 1)
                    {
                        output += ", ";
                    }
                }
                output += "\n";
            }

            return output;
        }

        public static string printIntArray(int[,] array)
        {
            string output = "";
            for (int x = 0; x < array.GetUpperBound(0); x++)
            {
                for (int z = 0; z < array.GetUpperBound(1); z++)
                {
                    output += array[x, z];
                    if (z < array.GetUpperBound(1) - 1)
                    {
                        output += ", ";
                    }
                }
                output += "\n";
            }

            return output;
        }

        public static string printArrayOf<T>(T[,] array)
        {
            string output = "";
            for (int x = 0; x < array.GetUpperBound(0); x++)
            {
                for (int z = 0; z < array.GetUpperBound(1); z++)
                {
                    output += array[x, z];
                    if (z < array.GetUpperBound(1) - 1)
                    {
                        output += "(" + x + ", " + z + "):\n";
                        output += ", ";
                    }
                }
                output += "\n";
            }

            return output;
        }

        public static string printList<T>(List<T> list)
        {
            T[] array = list.ToArray();
            return string.Join(", ", array);
        }
    }
}
