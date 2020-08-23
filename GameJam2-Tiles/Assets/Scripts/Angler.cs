using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileSelectionMode { simpleProximity, sphereRaycast }
public class Angler : MonoBehaviour
{
    public TileSelectionMode TileSelectionMode;
    public Camera mainCamera;
    public AnimationCurve tileAttractiontoSqrDistance;
    public float maxAttractionForce;
    public float anglingRadius;

    private List<Coroutine> runningAttractions = new List<Coroutine>();
    private List<TileBehaviour> tilesInRadiusLastFrame = new List<TileBehaviour>();
    private List<TileBehaviour> tilesInRadiusThisFrame = new List<TileBehaviour>();
    private List<TileBehaviour> tilesDocked = new List<TileBehaviour>();

    private Rigidbody tangleball;
    private Transform tangleballTop;
    private Transform tangleballBottom;

    private LayerMask tileLayerMask;
    private LayerMask floorLayerMask;

    void Start()
    {
        tileLayerMask = LayerMask.GetMask("Tile");
        floorLayerMask = LayerMask.GetMask("Floor");

        SetupTangleball();
    }


    void SetupTangleball()
    {
        if (!tangleball || !tangleballTop || !tangleballBottom)
        {
            IEnumerable<Transform> tangleballPoints = GetComponentsInChildren<Transform>().Where(x => x.name.ToLower().Contains("tangleball"));
            tangleballTop = tangleballPoints.Where(x => x.name.ToLower().Contains("top")).First();
            tangleballBottom = tangleballPoints.Where(x => x.name.ToLower().Contains("bottom")).First();
        }

        tangleball = new GameObject().AddComponent<Rigidbody>();
        tangleball.name = "Tangleball";
        tangleball.isKinematic = true;
        tangleball.useGravity = false;
        UpdateTangleball();

    }

    void UpdateTangleball()
    {
        tangleball.MovePosition(Vector3.Lerp(tangleballTop.position, tangleballBottom.position, 0.5f));
    }

    private void FixedUpdate()
    {
        UpdateTangleball();
    }

    void Levetate(TileBehaviour tile)
    {
        tile.state.pushAwayFromPlayer = false;

        tile.state.Set(TileStateDescriptor.Angled);
        tile.SetConstancForce(Vector3.zero);
        runningAttractions.Add(StartCoroutine(AttractTile(tile)));
        tilesDocked.Add(tile);
    }

    void Update()
    {
        // Get Tiles that can be fished this frame
        tilesInRadiusLastFrame = tilesInRadiusThisFrame;
        tilesInRadiusThisFrame = GetMovableColliders();
        GetMovableColliders();

		// Set feedback 
		if (!Input.GetKey(KeyCode.Mouse0))
        {
            tilesInRadiusLastFrame.Where(tile => tile.isAngled).ToList().ForEach(x => x.state.Set(TileStateDescriptor.InRange));
            tilesInRadiusThisFrame.Where(tile => tile.isInRange).ToList().ForEach(x => x.state.Set(TileStateDescriptor.CanBeAngled));
        }

        // Levitatation
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            tilesInRadiusLastFrame.ForEach(tile => Levetate(tile));
        }

        if (Input.GetKeyUp(KeyCode.Mouse0)) 
        {
            runningAttractions.ForEach(x => StopCoroutine(x));
            tilesDocked.ForEach(tile => tile.ReturnTileToDefaultState());
            tilesDocked.Clear();
        }
    }

    private IEnumerator AttractTile(TileBehaviour tile) 
    {
        float maxTileDistance = 30f;
        float distanceTreshold = 0.5f;
        bool tilesDocked = false;

        while (tilesDocked == false)
        {
            Vector3 tileVector = tile.transform.position - tangleball.transform.position;
            float tileDistance = tileVector.magnitude;

            // Attract distant tiles
            if (tileDistance > distanceTreshold)
            {
                float forceValue = tileAttractiontoSqrDistance.Evaluate(tileDistance / maxTileDistance) * maxAttractionForce;
                Vector3 forceVector = tileVector.normalized * forceValue;
                tile.SetConstancForce(forceVector);
            }

            // Attatch close tiles
            else
            {
                tile.DockToRigidbody(tangleball);
                tilesDocked = true;
             }
            yield return null;
        }
     }

    private List<TileBehaviour> GetMovableColliders() 
    {
        Vector3 overlapPoint = Vector3.zero;

        // Get overlap sphere center point

        if (TileSelectionMode == TileSelectionMode.simpleProximity)
        {
            overlapPoint = tangleball.transform.position;
        }
        else if (TileSelectionMode == TileSelectionMode.sphereRaycast) 
        {
            Vector2 ballScreenPoint = mainCamera.WorldToScreenPoint(tangleball.position);
            Ray ray = mainCamera.ScreenPointToRay(ballScreenPoint);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f, floorLayerMask)) 
            {
                overlapPoint = hit.point;
            }
        }

        List<TileBehaviour> tilesInRadius = Physics.OverlapSphere(overlapPoint, anglingRadius, tileLayerMask).Select(x => x.GetComponent<TileBehaviour>()).ToList();


        ////////if (tile.tileState != TileState.outOfRange)
        ////////{
        ////////    // if no solid connected add this tile to list
        ////////    if (tile.solid)
        ////////    {
        ////////        if (!tilesInRadius.Contains(tile.solid))
        ////////        {
        ////////            tilesInRadius.Add(tile.solid);
        ////////        }
        ////////    }

        ////////    // if there are solids connected add the whole solid, if it not yet on th list
        ////////    else
        ////////    {
        ////////        tilesInRadius.Add(tile);
        ////////    }
        ////////}

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
