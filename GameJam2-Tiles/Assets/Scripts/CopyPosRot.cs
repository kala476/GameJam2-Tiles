using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CopyPosRot : MonoBehaviour
{
    public Transform origin;
    public bool saveTransformA;
    public bool saveTransformB;
    public bool saveTransformC;
    public bool applyTransformA;
    public bool applyTransformB;
    public bool applyTransformC;
    public Vector3 positionA;
    public Vector3 positionB;
    public Vector3 positionC;
    public Quaternion rotationA;
    public Quaternion rotationB;
    public Quaternion rotationC;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(origin)
        {
            if (saveTransformA || saveTransformB ||saveTransformC)
            {
                Vector3 pos = origin.transform.TransformPoint(transform.position);
                Quaternion rot = Quaternion.Inverse(origin.transform.rotation) * transform.rotation;

                if (saveTransformA)
                {
                    positionA = pos;
                    rotationA = rot;
                }
                else if (saveTransformB)
                {
                    positionB = pos;
                    rotationB = rot;
                }
                else
                {
                    positionC = pos;
                    rotationC = rot;
                }
            }
        }
        if (applyTransformA || applyTransformB || applyTransformC)
        {
            if (applyTransformA)
            {
                transform.position = positionA;
                transform.rotation = rotationA;
            }
            else if (applyTransformB)
            {
                transform.position = positionB;
                transform.rotation = rotationB;
            }
            else
            {
                transform.position = positionC;
                transform.rotation = rotationC;
            }
        }

        saveTransformA = false;
        saveTransformB = false;
        saveTransformC = false;
        applyTransformA = false;
        applyTransformB = false;
        applyTransformC = false;
    }
}
