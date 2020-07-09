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
    private float rotY;
    private float rotX;
    private float headRotY;
    private float headRotX;
    private float bodyRotX;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float horizontalInput = Input.GetAxis("Vertical");
        float verticalInput = Input.GetAxis("Horizontal");

        rotY += Input.GetAxis("Mouse Y");
        rotY = Mathf.Clamp(rotY, maxHeadRotY * -1, maxHeadRotY);
        headRotY = -rotY;

        rotX = Input.GetAxis("Mouse X");
        headRotX += rotX;

        // wenn headRotX sein max erreicht, dreht sich der Körper weiter und headRotX bleibt beim maximum.
        // sobald es wieder kleiner wird, hört der Körper auf sich zu drehen
        if (Mathf.Abs(headRotX) > maxHeadRotX)
        {
            // maximal + maxHeadRotX
            if (headRotX > maxHeadRotX)
            {
                headRotX = maxHeadRotX;
            }
            // minimal - maxHeadRotX
            else if (headRotX < maxHeadRotX)
            {
                headRotX = - maxHeadRotX;
            }

            bodyRotX += rotX;
        }


        this.transform.Translate(verticalInput * Time.deltaTime *movementSpeed, 0,  horizontalInput * Time.deltaTime *movementSpeed);
        this.head.transform.eulerAngles = new Vector3(headRotY * Time.deltaTime * rotSpeed, headRotX * Time.deltaTime * rotSpeed, 0);
        this.transform.localEulerAngles = new Vector3(this.transform.localEulerAngles.x, bodyRotX, this.transform.localEulerAngles.z);

        //Vector3 camLookDirection = this.transform.position - thirdPersonCam.transform.position;
        //camLookDirection = camLookDirection.normalized;
        //Quaternion rotation = Quaternion.LookRotation(camLookDirection);
        //this.head.rotation = rotation;

    }
}
