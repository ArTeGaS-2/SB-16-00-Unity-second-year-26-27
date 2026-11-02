using UnityEngine;
using UnityEngine.InputSystem;

public class GravityGun : MonoBehaviour
{
    [Header("Посилання")]
    [SerializeField] private Camera playerCamera;

    [Header("Захоплення")]
    [SerializeField] private float grabDistance = 10f;
    [SerializeField] private float holdDistance = 3f;
    [SerializeField] private float minHoldDistance = 1f;
    [SerializeField] private float maxHoldDistance = 8f;

    [Header("Керування предметом")]
    [SerializeField] private float scrollDistanceStep = 0.5f;
    [SerializeField] private float rotationSensitivity = 0.2f;

    [Header("Утримання")]
    [SerializeField] private float pullSpeed = 12f;
    [SerializeField] private float maxHoldSpeed = 20f;

    [Header("Кидок")]
    [SerializeField] private float throwForce = 20f;

    private Rigidbody heldObject;

    public bool IsRotatingHeldObject =>
        heldObject != null &&
        Mouse.current != null &&
        Mouse.current.middleButton.isPressed;

    private bool originalUseGravity;
    private float originalLinearDamping;
    private float originalAngularDamping;
    private Vector2 pendingRotation;

    private void Update()
    {
        if (Mouse.current == null)
            return;

        if (heldObject != null)
        {
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (scroll != 0f)
            {
                holdDistance = Mathf.Clamp(
                    holdDistance + Mathf.Sign(scroll) * scrollDistanceStep,
                    minHoldDistance,
                    maxHoldDistance
                );
            }

            if (Mouse.current.middleButton.isPressed)
                pendingRotation += Mouse.current.delta.ReadValue();
        }

        // ЛКМ
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (heldObject == null)
            {
                TryGrabObject();
            }
            else
            {
                ThrowObject();
            }
        }

        // ПКМ
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (heldObject != null)
            {
                DropObject();
            }
        }
    }

    private void FixedUpdate()
    {
        if (heldObject == null)
            return;

        HoldObject();

        if (pendingRotation != Vector2.zero)
        {
            Quaternion yaw = Quaternion.AngleAxis(
                pendingRotation.x * rotationSensitivity,
                playerCamera.transform.up
            );
            Quaternion pitch = Quaternion.AngleAxis(
                -pendingRotation.y * rotationSensitivity,
                playerCamera.transform.right
            );

            heldObject.MoveRotation(yaw * pitch * heldObject.rotation);
            pendingRotation = Vector2.zero;
        }
    }

    private void TryGrabObject()
    {
        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (Physics.Raycast(ray, out RaycastHit hit, grabDistance))
        {
            // Працює також якщо Collider знаходиться на дочірньому об'єкті
            Rigidbody rb = hit.collider.attachedRigidbody;

            if (rb == null)
                return;

            if (!rb.CompareTag("Object"))
                return;

            GrabObject(rb);
        }
    }

    private void GrabObject(Rigidbody rb)
    {
        heldObject = rb;

        // Запам'ятовуємо початкові налаштування
        originalUseGravity = heldObject.useGravity;
        originalLinearDamping = heldObject.linearDamping;
        originalAngularDamping = heldObject.angularDamping;

        // Поки тримаємо — гравітація не заважає
        heldObject.useGravity = false;

        // Трохи стабілізуємо предмет
        heldObject.linearDamping = 8f;
        heldObject.angularDamping = 8f;

        heldObject.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void HoldObject()
    {
        Vector3 targetPosition =
            playerCamera.transform.position +
            playerCamera.transform.forward * holdDistance;

        Vector3 direction = targetPosition - heldObject.worldCenterOfMass;

        Vector3 desiredVelocity = direction * pullSpeed;

        // Не дозволяємо предмету розганятися до абсурдної швидкості
        desiredVelocity = Vector3.ClampMagnitude(
            desiredVelocity,
            maxHoldSpeed
        );

        heldObject.linearVelocity = desiredVelocity;
    }

    private void ThrowObject()
    {
        Rigidbody rb = heldObject;

        ReleaseObject();

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.AddForce(
            playerCamera.transform.forward * throwForce,
            ForceMode.Impulse
        );
    }

    private void DropObject()
    {
        ReleaseObject();
    }

    private void ReleaseObject()
    {
        if (heldObject == null)
            return;

        heldObject.useGravity = originalUseGravity;
        heldObject.linearDamping = originalLinearDamping;
        heldObject.angularDamping = originalAngularDamping;

        heldObject = null;
    }
}
