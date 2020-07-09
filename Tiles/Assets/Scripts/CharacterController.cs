using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    public float movementSpeed = 10;
    //public Transform thirdPersonCam;
    public Transform head;
    public float maxHeadRotY;
    public float maxHeadRotX;
    public float rotSpeed = 500;
    private float thisFrameRotY;
    private float thisFrameRotX;
    private float targetHeadRotY;
    private float targetHeadRotX;
    private float targetBodyRotX;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float horizontalInput = Input.GetAxis("Vertical");
        float verticalInput = Input.GetAxis("Horizontal");

        thisFrameRotY = Input.GetAxis("Mouse Y") * Time.deltaTime * rotSpeed;
        //thisFrameRotY = Mathf.Clamp(thisFrameRotY, maxHeadRotY * -1, maxHeadRotY);
        targetHeadRotY += thisFrameRotY;
        targetHeadRotY = Mathf.Clamp(targetHeadRotY, maxHeadRotY * -1, maxHeadRotY);


        thisFrameRotX = Input.GetAxis("Mouse X") * Time.deltaTime * rotSpeed;
        targetHeadRotX += thisFrameRotX;

        // wenn headRotX sein max erreicht, dreht sich der Körper weiter und headRotX bleibt beim maximum.
        // sobald es wieder kleiner wird, hört der Körper auf sich zu drehen
        if (Mathf.Abs(targetHeadRotX) > maxHeadRotX)
        {
            // maximal + maxHeadRotX
            if (targetHeadRotX > maxHeadRotX)
            {
                targetHeadRotX = maxHeadRotX;
            }
            // minimal - maxHeadRotX
            else if (targetHeadRotX < maxHeadRotX)
            {
                targetHeadRotX = - maxHeadRotX;
            }

            targetBodyRotX += thisFrameRotX;
        }


        this.transform.Translate(verticalInput * Time.deltaTime *movementSpeed, 0,  horizontalInput * Time.deltaTime *movementSpeed);
        
        this.head.transform.localEulerAngles = new Vector3(-targetHeadRotY, targetHeadRotX, 0);
        this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, targetBodyRotX, this.transform.eulerAngles.z);

        //Vector3 camLookDirection = this.transform.position - thirdPersonCam.transform.position;
        //camLookDirection = camLookDirection.normalized;
        //Quaternion rotation = Quaternion.LookRotation(camLookDirection);
        //this.head.rotation = rotation;

    }
}
