using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    private Vector2 movement;
    private Vector2 rotation;
    private float scroll;
    private float angleHeight;
    private bool isInRotateMode;

    [SerializeField] private Transform center;
    [SerializeField] private Camera myCamera;
    [SerializeField] private Transform cameraTransform;

    [SerializeField] private float speed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float scrollSpeed;
    [SerializeField] private float distanceToCenter;
    [SerializeField] private float distanceMaxToCenter;

    // Start is called before the first frame update
    void Start()
    {
        // start at an angle of around 45°
        angleHeight = Mathf.Atan2(cameraTransform.localPosition.y, -cameraTransform.localPosition.z);
    }

    // Update is called once per frame
    void Update()
    {
        // adjust the center point rotation and rotation according to input movement
        center.Translate(new Vector3(movement.x,0, movement.y)*speed*Time.deltaTime);
        center.Rotate(new Vector3(0,rotation.x * rotationSpeed * Time.deltaTime * 5.7f,0),Space.World);

        // adjust the angle between the ground and the camera, clamp it on one quarter of a circle
        angleHeight -= rotation.y * rotationSpeed * Time.deltaTime * 0.1f;
        angleHeight = Mathf.Clamp(angleHeight, 0, 1.56f);
        cameraTransform.LookAt(center);

        // set the zoom
        distanceToCenter += scroll * scrollSpeed * Time.deltaTime;
        distanceToCenter = Mathf.Clamp(distanceToCenter, 1, distanceMaxToCenter);

        // set the position of the camera
        cameraTransform.localPosition = new Vector3(0, distanceToCenter*Mathf.Sin(angleHeight) , -distanceToCenter * Mathf.Cos(angleHeight));

    }

    public void Move(InputAction.CallbackContext context)
    {
        // move event Handler
        movement = context.ReadValue<Vector2>();
    }

    public void Rotatae(InputAction.CallbackContext context)
    {
        // Rotate event Handler
        if (isInRotateMode) rotation = context.ReadValue<Vector2>();
    }

    public void Zoom(InputAction.CallbackContext context)
    {
        // Zoom event Handler
        scroll = context.ReadValue<float>() / 120f;
    }

    public void RotateMode(InputAction.CallbackContext context)
    {
        // Rotation mode event Handler
        if (context.phase == InputActionPhase.Started)
        {
            isInRotateMode = true;
            Cursor.lockState = CursorLockMode.Locked;
        }
        if (context.phase == InputActionPhase.Canceled)
        {
            isInRotateMode = false;
            rotation = Vector2.zero;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
