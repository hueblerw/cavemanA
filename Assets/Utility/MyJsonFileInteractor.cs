using System.IO;
using UnityEngine;

namespace CavemanLand.Utility
{
	public class MyJsonFileInteractor
    {
		public static string loadSaveFileToString(string pathname)
        {
            return File.ReadAllText(pathname);
        }

        public static string loadDataFileToString(string pathname)
        {
            return Resources.Load(pathname).ToString();
        }

        public static void writeFileToPath(string filename, string json)
        {
            File.WriteAllText(filename, json);
        }
    }
}
