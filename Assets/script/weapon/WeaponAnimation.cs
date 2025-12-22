using UnityEngine;

public class WeaponAnimation : MonoBehaviour
{
    [Header("Sway Settings")]
    [SerializeField] private float positionAmount = 0.02f;
    [SerializeField] private float rotationAmount = 1.5f;
    [SerializeField] private float smoothTime = 0.08f;

    private Vector3 initialPos;
    private Quaternion initialRot;

    private Vector3 currentPosVelocity;

    private void Awake()
    {
        initialPos = transform.localPosition;
        initialRot = transform.localRotation;
    }

    private void Update()
    {
        Vector2 mouseInput = GetMouseInput();
        ApplyPositionSway(mouseInput);
        ApplyRotationSway(mouseInput);
    }

    private Vector2 GetMouseInput()
    {
        return new Vector2(
            Input.GetAxis("Mouse X"),
            Input.GetAxis("Mouse Y")
        );
    }

    private void ApplyPositionSway(Vector2 input)
    {
        Vector3 targetPos = initialPos + new Vector3(
            -input.x * positionAmount,
            -input.y * positionAmount,
            0f
        );

        transform.localPosition = Vector3.SmoothDamp(
            transform.localPosition,
            targetPos,
            ref currentPosVelocity,
            smoothTime
        );
    }

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
}
