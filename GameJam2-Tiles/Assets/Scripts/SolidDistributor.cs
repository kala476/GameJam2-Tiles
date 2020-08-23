using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class SolidDistributor : MonoBehaviour
{
    public Transform rootPoolSolids;
    public Transform rootPoolTiles;

    // Options
    public SolidCollection solidCollection;
    public AnimationCurve typDistribution = new AnimationCurve();
    public int instanceCount;

    // Commands
    public bool scatterSolids;
    public bool genrateTiles;


    public int[] pointIndexMap;
    public Vector3[] points = new Vector3[0];
    public GameObject[] pooledTiles;
    public GameObject[] pooledSolids;
    public GameObject[] activeSolids;
    public GameObject[] activeTiles;

    private SolidCollection m_solidCollection;
    private AnimationCurve m_typDistribution = new AnimationCurve();
    private int m_instanceCount;

    private void OnValidate()
    {        
        bool changed_typDistribution = !m_typDistribution.Equals(typDistribution);
        bool changed_instanceCount = !m_instanceCount.Equals(instanceCount);
        bool changed_solidCollection = !m_solidCollection.Equals(solidCollection);

        if (changed_typDistribution)
        {
            m_typDistribution = typDistribution;
        }

        if (changed_instanceCount)
        {
            m_instanceCount = instanceCount;
        }

        if (changed_solidCollection)
        {
            m_solidCollection = solidCollection;
        }
    }

    void Update()
    {


    }

    void UpdatePoints()
    {
        int[] pointIndices = new int[instanceCount];
        pointIndices = pointIndices.Select(x => Mathf.RoundToInt(Random.value * transform.childCount)).ToArray();
        Vector3[] p = pointIndices.Select(x => transform.GetChild(x).position).ToArray();
        points = p;
    }

    void ScatterSolids()
    {
        int[] instanceMap;
        float[] typeDensity = new float[solidCollection.solidCount];
        float totalTypeDistributionMass = 0.0f;
        for (int i = 0; i < typeDensity.Length; i++)
        {
            float t = i/typeDensity.Length;
            float value = typDistribution.Evaluate(t);
            totalTypeDistributionMass += value;
            typeDensity[i] = value;
        }
    }

    void CreateSoldPool()
    {
        Transform root = rootPoolSolids;
        DestroyChildren(root);

    }

    void CreateTilePool()
    {
        Transform root = rootPoolTiles;
        DestroyChildren(root);

        for (int i = 0; i < solidCollection.tileCount; i++)
        {
            Tile tile = solidCollection.commonTiles[i];
            GameObject obj = new GameObject();
            MeshRenderer mr = obj.AddComponent<MeshRenderer>();
            MeshFilter mf = obj.AddComponent<MeshFilter>();
            mf.sharedMesh = tile.mesh;
            mr.sharedMaterial = new Material(Shader.Find("Standard"));
        }
    }

    void DestroyChildren(Transform parent)
    {
        IEnumerable<Transform> children = parent.GetComponentsInChildren<Transform>().Where(x => x != parent);
        GameObject[] toDestroy = children.Select(x => x.gameObject).ToArray();

        for (int i = 0; i < toDestroy.Length; i++)
        {
            DestroyImmediate(toDestroy[i]);
        }
    }

    
}
