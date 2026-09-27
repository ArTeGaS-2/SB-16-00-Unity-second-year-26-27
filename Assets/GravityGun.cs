using UnityEngine;
using UnityEngine.InputSystem;

public class GravityGun : MonoBehaviour
{
    [Header("Посилання")]
    [SerializeField] private Camera playerCamera;

    [Header("Захоплення")]
    [SerializeField] private float grabDistance = 10f;
    [SerializeField] private float holdDistance = 3f;

    [Header("Утримання")]
    [SerializeField] private float pullSpeed = 12f;
    [SerializeField] private float maxHoldSpeed = 20f;

    [Header("Кидок")]
    [SerializeField] private float throwForce = 20f;

    private Rigidbody heldObject;

    private bool originalUseGravity;
    private float originalLinearDamping;
    private float originalAngularDamping;

    private void Update()
    {
        if (Mouse.current == null)
            return;

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