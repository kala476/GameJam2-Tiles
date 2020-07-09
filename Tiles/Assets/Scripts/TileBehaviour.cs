using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBehaviour : MonoBehaviour
{
    public MeshRenderer mesh;
    public Rigidbody rigid;
    public ConstantForce force;
    public bool isInPlayerRadius;
    public bool stateChangeAllowed = false;
	public float stateChangeCoolDown = 0.1f;
	public bool pushedAwayFromPLayer = true;
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

}

