using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace XGD.TileQuest
{
    [Serializable]
    public class SolidCollection : ScriptableObject
    {
        public GameObject basePrefabTiles;
        public GameObject basePrefabSolids;
        public List<Solid> solids;
        public List<Tile> commonTiles;
        [HideInInspector] public string assetPath;
        public int solidCount { get { return solids.Count; } }
        public int tileCount { get { return commonTiles.Count; } }

        public Quaternion[,,] rotations = new Quaternion[0, 0, 0];

        public void Count(List<Tile> tilesProvided, out List<int> tileCounts, out List<Solid> buildableSolids)
        {
            int[] tilesFound = new int[tileCount];
            if (tilesProvided.TrueForAll(tile => commonTiles.Contains(tile)))
            {
                tileCounts = tilesFound.ToList();
                buildableSolids = null;
                return;
            }

            tilesProvided.ForEach(tile => tilesFound[commonTiles.IndexOf(tile)]++);
            tileCounts = tilesFound.ToList();
            buildableSolids = solids.Where(solid => Effordable(solid, tilesFound)).ToList();
        }

        public static bool Effordable(Solid solid, int[] provided)
        {
            List<int> needed = solid.tileCount;
            for (int i = 0; i < provided.Length; i++)
            {
                if (provided[i] < needed[i]) return false;
            }
            return true;
        }


        public void CreateSolids(string[] files)
        {
            solids = new List<Solid>();
            commonTiles = new List<Tile>();
            assetPath = AssetDatabase.GetAssetPath(this);

            files.ToList().ToList().ForEach(x => IncludeSolid(SolidFileBuffer.ReadFile(x)));
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

        public Tile IncludeTile(Vector2[] polygon)
        {
            Tile tile;
            List<Tile> options = commonTiles.Where(t => t.Match(polygon)).ToList();
            if (options.Count == 0)
            {
                tile = CreateInstance<Tile>();
                tile.AssignData(polygon);
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

        public GameObject Generate()
        {
            GameObject obj = new GameObject(name);

            List<Transform> soldInstances = solids.Select(x => x.Generate(this).transform).ToList();
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
    public class Solid : ScriptableObject
    {
        public List<Tile> tiles;
        public Vector3[] tilePositions;
        public Quaternion[] tileRotations;
        public List<int> tileCount;
        public float radialBounds;

        public bool Match(SolidFileBuffer buffer)
        {
            return buffer.name.Equals(name);
        }

        public static Vector2 Flatten(Vector3 point)
        {
            Vector3 flat = Vector3.ProjectOnPlane(point, Vector3.up);
            Vector2 plane = new Vector2(flat.x, flat.z);
            return plane;
        }

        public void AssignData(SolidFileBuffer buffer, SolidCollection solidCollection)
        {
            name = "Solid_" + buffer.name;
            List<List<int>> polys = buffer.polygonList;
            List<Vector3> verts = buffer.vertices;
            int tileCount = polys.Count;

            List<Tile> tileList = new List<Tile>();

            Vector3[] tilePos = new Vector3[tileCount];
            Quaternion[] tileRot = new Quaternion[tileCount];

            float largestRadius = 0.0f;

            for (int i = 0; i < polys.Count; i++)
            {
                int vertCount = polys[i].Count;
                Vector3 center = Vector3.zero;
                Vector3[] list = new Vector3[vertCount];
                Vector3[] tileVerts = new Vector3[vertCount * 3];
                int[] tileTris = new int[vertCount * 3];

                // Get tile verts from polylist
                for (int j = 0; j < list.Length; j++)
                {
                    Vector3 vert = verts[polys[i][j]];
                    center += vert;
                    list[j] = vert;
                }

                // Get tile origin from arithmetic mean of vertices
                center /= vertCount;

                // Optain 3d rotation of flat polygon
                Vector3 forward = list[0] - center;
                Vector3 right = list[1] - center;
                Vector3 up = Vector3.Cross(forward, right);
                Quaternion rot = Quaternion.LookRotation(forward, up);


                Vector2[] polygon = list.Select(x => Flatten(Quaternion.Inverse(rot) * (x - center))).ToArray();
                tileList.Add(solidCollection.IncludeTile(polygon));
                tilePos[i] = center;
                tileRot[i] = rot;
                largestRadius = Mathf.Max(largestRadius, center.magnitude);
            }

            // Assign Tile Data
            tilePositions = tilePos;
            tileRotations = tileRot;
            tiles = tileList;
            radialBounds = largestRadius;

        }

        public GameObject Generate(SolidCollection solidCollection)
        {
            GameObject obj = Instantiate(solidCollection.basePrefabSolids);
            obj.name = name;
            obj.GetComponent<SolidBehavior>().solid = this;

            List<Transform> tileInstances = tiles.Select(x => x.Generate(solidCollection).transform).ToList();

            for (int i = 0; i < tileInstances.Count; i++)
            {
                Transform tileInstance = tileInstances[i];
                tileInstance.localRotation = tileRotations[i];
                tileInstance.localPosition = tilePositions[i];
                tileInstance.parent = obj.transform;
            }
            return obj;
        }
    }

    [Serializable]
    public class Tile : ScriptableObject
    {
        public Mesh mesh;
        public Vector2[] polygon;

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
            if (id >= 0 && id < polygonNames.Length) return polygonNames[id];
            return "unknown";
        }

        public void AssignData(Vector2[] polygon)
        {
            string name = PolygonName(polygon.Length);
            this.polygon = polygon;
            this.name = "Tile_" + name;

            GenerateMesh();
            mesh.name = "Mesh_" + name;
        }

        public bool Match(Vector2[] polygon)
        {
            List<Vector2> reference = new List<Vector2>();
            List<Vector2> entry = new List<Vector2>();
            reference.AddRange(this.polygon);
            entry.AddRange(polygon);

            while (reference.Count > 0 && entry.Count > 0)
            {
                int id = reference.IndexOf(entry.First());

                if (id == -1)
                {
                    return false;
                }
                else
                {
                    reference.RemoveAt(id);
                    entry.RemoveAt(0);
                }
            }

            return true;
        }

        public GameObject Generate(SolidCollection solidCollection)
        {
            GameObject obj = Instantiate(solidCollection.basePrefabTiles);
            obj.name = name;
            obj.GetComponent<MeshFilter>().sharedMesh = mesh;
            obj.GetComponent<MeshCollider>().sharedMesh = mesh;
            return obj;
        }

        public void GenerateMesh()
        {
            float thickHalf = 0.05f;

            int n = polygon.Length;
            int vertexCount = n * 3 * 4;

            int[] tri = new int[vertexCount];
            Vector3[] v = new Vector3[vertexCount];
            Vector3[] nor = new Vector3[vertexCount];

            // Each iteration constructs a pie slice of a prism for each side of a flat polygon
            for (int k = 0; k < n; k++)
            {
                int next = (k + 1) % n;
                Vector3 left = new Vector3(polygon[k].x, 0.0f, polygon[k].y);
                Vector3 right = new Vector3(polygon[next].x, 0.0f, polygon[next].y);

                Vector3 ct = Vector3.up * thickHalf;
                Vector2 cb = -ct;
                Vector3 lt = left + ct;
                Vector3 rt = right + ct;
                Vector3 lb = left - ct;
                Vector3 rb = right - ct;
                //Vector3 mlt = Vector3.Lerp(lt, rt, 0.5f);
                //Vector3 mlb = Vector3.Lerp(lb, rb, 0.5f);
                //Vector3 mrt = Vector3.Lerp(lb, rb, 0.5f);
                //Vector3 mrb = Vector3.Lerp(lb, rb, 0.5f);

                int cap_t = (0 * n + k) * 3;
                int cap_b = (1 * n + k) * 3;
                int ring = (2 * n + k * 2) * 3;
                for (int i = cap_t; i < cap_t + 3; i++) tri[i] = i;
                for (int i = cap_b; i < cap_b + 3; i++) tri[i] = i;
                for (int i = ring; i < ring + 6; i++) tri[i] = i;
                v[cap_t + 0] = rt;
                v[cap_t + 1] = ct;
                v[cap_t + 2] = lt;
                v[cap_b + 0] = rb;
                v[cap_b + 1] = lb;
                v[cap_b + 2] = cb;
                v[ring + 0] = rt;
                v[ring + 1] = lt;
                v[ring + 2] = lb;
                v[ring + 3] = lb;
                v[ring + 4] = rb;
                v[ring + 5] = rt;
            }

            Mesh mesh = new Mesh()
            {
                vertices = v,
                triangles = tri,
                normals = nor
            };

            mesh.Optimize();
            mesh.RecalculateNormals();
            this.mesh = mesh;
        }
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

        public static SolidFileBuffer ReadFile(string filePath)
        {
            SolidFileBuffer buffer = new SolidFileBuffer();
            StreamReader reader = new StreamReader(filePath, Encoding.Default);

            string line;
            int lineIndex = 1;

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
            return buffer;
        }
    }



}