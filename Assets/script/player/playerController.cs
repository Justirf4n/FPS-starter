using UnityEngine;
using Cinemachine;

public class PlayerController : MonoBehaviour
{
    // CharacterController handles collision-based movement without Rigidbody.
    private CharacterController controller;
    // Cinemachine Perlin Noise is used for camera shake and camera bob.
    private CinemachineBasicMultiChannelPerlin noise;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float gravity = 9.5f;
    [SerializeField] private float jumpHeight = 2f;

    [Header("Camera")]
    [SerializeField] public CinemachineVirtualCamera vCam;
    [SerializeField] private float mouseSensitivity = 200f;

    [Header("Camera Bob")]
    [SerializeField] private float bobAmplitude = 1.5f;
    [SerializeField] private float bobFrequency = 2f;

    [Header("Footsteps")]
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float walkInterval = 0.5f;
    [SerializeField] private float sprintInterval = 0.35f;


    [Header("SFX Clips")]
    [SerializeField] private AudioClip[] groundFootsteps;
    [SerializeField] private AudioClip[] grassFootsteps;
    [SerializeField] private AudioClip[] gravelFootsteps;

    [Header("Recoil")]
    private Vector2 currentRecoil;
    private Vector2 targetRecoil;
    private GunData currentGun;

    private float xRotation;
    private float yVelocity;
    private float nextStepTime;

    private float inputX;
    private float inputZ;
    private float mouseX;
    private float mouseY;
    private bool sprinting;

    private Quaternion baseCamRot;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        noise = vCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update handles input and camera rotation.
    private void Update()
    {
        InputManagement();
        LookAround();
    }

    // Movement uses physics-based timing.
    private void FixedUpdate()
    {
        Movement();
        HandleFootsteps();
    }

    // LateUpdate ensures camera effects happen after movement.
    private void LateUpdate()
    {
        CameraBob();
        HandleRecoil();
    }

    // ================= MOVEMENT =================
    private void Movement()
    {
        Vector3 move =
            transform.right * inputX +
            transform.forward * inputZ;

        float speed = sprinting ? sprintSpeed : moveSpeed;
        move *= speed;

        ApplyGravity();
        move.y = yVelocity;

        controller.Move(move * Time.deltaTime);
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded)
        {
            if (yVelocity < 0f)
                yVelocity = -2f;

            if (Input.GetButtonDown("Jump"))
                // This is real physics jump formula.
                yVelocity = Mathf.Sqrt(jumpHeight * 2f * gravity);
        }
        else
        {
            yVelocity -= gravity * Time.deltaTime;
        }
    }

    // ================= CAMERA LOOK =================
    private void LookAround()
    {
        float lookX = mouseX * mouseSensitivity * Time.deltaTime;
        float lookY = mouseY * mouseSensitivity * Time.deltaTime;

        xRotation -= lookY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        baseCamRot = Quaternion.Euler(xRotation, 0f, 0f);
        vCam.transform.localRotation = baseCamRot;
        transform.Rotate(Vector3.up * lookX);
    }

    // ================= RECOIL =================
    private void HandleRecoil()
    {
        if (currentGun == null) return;

        currentRecoil = Vector2.Lerp(
            currentRecoil,
            targetRecoil,
            Time.deltaTime * currentGun.recoilKickSpeed
        );

        targetRecoil = Vector2.Lerp(
            targetRecoil,
            Vector2.zero,
            Time.deltaTime * currentGun.recoilReturnSpeed
        );

        Quaternion recoilRot =
            Quaternion.Euler(-currentRecoil.x, currentRecoil.y, 0f);

        vCam.transform.localRotation = baseCamRot * recoilRot;
    }

    public void ApplyRecoil(GunData gun)
    {
        currentGun = gun;

        float vertical = Random.Range(gun.recoilKick.x * 0.8f, gun.recoilKick.x);
        float horizontal = Random.Range(-gun.recoilKick.y, gun.recoilKick.y);

        targetRecoil += new Vector2(vertical, horizontal);
    }

    // ================= CAMERA BOB =================
    private void CameraBob()
    {
        if (!controller.isGrounded)
        {
            ResetBob();
            return;
        }

        Vector3 vel = controller.velocity;
        vel.y = 0f;

        float speed = vel.magnitude;
        if (speed < 0.1f)
        {
            ResetBob();
            return;
        }

        float t = Mathf.InverseLerp(0f, sprintSpeed, speed);
        noise.m_AmplitudeGain = Mathf.Lerp(0.3f, bobAmplitude, t);
        noise.m_FrequencyGain = Mathf.Lerp(1.5f, bobFrequency, t);
    }

    private void ResetBob()
    {
        noise.m_AmplitudeGain = 0f;
        noise.m_FrequencyGain = 0f;
    }

    // ================= FOOTSTEPS =================
    private void HandleFootsteps()
    {
        if (!controller.isGrounded) return;

        Vector3 velocity = controller.velocity;
        velocity.y = 0f;

        float speed = velocity.magnitude;
        if (speed < 0.2f) return;

        float interval = sprinting ? sprintInterval : walkInterval;

        if (Time.time < nextStepTime) return;

        PlayFootstep();
        nextStepTime = Time.time + interval;
    }

    private AudioClip[] GetFootstepClips()
    {
        if (Physics.Raycast(
            transform.position,
            Vector3.down,
            out RaycastHit hit,
            1.5f,
            groundMask))
        {
            if (hit.collider.CompareTag("Grass"))
                return grassFootsteps;
    
            if (hit.collider.CompareTag("Gravel"))
                return gravelFootsteps;
        }
    
        return groundFootsteps;
    }


    private void PlayFootstep()
    {
        AudioClip[] clips = GetFootstepClips();
        if (clips.Length == 0) return;

        footstepSource.pitch = Random.Range(0.9f, 1.1f);
        footstepSource.volume = Random.Range(0.8f, 1f);

        footstepSource.PlayOneShot(
            clips[Random.Range(0, clips.Length)]
        );
    }

    // ================= INPUT =================
    private void InputManagement()
    {
        inputZ = Input.GetAxis("Vertical");
        inputX = Input.GetAxis("Horizontal");
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");

        sprinting = Input.GetKey(KeyCode.LeftShift) && inputZ > 0f;
    }
}
