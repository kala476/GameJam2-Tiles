using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AssembleObject : MonoBehaviour
{
    public float speed;
    public float rotspeed;
    public float mt;
    public float asm;
    public float radius;
    public bool assemble;
    public bool save;
    public List<Rigidbody> tiles;
    public List<Quaternion> rot;
    public List<Vector3> pos;

    void OnDrawGizmos()
    {
        //Gizmos.DrawIcon("");
    }

    // Update is called once per frame
    void Update()
    {
        if (save)
        {
            rot = new List<Quaternion>();
            pos = new List<Vector3>();

            for(int i = 0; i < tiles.Count; i++)
            {
                pos.Add(transform.InverseTransformPoint(tiles[i].transform.position));
                rot.Add(tiles[i].transform.rotation);
            }
            save = false;
        }

        if (assemble)
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                Rigidbody rb = tiles[i];
                Quaternion tileRot = rb.transform.rotation;
                Quaternion targetRot = rot[i];
                Vector3 tilePos = rb.transform.position;
                Vector3 targetPos = transform.TransformPoint(pos[i]);

                if (radius == 0) radius = float.Epsilon;

                Vector3 dir = targetPos - tilePos;
                Vector3 ori = Quaternion.Slerp(Quaternion.identity, Quaternion.Inverse(tileRot) * targetRot, rotspeed).eulerAngles;

                rb.useGravity = false;

                Vector3 difference = new Vector3
                (
                    Vector3.Angle(rb.transform.right, rot[i] * Vector3.right),
                    Vector3.Angle(rb.transform.up, rot[i] * Vector3.up),
                    Vector3.Angle(rb.transform.forward, rot[i] * Vector3.forward)
                ) / 180.0f;

                float angleSum = difference.x + difference.y + difference.z;

                if(dir.magnitude < mt && angleSum < asm)
                {
                    rb.isKinematic = true;
                    rb.MovePosition(transform.TransformPoint(pos[i]));
                    rb.MoveRotation(rot[i]);
                    rb.GetComponent<MeshCollider>().enabled = false;
                }
                else
                {
                    rb.isKinematic = false;
                    rb.AddForce(dir.normalized * speed, ForceMode.VelocityChange);
                    rb.AddTorque(Vector3.Scale(difference, ori) * rotspeed, ForceMode.VelocityChange);
                    rb.GetComponent<MeshCollider>().enabled = true;
                }


            }
        }
        else
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                Rigidbody rb = tiles[i];

                rb.isKinematic = false;
                rb.useGravity = true;
                rb.GetComponent<MeshCollider>().enabled = true;

            }
        }
    }
}
