using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class CharacterController : MonoBehaviour
{
    [Header("Controls")]
    public bool invertAxisX;
    public float movementSpeed = 10.0f;
    public float rotSpeed = 500.0f;
    public float maxHeadRotY;
    public float maxHeadRotX;
    [Range(0.0f, 1.0f)] public float bodyRotThreshold;
    public AnimationCurve bodyRotMap = new AnimationCurve();
    public AnimationCurve headRotMapX = new AnimationCurve();
    public AnimationCurve headRotMapY = new AnimationCurve();

    [Header("State")]
    public bool movementActive;
    private float currentHeadRotationX;
    private float currentHeadRotationY;
    private float currentBodyRotationX;
    public float walkingDistance;

    [Header("Input")]
    private float mouseInputX;
    private float mouseInputY;

    [Header("Setup")]
    public Transform head;
    public Transform camAnchor;
    public Transform characterRoot;
    public new Camera camera;

    private bool activeCursor;
    private Vector3 characterBodyPosition;
    private Quaternion characterBodyRotation;
    private Quaternion characterHeadRotation;
    //private Quaternion characterRootInitRotation;

    //public float tiltAngle;
    //public float tiltBackSpeed;
    //public float angle;
    //public float stepSize = 1.0f;

    void Start()
    {
        activeCursor = true;
        characterBodyPosition = transform.position;
        characterBodyRotation = transform.rotation;
        characterHeadRotation = head.localRotation;

        Assert.IsNotNull(head);
        Assert.IsNotNull(camAnchor);
        Assert.IsNotNull(characterRoot);
        Assert.IsNotNull(camera);

        //characterRootInitRotation = characterRoot.localRotation;

    }

    public Vector2 WalkInput
    {
        get
        {
            Vector2 input = Vector3.zero;
            input += Input.GetKey(KeyCode.W) ? Vector2.up : Vector2.zero;
            input += Input.GetKey(KeyCode.A) ? Vector2.left : Vector2.zero;
            input += Input.GetKey(KeyCode.S) ? Vector2.down : Vector2.zero;
            input += Input.GetKey(KeyCode.D) ? Vector2.right : Vector2.zero;
            input += Input.GetKey(KeyCode.UpArrow) ? Vector2.up : Vector2.zero;
            input += Input.GetKey(KeyCode.LeftArrow) ? Vector2.left : Vector2.zero;
            input += Input.GetKey(KeyCode.DownArrow) ? Vector2.down : Vector2.zero;
            input += Input.GetKey(KeyCode.RightArrow) ? Vector2.right : Vector2.zero;
            return input.normalized;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            activeCursor = !activeCursor;
        }

        if(Input.mousePresent && Input.GetMouseButtonDown(0))
        {
            activeCursor = true;
        }

        Cursor.visible = !activeCursor;
        //Cursor.lockState = activeCursor ? CursorLockMode.Locked : CursorLockMode.None;

        if (activeCursor && movementActive)
        {
            // Get Input
            Vector2 walkInput = WalkInput;
            mouseInputX = Mathf.Clamp((Input.mousePosition.x / Screen.width - 0.5f) * 2.0f, -1.0f, 1.0f);
            mouseInputY = Mathf.Clamp((Input.mousePosition.y / Screen.height - 0.5f) * 2.0f, -1.0f, 1.0f);

            float screenExceedX = (Mathf.Abs(mouseInputX) - bodyRotThreshold) / (1.0f - bodyRotThreshold);

            currentBodyRotationX += Mathf.Sign(mouseInputX) * bodyRotMap.Evaluate(screenExceedX) * Time.deltaTime * rotSpeed;
            currentHeadRotationX = Mathf.Sign(mouseInputX) * headRotMapX.Evaluate(Mathf.Abs(mouseInputX)) * maxHeadRotX;
            currentHeadRotationY = Mathf.Sign(mouseInputY) * headRotMapY.Evaluate(Mathf.Abs(mouseInputY)) * maxHeadRotY;

            // Get Inversion
            float invert = invertAxisX ? 1.0f : 0.0f;

            // Set camera position
            camera.transform.position = camAnchor.position;
            camera.transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.position - camAnchor.position, Vector3.up), Vector3.up);

            // Calculation Character Position
            Vector3 motionVector = Vector3.zero;
            motionVector += Vector3.ProjectOnPlane(camera.transform.right, Vector3.up) * walkInput.x; 
            motionVector += Vector3.ProjectOnPlane(camera.transform.forward, Vector3.up) * walkInput.y;
            motionVector *= movementSpeed * Time.deltaTime;

            if (motionVector.magnitude < 0.001f) motionVector = Vector3.zero;

            characterBodyPosition = transform.position + motionVector;            

            // Calculate Character Rotation
            bool correct = true;
            Quaternion corrective = correct ? Quaternion.AngleAxis(90.0f, Vector3.right) : Quaternion.identity;
            characterHeadRotation = corrective * Quaternion.Euler(-currentHeadRotationY, currentHeadRotationX, 0);
            characterBodyRotation = Quaternion.Euler(0.0f, currentBodyRotationX, 0.0f);

            // Calculate Walking Distance
            walkingDistance += motionVector.magnitude;
            //walkingDistance %= stepSize;


            //float t, scale, snap;
            //t = (walkingDistance / stepSize) % 1.0f;
            ////snap = -0.5f * (((2.0f * t - 0.5f) % 1.0f) - 0.5f) + 0.25f;
            //snap = Mathf.Sin(2.0f * Mathf.PI * t);
            //t += Mathf.Clamp01(tiltBackSpeed * Time.deltaTime) * snap;
            //t %= 1.0f;
            //scale = 4.0f * Mathf.Abs(((t - 0.25f) % 1.0f) - 0.5f) - 1.0f;
            //angle = tiltAngle * scale;
            ////angle = tiltAngle * Mathf.Sin(2.0f * Mathf.PI * t);

            //walkingDistance = Mathf.Floor(walkingDistance/stepSize)*stepSize + t * stepSize;
                        

            // Calculate Head Rotation
            //Quaternion tilt = Quaternion.AngleAxis(angle, Vector3.up);
            //characterRoot.localRotation = characterRootInitRotation * tilt;

            head.localRotation = characterHeadRotation;
            transform.position = characterBodyPosition;
            transform.rotation = characterBodyRotation;
        }
    }

    public void SetMovementPaused(bool value) 
    {
        movementActive = !value;
    }
}
