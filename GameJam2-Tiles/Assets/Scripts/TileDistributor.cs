using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileDistributor : MonoBehaviour
{
    [SerializeField] List<GameObject> tilePrefabs;
    public float minFloatY = 1;
    public float maxFloatY = 7;

    // Start is called before the first frame update
    void Start()
    {

    }

    private void OnEnable()
    {
        //ReplaceTiles();
        FloatHalf();
    }

    private void FloatHalf()
    {
        foreach (Transform child in this.transform)
        {
            int randInt = UnityEngine.Random.Range(0, 2);
            if (randInt == 1)
            {
                float floatHeight = UnityEngine.Random.Range(minFloatY, maxFloatY);
                child.transform.position = new Vector3(child.transform.position.x, floatHeight, child.transform.position.z);
                child.GetComponent<FloatingBehavior>().activateFloating = true;
                //Debug.LogError(floatHeight);
            }
            else
            {
                child.GetComponent<FloatingBehavior>().activateFloating = false;
            }
        }
    }

    private void ReplaceTiles()
    {
        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++) 
        { 
            Transform child = transform.GetChild(i);
            Vector3 childPosition = child.transform.position;
            GameObject newTile = tilePrefabs[UnityEngine.Random.Range(0, tilePrefabs.Count)];
            Destroy(child.gameObject);
            Instantiate(newTile, childPosition, newTile.transform.rotation, transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
