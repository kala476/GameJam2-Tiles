using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    public float movementSpeed = 10;
    public Transform thirdPersonCam;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float horizontalInput = Input.GetAxis("Vertical");
        float verticalInput = Input.GetAxis("Horizontal");

        this.transform.Translate(verticalInput * Time.deltaTime *movementSpeed, 0,  horizontalInput * Time.deltaTime *movementSpeed);

        Vector3 camLookDirection = this.transform.position - thirdPersonCam.transform.position;
    }
}
