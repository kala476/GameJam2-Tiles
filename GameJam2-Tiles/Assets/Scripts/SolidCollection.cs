using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public interface IGeneratable
{
    GameObject Generate(Material material);
}

[Serializable]
public class SolidFileBuffer
{
    public string name;
    public List<int[]> vertexIds;
    public List<string> polynomials;
    public List<List<int>> polygonList;
    public List<float> geometricValues;
    public List<Vector3> vertices;

    public SolidFileBuffer()
    {
        vertexIds = new List<int[]>();
        geometricValues = new List<float>();
        polynomials = new List<string>();
        polygonList = new List<List<int>>();
        vertices = new List<Vector3>();
    }
}


[Serializable]
public class SolidCollection : ScriptableObject, IGeneratable
{
    public GameObject prefab;
    public List<Solid> solids;
    public List<Tile> commonTiles;
    [HideInInspector] public string assetPath;
    public int solidCount { get { return solids.Count; } }
    public int tileCount  { get { return commonTiles.Count; } }

    public void CreateSolids(string[] files)
    {
        solids = new List<Solid>();
        commonTiles = new List<Tile>();
        assetPath = AssetDatabase.GetAssetPath(this);

        for (int i = 0; i < files.Length; i++)
        {
            string line;
            int lineIndex = 1;
            StreamReader reader = new StreamReader(files[i], Encoding.Default);
            SolidFileBuffer buffer = new SolidFileBuffer();

            //Read File ----------------------------------------------------------------------
            using (reader)
            {
                while ((line = reader.ReadLine()) != null)
                {

                    switch (Encoder.GetLineType(line, lineIndex))
                    {
                        case SolidFileLineType.None:
                            lineIndex++;
                            continue;

                        case SolidFileLineType.Name:
                            buffer.name = line;
                            lineIndex++;
                            continue;

                        case SolidFileLineType.Metric:
                            Encoder.ReadMetric(ref buffer, line);
                            lineIndex++;
                            continue;

                        case SolidFileLineType.Vertex:
                            Encoder.ReadVertex(ref buffer, line);
                            lineIndex++;
                            continue;

                        case SolidFileLineType.Face:
                            Encoder.ReadFace(ref buffer, line);
                            lineIndex++;
                            continue;
                    }
                }
            }
            reader.Close();

            //Write File ----------------------------------------------------------------------
            Solid solid = IncludeSolid(buffer);
            Encoder.Log(solid);
            //Encoder.Log(string.Format("{0} {1}", Encoder.ColorIn("Imported", "green"), buffer.name), buffer.polygonList.ToArray());
        }
    }

    private Solid IncludeSolid(SolidFileBuffer buffer)
    {
        Solid solid;
        List<Solid> options = solids.Where(s => s.Match(buffer)).ToList();
        if (options.Count == 0)
        {
            solid = CreateInstance<Solid>();
            solid.AssignData(buffer, this);
            AssetDatabase.AddObjectToAsset(solid, assetPath);
            solids.Add(solid);
        }
        else
        {
            solid = options.First();
        }
        return solid;
    }

    public Tile IncludeTile(Mesh mesh)
    {
        Tile tile;
        List<Tile> options = commonTiles.Where(t => t.Match(mesh)).ToList();
        if (options.Count == 0)
        {
            tile = CreateInstance<Tile>();
            tile.AssignData(mesh);
            string path = AssetDatabase.GetAssetPath(this);
            AssetDatabase.AddObjectToAsset(tile, path);
            string subpath = AssetDatabase.GetAssetPath(tile);
            AssetDatabase.AddObjectToAsset(tile.mesh, subpath);
            commonTiles.Add(tile);
        }
        else
        {
            tile = options.First();
        }
        return tile;
    }

    public GameObject Generate(Material mat)
    {
        GameObject obj = new GameObject(name);

        List<Transform> soldInstances = solids.Select(x => x.Generate(mat).transform).ToList();
        int w = Mathf.CeilToInt(Mathf.Sqrt(soldInstances.Count));
        float offset = 10.0f;        

        for (int i = 0; i < soldInstances.Count; i++)
        {
            Transform solidInstance = soldInstances[i];
            int v = i / w;
            int u = i % w;
            solidInstance.parent = obj.transform;
            solidInstance.localPosition = new Vector3(u * offset, 0.0f, v * offset);
        }

        return obj;
    }
}

[Serializable]
public class Tile : ScriptableObject, IGeneratable
{
    public Mesh mesh;

    public static string[] polygonNames = new string[]
    {
        "Triangle",
        "Quadrangle",
        "Pentagon",
        "Hexagon",
        "Heptagon",
        "Octagon"
    };

