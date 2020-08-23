using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Angler : MonoBehaviour
{
    public Transform anglingBall;
    public Camera mainCamera;
    int tileLayerMask;
    int floorLayerMask;
    public AnimationCurve tileAttractiontoSqrDistance;
    public float maxAttractionForce;
    public float anglingRadius;

    List<TileBehaviour> dockedTiles = new List<TileBehaviour>();
    List<Coroutine> runningAttractions = new List<Coroutine>();
    List<TileBehaviour> tilesInRadiusLastFrame;
    public List<TileBehaviour> tilesInRadiusThisFrame;

    public enum TileSelection { simpleProximity, sphereRaycast}
    public TileSelection tileSelection;
    // Start is called before the first frame update
    void Start()
    {
        tileLayerMask = LayerMask.GetMask("Tile");
        floorLayerMask = LayerMask.GetMask("Floor");
    }

    // Update is called once per frame
    void Update()
    {
        //get Tiles that can be fished this frame
        tilesInRadiusLastFrame = tilesInRadiusThisFrame;
        tilesInRadiusThisFrame = GetMovableColliders();
        GetMovableColliders();

		//set feedback 
		if (!Input.GetKey(KeyCode.Mouse0)){
            if (tilesInRadiusLastFrame != null)
            {
                for (int i = 0; i < tilesInRadiusLastFrame.Count; i++)
                {
                    if (tilesInRadiusLastFrame[i].tileState != TileBehaviour.TileState.isAngled && tilesInRadiusLastFrame[i].tileState != TileBehaviour.TileState.outOfRange)
                    {
                        tilesInRadiusLastFrame[i].tileState = TileBehaviour.TileState.inRangeIdle;
                    }
                }
            }

            if (tilesInRadiusLastFrame != null)
            {
                for (int i = 0; i < tilesInRadiusThisFrame.Count; i++)
                { if (tilesInRadiusThisFrame[i].tileState == TileBehaviour.TileState.inRangeIdle)
                    {
                        tilesInRadiusThisFrame[i].tileState = TileBehaviour.TileState.canBeAngled;
                    }
                }
            } }


        //on button pressed start levitating the tiles towads the rod
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (tilesInRadiusThisFrame != null && tilesInRadiusLastFrame.Count > 0)
            {
                for (int i = 0; i < tilesInRadiusLastFrame.Count; i++)
                {
                    TileBehaviour tile = tilesInRadiusThisFrame[i];

                    tile.pushedAwayFromPLayer = false;
                    tile.SetConstancForce(Vector3.zero);

                    if (dockedTiles.Contains(tile) == false)
                    {
                        runningAttractions.Add(StartCoroutine(AttractTile(tile)));
                        dockedTiles.Add(tile);
                        tile.tileState = TileBehaviour.TileState.isAngled;
                    }
                }
            }
        }

        if (Input.GetKeyUp(KeyCode.Mouse0)) 
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
        float maxTileDistance = 30f;
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

    private List<TileBehaviour> GetMovableColliders() 
    {
        Vector3 overlapPoint = Vector3.zero;

        // set centre point from which the ovelap sphere will be checked
        if (tileSelection == TileSelection.simpleProximity)
        {
            // the middle off the angler
            overlapPoint = anglingBall.transform.position;
        }
        else if (tileSelection == TileSelection.sphereRaycast) 
        {
            //raycast from the camera through the angler
            Vector2 ballScreenPoint = mainCamera.WorldToScreenPoint(anglingBall.position);
            Ray ray = mainCamera.ScreenPointToRay(ballScreenPoint);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f, floorLayerMask)) 
            {
                overlapPoint = hit.point;
            }

        }
        Collider[] collidersInRadius = Physics.OverlapSphere(overlapPoint, anglingRadius, tileLayerMask);
        List<TileBehaviour> tilesInRadius = new List<TileBehaviour>();
        for (int i = 0; i < collidersInRadius.Length; i++)
        {
            TileBehaviour tile = collidersInRadius[i].GetComponent<TileBehaviour>();
            if (tile.tileState != TileBehaviour.TileState.outOfRange)
            {
                // if no solid connected add this tile to list
                if (tile.solid == null)
                {
                    tilesInRadius.Add(tile);
                }

                // if there are solids connected ad the whole solid, if it not yet on th list
                else
                {
                    if (!tilesInRadius.Contains(tile.solid))
                    {
                        tilesInRadius.Add(tile.solid);
                    }
                }
            }

        }

        return tilesInRadius;
    }

    private void AddPolyheadronJoint(List<Rigidbody> anchors, Vector3 position, Quaternion rotation) 
    {
        // create new GameObject at te centre
        GameObject go = new GameObject("Polyheadron Centre");
        go.transform.position = position;
        go.transform.rotation = rotation;
        Rigidbody centreRigidbody = go.AddComponent<Rigidbody>();

		// anchor allobjects to the central rigidbody
		for (int i = 0; i < anchors.Count; i++)
		{
            FixedJoint fixedJoint = anchors[i].gameObject.AddComponent<FixedJoint>();
            fixedJoint.connectedBody = centreRigidbody;
		}
    }

    private void AddPolyheadronJoint(List<TileBehaviour> anchors, Vector3 position, Quaternion rotation) { }
}
