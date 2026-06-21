using UnityEngine;

public class CameraController2D : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float fastMoveMultiplier = 2f; // Hold Shift to move faster

    [Header("Zoom Settings")]
    public float zoomSpeed = 2f;
    public float minZoom = 5f;
    public float maxZoom = 100f;

    [Header("Bounds (optional)")]
    public bool useBounds = true;
    public float minX = 0f;
    public float maxX = 100f;
    public float minY = 0f;
    public float maxY = 80f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();

        if (!cam.orthographic)
        {
            Debug.LogWarning("CameraController2D: Camera should be in Orthographic mode for 2D!");
        }
    }

    void Update()
    {
        HandleMovement();
        HandleZoom();
    }

    void HandleMovement()
    {
        // Get input from both WASD and Arrow keys
        float horizontal = Input.GetAxis("Horizontal"); // A/D or Left/Right arrows
        float vertical = Input.GetAxis("Vertical");     // W/S or Up/Down arrows

        // Check if Shift is held for fast movement
        float currentSpeed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            currentSpeed *= fastMoveMultiplier;
        }

        // Calculate movement
        Vector3 movement = new Vector3(horizontal, vertical, 0) * currentSpeed * Time.deltaTime;
        Vector3 newPosition = transform.position + movement;

        // Apply bounds if enabled
        if (useBounds)
        {
            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);
        }

        transform.position = newPosition;
    }

    void HandleZoom()
    {
        float zoomChange = 0f;

        // Mouse scroll wheel for zoom
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0f)
        {
            zoomChange = -scrollInput * zoomSpeed;
        }

        // Keyboard zoom: Plus/Equals to zoom in, Minus to zoom out
        if (Input.GetKey(KeyCode.Equals) || Input.GetKey(KeyCode.KeypadPlus))
        {
            zoomChange = -zoomSpeed * Time.deltaTime * 10f; // Zoom in
        }
        if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus))
        {
            zoomChange = zoomSpeed * Time.deltaTime * 10f; // Zoom out
        }

        // Alternative: Page Up/Page Down for zoom
        if (Input.GetKey(KeyCode.PageUp))
        {
            zoomChange = -zoomSpeed * Time.deltaTime * 10f; // Zoom in
        }
        if (Input.GetKey(KeyCode.PageDown))
        {
            zoomChange = zoomSpeed * Time.deltaTime * 10f; // Zoom out
        }

        // Apply zoom if there was any input
        if (zoomChange != 0f)
        {
            float newSize = cam.orthographicSize + zoomChange;
            cam.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        }
    }

    // Helper method to center camera on a specific tile
    public void CenterOnTile(int x, int y)
    {
        Vector3 newPos = transform.position;
        newPos.x = x;
        newPos.y = y;

        if (useBounds)
        {
            newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
            newPos.y = Mathf.Clamp(newPos.y, minY, maxY);
        }

        transform.position = newPos;
    }

    // Helper method to set bounds based on world size
    public void SetBoundsFromWorldSize(int worldX, int worldZ)
    {
        minX = 0f;
        maxX = worldX;
        minY = 0f;
        maxY = worldZ;
        useBounds = true;
    }
}
