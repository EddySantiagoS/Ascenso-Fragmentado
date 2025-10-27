using UnityEngine;
using UnityEngine.InputSystem; // Nuevo sistema de entrada

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 2f, -4f);
    public float smoothSpeed = 10f;
    public float rotationSpeed = 0.1f;

    private PlayerControls controls;
    private Vector2 lookInput;
    private float yaw;
    private float pitch;

    void Awake()
    {
        controls = new PlayerControls();
        controls.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        controls.Player.Look.canceled += ctx => lookInput = Vector2.zero;
    }

    void OnEnable() => controls.Player.Enable();
    void OnDisable() => controls.Player.Disable();

    void LateUpdate()
    {
        if (!target) return;

        // Entrada del mouse (nuevo sistema)
        yaw += lookInput.x * rotationSpeed;
        pitch -= lookInput.y * rotationSpeed;
        pitch = Mathf.Clamp(pitch, -20f, 60f);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPosition = target.position + rotation * offset;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}