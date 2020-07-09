using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class Angler : MonoBehaviour
{
    public Transform anglingBall;
    public Camera mainCamera;
    int tileLayerMask;
    public AnimationCurve tileAttractiontoSqrDistance;
    public float maxAttractionForce;
    public float anglingRadius;
    public bool attractingTiles;

    List<TileBehaviour> dockedTiles = new List<TileBehaviour>();
    List<Coroutine> runningAttractions = new List<Coroutine>();
    TileBehaviour[] tilesInRadiusLastFrame;
    TileBehaviour[] tilesInRadiusThisFrame;

    // Start is called before the first frame update
    void Start()
    {
        tileLayerMask = LayerMask.GetMask("Tile");
    }

    // Update is called once per frame
    void Update()
    {
        //get Tiles that can be fished this frame
        tilesInRadiusLastFrame = tilesInRadiusThisFrame;
        tilesInRadiusThisFrame = GetMovableColliders();
        GetMovableColliders();

        //set feedback 
        if (tilesInRadiusLastFrame != null)
        {
            for (int i = 0; i < tilesInRadiusLastFrame.Length; i++)
            {
                tilesInRadiusLastFrame[i].tileState = TileBehaviour.TileState.inRangeIdle;
            }
        }

        if (tilesInRadiusLastFrame != null)
        {
            for (int i = 0; i < tilesInRadiusThisFrame.Length; i++)
            {
                tilesInRadiusThisFrame[i].tileState = TileBehaviour.TileState.canBeAngled;
            }
        }


        //on button pressed start levitating the tiles towads the rod
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (tilesInRadiusThisFrame != null && tilesInRadiusLastFrame.Length > 0)
            {
                for (int i = 0; i < tilesInRadiusLastFrame.Length; i++)
                {
                    TileBehaviour tile = tilesInRadiusThisFrame[i];

                    tile.pushedAwayFromPLayer = false;
                    tile.SetConstancForce(Vector3.zero);

                    if (dockedTiles.Contains(tile) == false)
                    {
                        runningAttractions.Add(StartCoroutine(AttractTile(tile)));
                        dockedTiles.Add(tile);
                        tile.tileState = TileBehaviour.TileState.IsAngled;
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha2)) 
        {
			for (int i = 0; i < runningAttractions.Count; i++)
			{
                StopCoroutine(runningAttractions[i]);
            }

			for (int i = 0; i < dockedTiles.Count; i++)
			{
                dockedTiles[i].tileState = TileBehaviour.TileState.inRangeIdle;
                ReturnTileToDefaultState(dockedTiles[i]);

            }

            dockedTiles.Clear();
        }
    }

    private IEnumerator AttractTile(TileBehaviour tile) 
    {
        float maxTileDistance = 10f;
        float distanceTreshold = 0.5f;
        bool tileDocked = false;

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
                // when tile is close enough create a temporary fixed joint
                tile.DockToRigidbody(anglingBall.GetComponent<Rigidbody>());
                tileDocked = true;
             }
            yield return null;
        }
     }

    private void ReturnTileToDefaultState(TileBehaviour tile)
    {
        //remove spring joint
        FixedJoint temporarySpringJoint = tile.gameObject.GetComponent<FixedJoint>();
        if (temporarySpringJoint != null)
        {
            Component.Destroy(temporarySpringJoint);
        }

        //reset forces
        tile.pushedAwayFromPLayer = Settings.instance.tilesPushedByPlayer;
        tile.SetConstancForce(Vector3.zero);
    }

    private TileBehaviour[] GetMovableColliders() 
    {
        Collider[] collidersInRadius = Physics.OverlapSphere(anglingBall.transform.position, anglingRadius, tileLayerMask);
        TileBehaviour[] tilesInRadius = new TileBehaviour[collidersInRadius.Length];

		for (int i = 0; i < collidersInRadius.Length; i++)
		{
            tilesInRadius[i] = collidersInRadius[i].GetComponent<TileBehaviour>();
        }

        return tilesInRadius;

    } 
}
