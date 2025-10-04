using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HashPrinter : MonoBehaviour
{
    
    public static string printHash<T1, T2>(string dictionaryName, Dictionary<T1, T2> hash)
    {
        string output = dictionaryName + ": \n";
        foreach(KeyValuePair<T1, T2> entry in hash)
        {
            output += entry.Key + ": " + entry.Value + "\n";
        }

        return output;
    }

}
