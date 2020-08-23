using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "DataStorage")]
public class DataAsset : ScriptableObject
{
    public Dataset dataset;

    public DataAsset()
    {
        dataset = new Dataset();
    }
}

public class Dataset
{
    public List<DataStorageElement> dse = new List<DataStorageElement>()
        {
            new DataStorageElement("Aa"),
            new DataStorageElement("Bb"),
            new DataStorageElement("Cc")
        };
}

[Serializable]
public class DataStorageElement
{
    [HideInInspector]
    public string name;

    public string value;

    public DataStorageElement(string name)
    {
        this.name = name;
    }
}
