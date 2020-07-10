using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileDistributor : MonoBehaviour
{
    [SerializeField] List<GameObject> tilePrefabs;

    // Start is called before the first frame update
    void Start()
    {

    }

    private void OnEnable()
    {
        ReplaceTiles();
    }

    private void ReplaceTiles()
    {
        int childCount = this.transform.childCount;
        for (int i = 0; i < childCount; i++) 
        {
            Transform child = this.transform.GetChild(i);
            Vector3 childPosition = child.transform.position;
            GameObject newTile = tilePrefabs[UnityEngine.Random.Range(0, tilePrefabs.Count)];
            Destroy(child.gameObject);
            Instantiate<GameObject>(newTile, childPosition, newTile.transform.rotation, this.transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
