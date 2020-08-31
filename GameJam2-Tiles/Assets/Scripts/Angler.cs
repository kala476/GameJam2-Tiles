using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XGD.TileQuest
{
    public enum TileSelectionMode { Proximity, FromView }
    public class Angler : MonoBehaviour
    {
        public TileSelectionMode TileSelectionMode;
        public Camera mainCamera;
        public GameObject tangleballPrefab;
        public AnimationCurve tileAttractiontoSqrDistance;
        public float maxAttractionForce;
        public float anglingRadius;
        public bool generateJoints;

        public List<Coroutine> runningAttractions = new List<Coroutine>();
        //private List<TileBehaviour> tilesInRadiusThisFrame = new List<TileBehaviour>();
        public List<TileBehaviour> tilesNear = new List<TileBehaviour>();
        public List<TileBehaviour> tilesDocked = new List<TileBehaviour>();
        public List<Solid> availableSolids = new List<Solid>();
        public List<Tile> availableTiles = new List<Tile>();
        public List<int> availableTileCounts = new List<int>();

        public Rigidbody tangleball;
        private Transform tangleballTop;
        private Transform tangleballBottom;

        private LayerMask tileLayerMask;
        private LayerMask floorLayerMask;

        void Start()
        {
            tileLayerMask = LayerMask.GetMask("Tiles");
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

            tangleball = Instantiate(tangleballPrefab).GetComponent<Rigidbody>();
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

        void Grab(TileBehaviour tile)
        {
            tile.OnGrab();
            tile.state.pushAwayFromPlayer = false;
            tile.SetConstancForce(Vector3.zero);
            runningAttractions.Add(StartCoroutine(AttractTile(tile)));
            tilesDocked.Add(tile);
        }

        void Release(TileBehaviour tile)
        {
            int id = tilesDocked.IndexOf(tile);
            tile.OnRelease();
            Coroutine c = runningAttractions[id];
            StopCoroutine(c);
        }

        void Update()
        {
            Vector3 overlapPoint = Vector3.zero;

            if (TileSelectionMode == TileSelectionMode.Proximity)
            {
                overlapPoint = tangleball.transform.position;
            }
            else if (TileSelectionMode == TileSelectionMode.FromView)
            {
                Vector2 ballScreenPoint = mainCamera.WorldToScreenPoint(tangleball.position);
                Ray ray = mainCamera.ScreenPointToRay(ballScreenPoint);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 100f, floorLayerMask))
                {
                    overlapPoint = hit.point;
                }
            }

            List<TileBehaviour> tilesProbed = Physics.OverlapSphere(overlapPoint, anglingRadius, tileLayerMask).Select(x => x.GetComponent<TileBehaviour>()).ToList();
            List<TileBehaviour> addTiles = tilesNear.Where(x => !tilesProbed.Contains(x)).ToList();
            List<TileBehaviour> removeTiles = tilesProbed.Where(x => !tilesNear.Contains(x)).ToList();

            bool active = Input.GetKey(KeyCode.Mouse0);
            bool release = Input.GetKeyUp(KeyCode.Mouse0);
            bool press = Input.GetKeyDown(KeyCode.Mouse0);

            if (press || release)
            {
                if (press)
                {
                    removeTiles.ForEach(tile => tile.OnExit());
                    addTiles.ForEach(tile => Grab(tile));
                    tilesNear.ForEach(tile => Grab(tile));
                    tilesNear.Clear();
                }
                else if (release)
                {
                    tilesDocked.ForEach(tile => Release(tile));
                    removeTiles.ForEach(tile => tile.OnExit());
                    tilesDocked.Clear();
                    runningAttractions.Clear();
                }
            }
            else
            {
                addTiles.ForEach(tile => tile.OnEnter());
                removeTiles.ForEach(tile => tile.OnExit());
            }
            tilesNear = tilesProbed;

            availableTiles = tilesNear.Select(tile => tile.commonTile).ToList();
            GameManager.instance.solidCollection.Count(availableTiles, out availableTileCounts, out availableSolids);

        }




        private IEnumerator AttractTile(TileBehaviour tile)
        {
            float maxTileDistance = 30f;
            float distanceTreshold = 0.5f;
            bool tilesDocked = false;

            while (tilesDocked == false)
            {
                Vector3 tileVector = tangleball.transform.position - tile.transform.position;
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

        private void OnTriggerEnter(Collider other)
        {

            if (other.tag.Equals("Tile"))
            {
                TileBehaviour tile = other.GetComponent<TileBehaviour>();

                if (tile)
                {
                    RegisterTile(tile);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.tag.Equals("Tile"))
            {
                TileBehaviour tile = other.GetComponent<TileBehaviour>();

                if (tile)
                {
                    UnRegisterTile(tile);
                }
            }
        }

        void RegisterTile(TileBehaviour tile)
        {
            Debug.Log("Registered " + tile.name);

            if (GameManager.instance.conserveVelocity)
                tile.RestoreVelocity();


            tile.state.isInPlayerRadius = true;

        }

        void UnRegisterTile(TileBehaviour tile)
        {
            Debug.Log("Unregistered " + tile.name);

            tile.state.isInPlayerRadius = true;

            tile.SaveVelocity();
        }

        private void AddPolyheadronJoint(List<TileBehaviour> anchors, Vector3 position, Quaternion rotation) { }
    }

}