using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Polygon : ScriptableObject
{
    //Enumorators & Sub Classes  ----------------------------------------------------------------

    public enum Type { RegularTriangle, Square, RegularPentagon, RegularHexagon, RegularOctagon, RegularDecagon }


    [Serializable]
    public class Alignments
    {
        public string name;
        public List<Alignment> alignments;
    }

    [Serializable]
    public class Alignment
    {
        public string name;
        public Vector3 position;
        public Vector3 tangent;
        public Vector3 normal;
    }


    public class Integration
    {

    }


    // Instance Properties  ---------------------------------------------------------------------

    public Type polygonType;
    List<Alignment> alignments;

    public string Name
    {
        get
        {
            return polygonType.ToString();
        }
    }
           

    // Static Functions -------------------------------------------------------------------------

    public static string GetName(int i)
    {
        return Enum.GetName(typeof(Type), (Type)i);
    }

    public static int GetId(int vertexCount)
    {
        return vertexCountId[vertexCount];
    }

    public static int Count
    {
        get
        {
            return vertexCountId.Count;
        }
    }

    public static bool Exists(int vertexCount)
    {
        return vertexCountId.ContainsKey(vertexCount);
    }

    public static bool IsRegular(List<Vector3> vertices)
    {
        int count = vertices.Count;
        float[] edges = new float[count];
        edges[0] = (vertices[0] - vertices[count - 1]).magnitude;
        for (int i = 1; i < vertices.Count; i++)
        {
            edges[i] = (vertices[i] - vertices[i - 1]).magnitude;
        }

        for (int i = 0; i < edges.Length; i++)
        {
            if (edges[i] == 0)
            {
                return false;
            }
            else
            {
                for (int j = i; j < edges.Length; j++)
                {
                    if (Mathf.Abs(edges[i] - edges[j]) > 0.0001f)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    public static bool Exists(List<Vector3> vertices)
    {
        bool isRegular = IsRegular(vertices);

        return isRegular;
    }

    private static Dictionary<int, int> vertexCountId = new Dictionary<int, int>()
    {
        {3,0},
        {4,1},
        {5,2},
        {6,3},
        {8,4},
        {10,5}
    };



}
