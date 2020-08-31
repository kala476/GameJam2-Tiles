
using System;
using System.Linq;
using UnityEngine;

namespace XGD.TileQuest
{
	public enum TileStateDescriptor { OutOfRange, InRange, Docked }

	[Serializable]
	public class TileState
	{
		public bool active;
		public bool isInPlayerRadius;
		public bool pushAwayFromPlayer;
		private TileStateDescriptor state;

		public static implicit operator TileStateDescriptor(TileState tileState) { return tileState.state; }
		public static implicit operator int(TileState tileState) { return (int)tileState.state; }
		public static implicit operator Color(TileState tileState) { return tileStateColors[(int)tileState.state]; }

		public void Set(TileStateDescriptor descriptor)
		{
			state = descriptor;
		}

		public static Color[] tileStateColors = new Color[]
		{
		Color.red,
		Color.black,
		Color.yellow,
		Color.blue
		};

		public TileState()
		{
			active = true;
			isInPlayerRadius = false;
			pushAwayFromPlayer = true;
			state = TileStateDescriptor.OutOfRange;
		}
	}

	public class TileBehaviour : MonoBehaviour
	{
		public Tile commonTile;
		public Vector3 conservedVelovcity;
		public TileState state;
		public SolidBehavior solid = null;

		private MeshRenderer mr;
		private Rigidbody rb;
		private ConstantForce cf;

		public bool isOutOfRange { get { return state == TileStateDescriptor.OutOfRange; } }
		public bool isInRange { get { return state == TileStateDescriptor.InRange; } }
		public bool isAngled { get { return state == TileStateDescriptor.Docked; } }


		void Awake()
		{
			Init();
		}

		void Start()
		{
			Init();
		}

		void Init()
		{
			state = new TileState();
			mr = GetComponent<MeshRenderer>();
			rb = GetComponent<Rigidbody>();
			cf = GetComponent<ConstantForce>();
		}

		void Update()
		{
			if (GameManager.instance.gamePaused)
			{
				if (!rb.IsSleeping()) rb.Sleep();
			}
			else
			{
				if (rb.IsSleeping()) rb.WakeUp();
			}

			if (state.active && state.isInPlayerRadius)
			{
				// Push away depending on distance
				if (state.pushAwayFromPlayer && GameManager.instance.playerPushesTiles)
				{
					Vector3 playerPos = GameManager.instance.player.transform.position;
					Vector3 push = -(playerPos - transform.position);
					push.Scale(new Vector3(1.0f, 0.0f, 1.0f));

					float distance = push.sqrMagnitude;
					float maxDistance = GameManager.instance.maxSqrPushDistance;
					float maxForce = GameManager.instance.maxSqrPushDistance;
					AnimationCurve forceMap = GameManager.instance.forceToSqrDistance;

					if (distance < maxDistance)
					{
						Vector3 forceVector = push.normalized * forceMap.Evaluate(distance / maxDistance) * maxForce;
						SetConstancForce(forceVector);
					}
					else
					{
						SetConstancForce(Vector3.zero);
					}
				}
			}

		}

		public void OnEnter()
		{
			state.Set(TileStateDescriptor.InRange);
			rb.isKinematic = false;
			state.Set(TileStateDescriptor.OutOfRange);
			SetColor(state);
		}
		public void OnExit()
		{
			state.Set(TileStateDescriptor.OutOfRange);
			rb.isKinematic = true;
			SetColor(state);
		}
		public void OnGrab()
		{
			state.Set(TileStateDescriptor.Docked);
			rb.isKinematic = false;
			state.Set(TileStateDescriptor.OutOfRange);
			SetColor(state);

		}
		public void OnRelease()
		{
			state.Set(TileStateDescriptor.InRange);
			rb.isKinematic = false;
			state.Set(TileStateDescriptor.OutOfRange);
			SetColor(state);
			ReturnTileToDefaultState();
		}

		public void ReturnTileToDefaultState()
		{
			// Reset state
			state.Set(TileStateDescriptor.InRange);

			// Remove components
			GetComponents<FixedJoint>().ToList().ForEach(joint => Destroy(joint));

			// Reset forces
			state.pushAwayFromPlayer = GameManager.instance.playerPushesTiles;
			SetConstancForce(Vector3.zero);
		}

		public void SetConstancForce(Vector3 forceVector)
		{
			cf.force = forceVector;
		}

		public void SetColor(Color color)
		{
			mr.material.SetColor("_EmissiveColor", color);
		}

		public void DockToRigidbody(Rigidbody connectedBody)
		{
			FixedJoint temporarySpringJoint = gameObject.AddComponent<FixedJoint>();
			temporarySpringJoint.connectedBody = connectedBody.GetComponent<Rigidbody>();
		}

		public void SaveVelocity()
		{
			conservedVelovcity = rb.velocity;
		}

		public void RestoreVelocity()
		{
			rb.velocity = conservedVelovcity;
		}
		public void SetActive(bool value)
		{
			state.active = value;
			SetColor(state);

		}


	}


}