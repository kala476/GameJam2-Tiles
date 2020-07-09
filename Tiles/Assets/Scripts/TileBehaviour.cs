using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class TileBehaviour : MonoBehaviour
{
    public MeshRenderer mesh;
    public Rigidbody rigid;
    public ConstantForce force;
    public bool isInPlayerRadius;
	public bool pushedAwayFromPLayer = true;
	public Vector3 conservedVelovcity;
	public enum TileState { outOfRange, inRangeIdle, canBeAngled, isAngled }
	public TileState tileState;

    // Start is called before the first frame update
    void Start()
    {
        mesh = this.GetComponent<MeshRenderer>();
        rigid = this.GetComponent<Rigidbody>();
        force = GetComponent<ConstantForce>();
    }

	// Update is called once per frame
	void Update()
	{
		if (isInPlayerRadius)
		{
			//pushaway from the player depended in distance;
			if (pushedAwayFromPLayer && Settings.instance.tilesPushedByPlayer)
			{
				Vector3 vecToPlayer = this.gameObject.transform.position - new Vector3(Settings.instance.player.transform.position.x, this.gameObject.transform.position.y, Settings.instance.player.transform.position.z); ;
				float sqrDisToPlayer = vecToPlayer.sqrMagnitude;

				
				// pushing away
				if (sqrDisToPlayer < Settings.instance.maxSqrPushDistance)
				{
					float forceValue = Settings.instance.forceToSqrDistance.Evaluate(1f - (Settings.instance.maxSqrPushDistance - sqrDisToPlayer)) * Settings.instance.MaxPushForce;
					Vector3 forceVector = vecToPlayer.normalized * forceValue;
					SetConstancForce(forceVector);
				}
				else
				{
					SetConstancForce(Vector3.zero);
				}
			}
		}

		if (isInPlayerRadius && rigid.isKinematic)
		{
			rigid.isKinematic = false;
		}
		else if (!isInPlayerRadius && !rigid.isKinematic)
		{

			rigid.isKinematic = true;
		}

		//SetColor depending onState

		if (tileState == TileState.outOfRange)
		{
			SetColor(Color.black);
		}
		else if (tileState == TileState.inRangeIdle)
		{
			SetColor(Color.red);
		}
		else if (tileState == TileState.isAngled)
		{
			SetColor(Color.yellow);
		}
		else if (tileState == TileState.canBeAngled)
		{
			SetColor(Color.blue);
		}


	}

	public void LateUpdate()
	{

	}

	public void SetConstancForce(Vector3 forceVector)
	{
		force.force = forceVector;
	}

	public void SetColor(Color color) 
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
	


}

