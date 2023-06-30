using UnityEngine;
using TMPro;

public class CameraController : MonoBehaviour
{
    public Transform target; // Reference to the cube's transform
    public float followDistance = 10f; // Distance between the camera and the cube

    public float speed = 5.0f;
    public float sensitivity = 5.0f;

    public TextMeshProUGUI cameraStatusText;

    private Vector3 initPosition;
    private Quaternion initRotation;
    private Vector3 initScale;

    bool shouldMoveCamera = false;

    readonly string moveCameraText = "Camera Movable";
    readonly string stillCameraText = "Camera Frozen";

    private void Start()
    {
        initPosition = transform.position;
        initRotation = transform.rotation;
        initScale = transform.localScale;

        HandleStatusText(shouldMoveCamera);
    }

    private void Update()
    {
        // Check for user input to move/freeze camera
        if (Input.GetKeyDown(KeyCode.R))
        {
            shouldMoveCamera = !shouldMoveCamera;
            HandleStatusText(shouldMoveCamera);
        }

        if (shouldMoveCamera)
        {
            // Move the camera forward, backward, left, and right
            transform.position += Input.GetAxis("Vertical") * speed * Time.deltaTime * transform.forward;
            transform.position += Input.GetAxis("Horizontal") * speed * Time.deltaTime * transform.right;

            // Rotate the camera based on the mouse movement
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            transform.eulerAngles += new Vector3(-mouseY * sensitivity, mouseX * sensitivity, 0);
        }
    }

    // set current camera status
    private void HandleStatusText(bool moveCamera)
    {
        cameraStatusText.text = moveCamera ? moveCameraText : stillCameraText;
        cameraStatusText.color = moveCamera ? Color.green : Color.red;
    }

    // reset camera position to the default place
    public void ResetCamera()
    {
        Debug.Log("Resetting Camera Position");
        transform.position = initPosition;
        transform.rotation = initRotation;
        transform.localScale = initScale;
        shouldMoveCamera = false;
        HandleStatusText(shouldMoveCamera);
    }
}