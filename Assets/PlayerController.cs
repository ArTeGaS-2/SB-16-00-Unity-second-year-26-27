using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float jumpForce = 6f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 2f;
    public float minLookAngle = -80f;
    public float maxLookAngle = 80f;

    [Header("Camera")]
    public Vector3 firstPersonPosition = new Vector3(0f, 1.6f, 0f);
    public Vector3 thirdPersonOffset = new Vector3(0f, 2f, -4f);

    [Header("Ground Check")]
    public float groundCheckDistance = 1.1f;

    private Rigidbody rb;
    private Camera playerCamera;

    private float horizontalInput;
    private float verticalInput;

    private float cameraPitch = 0f;

    private bool isThirdPerson = false;
    private bool jumpRequested = false;

    void Awake()
    {
        // Отримуємо Rigidbody з цього ж GameObject
        rb = GetComponent<Rigidbody>();

        // Шукаємо камеру з тегом MainCamera
        playerCamera = Camera.main;

        if (playerCamera == null)
        {
            Debug.LogError("Main Camera не знайдена! Перевір тег MainCamera.");
            return;
        }

        // Забороняємо Rigidbody перевертати персонажа
        rb.freezeRotation = true;

        // Ховаємо курсор
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        ReadMovementInput();
        ReadMouseInput();
        CheckJump();
        CheckCameraSwitch();
    }

    void FixedUpdate()
    {
        MovePlayer();

        if (jumpRequested)
        {
            Jump();
            jumpRequested = false;
        }
    }

    void LateUpdate()
    {
        UpdateCameraPosition();
    }

    void ReadMovementInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    void ReadMouseInput()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Поворот самого персонажа вліво / вправо
        transform.Rotate(Vector3.up * mouseX);

        // Поворот камери вверх / вниз
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(
            cameraPitch,
            minLookAngle,
            maxLookAngle
        );
    }

    void CheckJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            jumpRequested = true;
        }
    }

    void CheckCameraSwitch()
    {
        if (Input.GetKeyDown(KeyCode.LeftAlt) ||
            Input.GetKeyDown(KeyCode.RightAlt))
        {
            isThirdPerson = !isThirdPerson;
        }
    }

    void MovePlayer()
    {
        Vector3 movement =
            transform.right * horizontalInput +
            transform.forward * verticalInput;

        movement = movement.normalized * moveSpeed;

        // Зберігаємо вертикальну швидкість Rigidbody
        rb.linearVelocity = new Vector3(
            movement.x,
            rb.linearVelocity.y,
            movement.z
        );
    }

    void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    bool IsGrounded()
    {
        return Physics.Raycast(
            transform.position,
            Vector3.down,
            groundCheckDistance
        );
    }

    void UpdateCameraPosition()
    {
        if (playerCamera == null)
            return;

        if (isThirdPerson)
        {
            // Позиція камери позаду персонажа
            Vector3 cameraPosition =
                transform.position +
                transform.TransformDirection(thirdPersonOffset);

            playerCamera.transform.position = cameraPosition;
        }
        else
        {
            // Камера всередині / на рівні голови персонажа
            playerCamera.transform.position =
                transform.position +
                transform.TransformDirection(firstPersonPosition);
        }

        // Камера дивиться в напрямку персонажа
        // + окремий вертикальний нахил
        playerCamera.transform.rotation =
            transform.rotation *
            Quaternion.Euler(cameraPitch, 0f, 0f);
    }
}