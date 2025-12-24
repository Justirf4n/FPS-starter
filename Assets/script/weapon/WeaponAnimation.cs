using UnityEngine;

public class WeaponAnimation : MonoBehaviour
{
    [Header("Sway")]
    [SerializeField] private float swayAmount = 0.02f;
    [SerializeField] private float swayRotation = 1.5f;

    [Header("Bobbing")]
    [SerializeField] private float bobAmount = 0.02f;
    [SerializeField] private float bobSpeed = 8f;

    [Header("Recoil")]
    [SerializeField] private float recoilAmount = 0.1f;
    [SerializeField] private float recoilSmoothness = 12f;

    [Header("Smooth")]
    [SerializeField] private float smooth = 10f;

    private Vector3 startPos;
    private Quaternion startRot;

    private Vector3 recoilOffset;
    private float bobTime;

    private void Start()
    {
        startPos = transform.localPosition;
        startRot = transform.localRotation;
    }

    private void Update()
    {
        Vector2 mouse = GetMouseInput();
        Vector2 move = GetMoveInput();

        Vector3 sway = CalculateSway(mouse);
        Vector3 bob = CalculateBobbing(move);

        recoilOffset = Vector3.Lerp(
            recoilOffset,
            Vector3.zero,
            Time.deltaTime * recoilSmoothness
        );

        Vector3 finalPos = startPos + sway + bob + recoilOffset;
        Quaternion finalRot = startRot * CalculateRotationSway(mouse);

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            finalPos,
            Time.deltaTime * smooth
        );

        transform.localRotation = Quaternion.Lerp(
            transform.localRotation,
            finalRot,
            Time.deltaTime * smooth
        );
    }

    // 🔫 Dipanggil saat menembak
    public void Fire()
    {
        recoilOffset += Vector3.back * recoilAmount;
    }

    #region Calculations
    private Vector3 CalculateSway(Vector2 mouse)
    {
        return new Vector3(
            -mouse.x * swayAmount,
            -mouse.y * swayAmount,
            0f
        );
    }

    private Quaternion CalculateRotationSway(Vector2 mouse)
    {
        return Quaternion.Euler(
            mouse.y * swayRotation,
            -mouse.x * swayRotation,
            -mouse.x * swayRotation
        );
    }

    private Vector3 CalculateBobbing(Vector2 move)
    {
        if (move.sqrMagnitude < 0.1f)
        {
            bobTime = 0f;
            return Vector3.zero;
        }

        bobTime += Time.deltaTime * bobSpeed;

        return new Vector3(
            Mathf.Sin(bobTime) * bobAmount,
            Mathf.Cos(bobTime * 2f) * bobAmount,
            0f
        );
    }
    #endregion

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
}
