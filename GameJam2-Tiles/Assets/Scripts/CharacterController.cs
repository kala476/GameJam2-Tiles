using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace XGD.TileQuest
{
    public class CharacterController : MonoBehaviour
    {
        [Header("Controls")]
        public bool invertAxisX;
        public float movementSpeed = 10.0f;
        public float rotSpeedHead = 500.0f;
        public float rotSpeedBody = 10.0f;
        public float maxHeadRotY;
        public float maxHeadRotX;
        public float maxBodyRot;
        [Range(0.0f, 1.0f)] public float bodyRotThreshold;
        public AnimationCurve bodyRotMap = new AnimationCurve();
        public AnimationCurve headRotMapX = new AnimationCurve();
        public AnimationCurve headRotMapY = new AnimationCurve();

        [Header("State")]
        public bool movementActive;
        private float currentBodyRotationX;
        public float walkingDistance;


        [Header("Setup")]
        public Transform head;
        public Transform camAnchor;
        public Transform characterRoot;
        public new Camera camera;
        private Angler angler;

        private bool activeCursor;

        private static readonly Quaternion assetRotationCorrection = Quaternion.AngleAxis(90.0f, Vector3.right);

        void Start()
        {
            activeCursor = true;

            Assert.IsNotNull(head);
            Assert.IsNotNull(camAnchor);
            Assert.IsNotNull(characterRoot);
            Assert.IsNotNull(camera);

            angler = GetComponent<Angler>();
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

        public Vector2 ViewInput
        {
            get
            {
                return new Vector2()
                {
                    x = Mathf.Clamp((Input.mousePosition.x / Screen.width - 0.5f) * 2.0f, -1.0f, 1.0f),
                    y = Mathf.Clamp((Input.mousePosition.y / Screen.height - 0.5f) * 2.0f, -1.0f, 1.0f)
                };
            }
        }


        public Vector3 WalkFromView
        {
            get
            {
                Vector2 walkInput = WalkInput;
                Vector3 motionVector = Vector3.zero;
                if (walkInput.magnitude > 0.001f)
                {
                    motionVector += Vector3.ProjectOnPlane(camera.transform.right, Vector3.up) * walkInput.x;
                    motionVector += Vector3.ProjectOnPlane(camera.transform.forward, Vector3.up) * walkInput.y;
                }
                return motionVector.normalized;
            }
        }

        public float ProjectedLength(Vector3 vector, Vector3 normal)
        {
            Vector3 p = Vector3.Project(vector, normal);
            return p.normalized == normal ? p.magnitude : 0.0f;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                activeCursor = !activeCursor;
                GameManager.TogglePauseMenu();
                return;
            }

            if (Input.mousePresent && Input.GetMouseButtonDown(0))
            {
                activeCursor = true;
            }

            Cursor.visible = !activeCursor;

            if (activeCursor && movementActive)
            {
                Vector3 motionVector = WalkFromView;
                Vector2 mouseInput = ViewInput;

                float dRotFromWalk = Utility.SignedAngle(transform.forward, motionVector);
                float headOverdrive = (Mathf.Abs(mouseInput.x) - bodyRotThreshold) / (1.0f - bodyRotThreshold);
                float dRotFromView = Mathf.Sign(mouseInput.x) * bodyRotMap.Evaluate(headOverdrive) * maxHeadRotX;
                float currentHeadRotationX = Mathf.Sign(mouseInput.x) * headRotMapX.Evaluate(Mathf.Abs(mouseInput.x)) * maxHeadRotX;
                float currentHeadRotationY = Mathf.Sign(mouseInput.y) * headRotMapY.Evaluate(Mathf.Abs(mouseInput.y)) * maxHeadRotY;

                float dWalkDist = ProjectedLength(motionVector, transform.forward) * movementSpeed * Time.deltaTime;
                float dWalkRot = dRotFromWalk * rotSpeedBody * Time.deltaTime;
                float dViewRot = dRotFromView * rotSpeedHead * Time.deltaTime;
                float dBodyRotUnclamped = dWalkRot + dViewRot;
                float dBodyRot = Mathf.Clamp(dBodyRotUnclamped, -maxBodyRot, maxBodyRot);
                float ratio = dBodyRotUnclamped == 0 ? 0.0f : (dBodyRot / dBodyRotUnclamped);
                dWalkRot *= ratio;
                dViewRot *= ratio;

                walkingDistance += dWalkDist;
                currentBodyRotationX += dBodyRot;
                currentHeadRotationX -= dWalkRot;

                // Apply to character transform
                head.localRotation = assetRotationCorrection * Quaternion.Euler(-currentHeadRotationY, currentHeadRotationX, 0); ;
                transform.position = transform.position + transform.forward * dWalkDist;
                transform.rotation = Quaternion.Euler(0.0f, currentBodyRotationX, 0.0f);

                // Apply to camera
                Vector3 viewTarget = Vector3.ProjectOnPlane(Vector3.Lerp(transform.position, angler.tangleball.transform.position, Mathf.Abs(dViewRot)), Vector3.up);
                camAnchor.localPosition = Quaternion.AngleAxis(-dWalkRot, Vector3.up) * camAnchor.localPosition;
                camera.transform.position = camAnchor.position;
                camera.transform.rotation = Quaternion.LookRotation(viewTarget - camAnchor.position, Vector3.up);
            }
        }

        public void SetMovementPaused(bool value)
        {
            movementActive = !value;
        }
    }

}