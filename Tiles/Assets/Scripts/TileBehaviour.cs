using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBehaviour : MonoBehaviour
{
    public MeshRenderer mesh;
    public Rigidbody rigid;
    public ConstantForce force;
    public bool isInPlayerRadius;

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

            if (Settings.instance.tilesPushedByPlayer) 
            {
                Vector3 vecToPlayer = this.gameObject.transform.position - new Vector3(Settings.instance.player.transform.position.x, this.gameObject.transform.position.y, Settings.instance.player.transform.position.z); ;
                float sqrDisToPlayer = vecToPlayer.sqrMagnitude;


                if (sqrDisToPlayer < Settings.instance.maxSqrPushDistance)
                {
                    Debug.Log("Distance ok");
                    float forceValue = Settings.instance.forceToSqrDistance.Evaluate( 1f - (Settings.instance.maxSqrPushDistance - sqrDisToPlayer)) * Settings.instance.MaxPushForce;
                    Vector3 forceVector = vecToPlayer.normalized * forceValue;
                    force.force = forceVector;
                }
                else 
                {
                    force.force = Vector3.zero;
                }
            }
        }
        
    }
}
