
using System;
using System.Linq;
using UnityEngine;


public enum TileStateDescriptor { OutOfRange, InRange, CanBeAngled, Angled, Docked}

[Serializable]
public class TileState
{
	public bool active;
	public bool isInPlayerRadius;
	public bool pushAwayFromPlayer;
	private TileStateDescriptor state;

	public static implicit operator TileStateDescriptor(TileState tileState) { return tileState.state; }
	public static implicit operator int(TileState tileState) { return (int)tileState.state; }
	public static implicit operator Color (TileState tileState) { return tileStateColors[(int)tileState.state]; }

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
	public Vector3 conservedVelovcity;
	public TileState state;
	public SolidBehavior solid = null;

	private MeshRenderer mesh;
	private Rigidbody rigid;
	private ConstantForce force;

	public bool isOutOfRange { get { return state == TileStateDescriptor.OutOfRange; } }
	public bool isInRange { get { return state == TileStateDescriptor.InRange; } }
	public bool canBeAngled { get { return state == TileStateDescriptor.CanBeAngled; } }
	public bool isAngled { get { return state == TileStateDescriptor.Angled; } }

	public void SetOutOfRange()
	{
		state.Set(TileStateDescriptor.OutOfRange);
		rigid.isKinematic = true;
		SetColor(state);
	}
	public void SetInRange()
	{
		
		state.Set(TileStateDescriptor.InRange);
		rigid.isKinematic = false;
		state.Set(TileStateDescriptor.OutOfRange);
		SetColor(state);
	}

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
		mesh = GetComponent<MeshRenderer>();
		rigid = GetComponent<Rigidbody>();
		force = GetComponent<ConstantForce>();
	}

	void Update()
	{
		if (state.active && state.isInPlayerRadius)
		{
			// Push away depending on distance
			if (state.pushAwayFromPlayer && Settings.instance.playerPushesTiles)
			{
				Vector3 playerPos = Settings.instance.player.transform.position;
				Vector3 push = - (playerPos - transform.position);
				push.Scale(new Vector3(1.0f, 0.0f, 1.0f));

				float distance = push.sqrMagnitude;
				float maxDistance = Settings.instance.maxSqrPushDistance;
				float maxForce = Settings.instance.maxSqrPushDistance;
				AnimationCurve forceMap = Settings.instance.forceToSqrDistance;

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

	public void ReturnTileToDefaultState()
	{
		// Reset state
		state.Set(TileStateDescriptor.InRange);

		// Remove components
		GetComponents<FixedJoint>().ToList().ForEach(joint => Destroy(joint));

		// Reset forces
		state.pushAwayFromPlayer = Settings.instance.playerPushesTiles;
		SetConstancForce(Vector3.zero);
	}

	public void SetConstancForce(Vector3 forceVector)
	{
		force.force = forceVector;
	}

	public virtual void SetColor(Color color) 
	{
		mesh.material.color = color;
	}

	public void DockToRigidbody(Rigidbody connectedBody) 
	{
		FixedJoint temporarySpringJoint = gameObject.AddComponent<FixedJoint>();
		temporarySpringJoint.connectedBody = connectedBody.GetComponent<Rigidbody>();
	}

	public void SaveVelocity() 
	{
		conservedVelovcity = rigid.velocity;
	}

	public void RestoreVelocity() 
	{
		rigid.velocity = conservedVelovcity;
	}
	public void SetActive(bool value) 
	{
		state.active = value;
		SetColor(state);

	}


}

