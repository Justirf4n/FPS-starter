using UnityEngine;

public class WeaponAnimation : MonoBehaviour
{
    [Header("Sway Settings")]
    [SerializeField] private float positionAmount = 0.02f;
    [SerializeField] private float rotationAmount = 1.5f;
    [SerializeField] private float smoothTime = 0.08f;

    [Header("Bobbing Settings")]
    [SerializeField] private float bobAmount = 0.02f;
    [SerializeField] private float bobSpeed = 8f;

    private Vector3 initialPos;
    private Quaternion initialRot;

    private Vector3 currentPosVelocity;

    private float bobTimer;

    private void Start()
    {
        initialPos = transform.localPosition;
        initialRot = transform.localRotation;
    }

    private void Update()
    {
        Vector2 mouseInput = GetMouseInput();
        Vector2 moveInput = GetMoveInput();

        Vector3 swayOffset = CalculateSwayPosition(mouseInput);
        Vector3 bobOffset = CalculateBobbing(moveInput);

        ApplyPosition(swayOffset + bobOffset);
        ApplyRotationSway(mouseInput);
    }

    #region Input
    private Vector2 GetMouseInput()
    {
        return new Vector2(
            Input.GetAxis("Mouse X"),
            Input.GetAxis("Mouse Y")
        );
    }

    private Vector2 GetMoveInput()
    {
        return new Vector2(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical")
        );
    }
    #endregion

    #region Position
    private Vector3 CalculateSwayPosition(Vector2 input)
    {
        return new Vector3(
            -input.x * positionAmount,
            -input.y * positionAmount,
            0f
        );
    }

    private Vector3 CalculateBobbing(Vector2 moveInput)
    {
        if (moveInput.sqrMagnitude < 0.1f)
        {
            bobTimer = 0f;
            return Vector3.zero;
        }

        bobTimer += Time.deltaTime * bobSpeed;

        float bobX = Mathf.Sin(bobTimer) * bobAmount * 0.5f;
        float bobY = Mathf.Cos(bobTimer * 2f) * bobAmount;

        return new Vector3(bobX, bobY, 0f);
    }

    private void ApplyPosition(Vector3 offset)
    {
        Vector3 targetPos = initialPos + offset;

        transform.localPosition = Vector3.SmoothDamp(
            transform.localPosition,
            targetPos,
            ref currentPosVelocity,
            smoothTime
        );
    }
    #endregion

    #region Rotation
    private void ApplyRotationSway(Vector2 input)
    {
        Quaternion targetRot = initialRot * Quaternion.Euler(
            input.y * rotationAmount,
            -input.x * rotationAmount,
            -input.x * rotationAmount
        );

        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRot,
            Time.deltaTime * 10f
        );
    }
    #endregion
}