    public static string PolygonName(int vertCount)
    {
        int id = vertCount - 3;
        if (id >= 0 && id < polygonNames.Length)
        {
            return polygonNames[id];
        }
        else
        {
            return "unknown";
        }
    }

    public void AssignData(Mesh mesh)
    {
        name = "Tile_" + mesh.name;
        mesh.name = "Mesh_" + mesh.name;
        this.mesh = mesh;
    }

    public bool Match (Mesh compare)
    {
        int countA = mesh.vertexCount;
        int countB = compare.vertexCount;
        if (!countA.Equals(countB)) return false;

        Vector3[] vertsA = mesh.vertices;
        Vector3[] vertsB = compare.vertices;
        for (int i = 0; i < mesh.vertexCount; i++)
        {
            if (!Equals(vertsA[i],vertsB[i])) return false;
        }
        return true;
    }

    public GameObject Generate(Material mat)
    {
        GameObject obj = new GameObject(name);
        MeshRenderer mr = obj.AddComponent<MeshRenderer>();
        MeshFilter mf = obj.AddComponent<MeshFilter>();
        mr.sharedMaterial = mat;
        mf.sharedMesh = mesh;

        return obj;
    }
}

[Serializable]
public class TileCost : ScriptableObject
{
    public Tile tile;
    public int amount;
}


[Serializable]
public class Solid : ScriptableObject, IGeneratable
{
    public List<Tile> tiles;
    public Vector3[] tilePositions;
    public Quaternion[] tileRotations;

    public bool Match (SolidFileBuffer buffer)
    {
        return buffer.name.Equals(name);
    }

    public void AssignData (SolidFileBuffer buffer, SolidCollection tileCollection)
    {
        name = "Solid_" + buffer.name;
        List<List<int>> polys = buffer.polygonList;
        List<Vector3> verts = buffer.vertices;
        int tileCount = polys.Count;
        
        List<Tile> tileList = new List<Tile>();

        Vector3[] tilePos = new Vector3[tileCount];
        Quaternion[] tileRot = new Quaternion[tileCount];

        for (int i = 0; i < polys.Count; i++)
        {
            int vertCount = polys[i].Count;
            Vector3 center = Vector3.zero;
            Vector3[] list = new Vector3[vertCount];
            Vector3[] tileVerts = new Vector3[vertCount * 3];
            int[] tileTris = new int[vertCount * 3];

            Vector3 forward = verts[polys[i][0]].normalized;
            Vector3 right = verts[polys[i][1]].normalized;
            Vector3 up = Vector3.Cross(forward,right);
            Quaternion rot = Quaternion.LookRotation(forward, up);

            // Get tile verts from polylist
            for (int j = 0; j < list.Length; j++)
            {
                Vector3 vert = verts[polys[i][j]];
                center += vert;
                list[j] = vert;
            }

            // Get tile origin from arithmetic mean of vertices
            center /= vertCount;

            for (int j = 0; j < vertCount; j++)
            {
                int a = j;
                int b = (j + 1) % vertCount;
                tileTris[j * 3 + 0] = j * 3 + 0;
                tileTris[j * 3 + 1] = j * 3 + 1;
                tileTris[j * 3 + 2] = j * 3 + 2;
                tileVerts[j * 3 + 0] = Vector3.zero;
                tileVerts[j * 3 + 1] = Quaternion.Inverse(rot) * (list[a] - center);
                tileVerts[j * 3 + 2] = Quaternion.Inverse(rot) * (list[b] - center);
            }

            Mesh mesh = new Mesh()
            {
                name = Tile.PolygonName(vertCount),
                vertices = tileVerts,
                triangles = tileTris
            };
            mesh.RecalculateNormals();
            mesh.Optimize();

            Tile tile = tileCollection.IncludeTile(mesh);
            tileList.Add(tile);
            tilePos[i] = center;
            tileRot[i] = rot;
        }

        // Assign Tile Data
        tilePositions = tilePos;
        tileRotations = tileRot;
        tiles = tileList;
    }

    public GameObject Generate(Material mat)
    {
        GameObject obj = new GameObject(name);

        List<Transform> tileInstances = tiles.Select(x => x.Generate(mat).transform).ToList();

        for(int i = 0; i < tileInstances.Count; i++)
        {
            Transform tileInstance = tileInstances[i];
            tileInstance.localRotation = tileRotations[i];
            tileInstance.localPosition = tilePositions[i];
            tileInstance.parent = obj.transform;
        }

        return obj;
    }

}







