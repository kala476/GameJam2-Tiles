using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Angler : MonoBehaviour
{
    public Transform anglingBall;
    public Camera mainCamera;
    int tileLayerMask;
    public AnimationCurve tileAttractiontoSqrDistance;
    public float maxAttractionForce;

    Coroutine attractionRoutine;
    TileBehaviour currentlyAttractedTile;
    // Start is called before the first frame update
    void Start()
    {
        tileLayerMask = LayerMask.GetMask("Tile");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0)) 
        {
            //raycast
            Vector2 ballScreenPoint = mainCamera.WorldToScreenPoint(anglingBall.position);
            Ray ray = mainCamera.ScreenPointToRay(ballScreenPoint);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f, tileLayerMask)) 
            {
                Collider hitCollider = hit.collider;
                GameObject gameObject = hitCollider.gameObject;
                TileBehaviour tile = hitCollider.GetComponent<TileBehaviour>();

                if (tile != null) 
                {
                    tile.pushedAwayFromPLayer = false;
                    tile.SetConstancForce(Vector3.zero);

                    currentlyAttractedTile = tile;
                    attractionRoutine = StartCoroutine(StartAttractingTile(currentlyAttractedTile));
                }

            }
        }
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            if (attractionRoutine != null && currentlyAttractedTile != null)
            {
                Debug.Log("stopping behaviour");
                StopCoroutine(attractionRoutine);
                ReturnTileToDefaultState(currentlyAttractedTile);
            }
        }
        
    }

    private IEnumerator StartAttractingTile(TileBehaviour tile) 
    {

        float maxTileDistance = 10f;
        float distanceTreshold = 1f;
        bool tileDocked = false;

        tile.SetColor(Color.yellow);

        while (tileDocked == false)
        {
            Vector3 vecToBall = new Vector3(tile.transform.position.x, tile.transform.position.y, tile.transform.position.z) - anglingBall.position;
            float disToPlayer = vecToBall.magnitude;
            // if the tile is far away, attract
            if (disToPlayer > distanceTreshold)
            {
                float forceValue = tileAttractiontoSqrDistance.Evaluate(disToPlayer / maxTileDistance) * maxAttractionForce;
                Vector3 forceVector = -vecToBall.normalized * forceValue;
                tile.SetConstancForce(forceVector);
            }
            else
            {
                // when tile is close enough create a temporary spring joint

                SpringJoint temporarySpringJoint = tile.gameObject.AddComponent<SpringJoint>();
                temporarySpringJoint.connectedBody = anglingBall.GetComponent<Rigidbody>();
                tileDocked = true;
            }
            yield return null;
        }
     }

    private void ReturnTileToDefaultState(TileBehaviour tile)
    {
        //remove spring joint
        SpringJoint temporarySpringJoint = tile.gameObject.GetComponent<SpringJoint>();
        if (temporarySpringJoint != null)
        {
            Component.Destroy(temporarySpringJoint);
        }


        //reset forces
        tile.pushedAwayFromPLayer = true;
        tile.SetConstancForce(Vector3.zero);
    }
}
