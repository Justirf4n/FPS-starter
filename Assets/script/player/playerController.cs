using UnityEngine;
using Cinemachine;
using System.Security.Cryptography;

public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private CinemachineBasicMultiChannelPerlin noiseComponent;

    ///////////////// MOVEMENT ////////////////////
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float gravity = 9.5f;
    [SerializeField] private float jumpHeight = 2f;

    ///////////////// CAMERA BOB ////////////////////
    [Header("Camera Bob")]
    [SerializeField] private float bobFrequency = 2f;
    [SerializeField] private float bobAmplitude = 2f;

    ///////////////// CAMERA ////////////////////
    [Header("Camera Settings")]
    public CinemachineVirtualCamera virtualCamera;
    [SerializeField] private float mouseSensitivity = 200f;

    ///////////////// RECOIL ////////////////////
    [Header("Recoil")]
    private Vector2 currentRecoil;
    private Vector2 targetRecoil;

    ///////////////// FOOTSTEPS ////////////////////
    [Header("Footstep Settings")]
    [SerializeField] private AudioSource footstepSound;
    [SerializeField] private LayerMask terrainLayerMask;
    [SerializeField] private float walkStepInterval = 0.5f;
    [SerializeField] private float sprintStepInterval = 0.35f;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip[] groundFootsteps;
    [SerializeField] private AudioClip[] grassFootsteps;
    [SerializeField] private AudioClip[] gravelFootsteps;

    ///////////////// INTERNAL STATE ////////////////////
    private Quaternion baseCameraRotation;

    private GunData currentGunData;
    private float xRotation;
    private float verticalVelocity;
    private float nextStepTime;

    private float moveInput;
    private float turnInput;
    private float mouseX;
    private float mouseY;
    private bool isSprinting;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        noiseComponent = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        InputManagement();
        LookAround();
        Movement();
        HandleFootsteps();
    }

    private void LateUpdate()
    {
        CameraBob();
        HandleRecoil();
    }

    ///////////////// MOVEMENT ////////////////////
    private void Movement()
    {
        Vector3 move = new Vector3(turnInput, 0f, moveInput);
        float speed = isSprinting ? sprintSpeed : moveSpeed;

        move = transform.TransformDirection(move) * speed;

        ApplyGravity();
        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded)
        {
            if (verticalVelocity < 0f)
                verticalVelocity = -2f;

            if (Input.GetButtonDown("Jump"))
                verticalVelocity = Mathf.Sqrt(jumpHeight * 2f * gravity);
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }
    }

    ///////////////// RECOIL ////////////////////
    private void HandleRecoil()
    {
        if (currentGunData == null ) return; 
        
        targetRecoil.y = Mathf.Clamp(targetRecoil.y, -6f, 6f);

        currentRecoil = Vector2.Lerp(
            currentRecoil,
            targetRecoil,
            Time.deltaTime * currentGunData.recoilKickSpeed
        );

        targetRecoil = Vector2.Lerp(
            targetRecoil,
            Vector2.zero,
            Time.deltaTime * currentGunData.recoilReturnSpeed
        );

        Quaternion recoilRotation =
            Quaternion.Euler(-currentRecoil.x, currentRecoil.y, 0f);

        virtualCamera.transform.localRotation =
            baseCameraRotation * recoilRotation;

    }

    public void ApplyRecoil(GunData gunData)
    {
        currentGunData = gunData;

        float vertical = Random.Range(
            gunData.recoilKick.x * 0.8f,
            gunData.recoilKick.x
        );

        float horizontal = Random.Range(
            -gunData.recoilKick.y,
            gunData.recoilKick.y
        );

        targetRecoil += new Vector2(vertical, horizontal);
    }

    ///////////////// CAMERA BOB ////////////////////
    private void CameraBob()
    {
        if (!controller.isGrounded)
        {
            ResetCameraBob();
            return;
        }

        Vector3 horizontalVelocity = controller.velocity;
        horizontalVelocity.y = 0f;

        float speed = horizontalVelocity.magnitude;
        if (speed < 0.1f)
        {
            ResetCameraBob();
            return;
        }

        float speed01 = Mathf.InverseLerp(0f, sprintSpeed, speed);

        noiseComponent.m_AmplitudeGain = Mathf.Lerp(0.3f, bobAmplitude, speed01);
        noiseComponent.m_FrequencyGain = Mathf.Lerp(1.5f, bobFrequency, speed01);
    }

    private void ResetCameraBob()
    {
        noiseComponent.m_AmplitudeGain = 0f;
        noiseComponent.m_FrequencyGain = 0f;
    }

    ///////////////// FOOTSTEPS ////////////////////
    private void HandleFootsteps()
    {
        if (!controller.isGrounded) return;

        Vector3 velocity = controller.velocity;
        velocity.y = 0f;

        float speed = velocity.magnitude;
        if (speed < 0.2f) return;

        float speed01 = Mathf.InverseLerp(0f, sprintSpeed, speed);
        float interval = Mathf.Lerp(walkStepInterval, sprintStepInterval, speed01);

        if (Time.time >= nextStepTime)
        {
            PlayFootstep();
            nextStepTime = Time.time + interval;
        }
    }

    private void PlayFootstep()
    {
        AudioClip[] clips = GetFootstepClips();
        if (clips.Length == 0) return;

        footstepSound.PlayOneShot(clips[Random.Range(0, clips.Length)]);
    }

    private AudioClip[] GetFootstepClips()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1.5f, terrainLayerMask))
        {
            return hit.collider.tag switch
            {
                "Grass" => grassFootsteps,
                "Gravel" => gravelFootsteps,
                _ => groundFootsteps
            };
        }

        return groundFootsteps;
    }

    ///////////////// CAMERA LOOK ////////////////////
    private void LookAround()
    {
        float lookX = mouseX * mouseSensitivity * Time.deltaTime;
        float lookY = mouseY * mouseSensitivity * Time.deltaTime;

        xRotation -= lookY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        baseCameraRotation = Quaternion.Euler(xRotation, 0f, 0f);
        virtualCamera.transform.localRotation = baseCameraRotation;
        transform.Rotate(Vector3.up * lookX);
    }

    ///////////////// INPUT ////////////////////
    private void InputManagement()
    {
        moveInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");

        isSprinting = Input.GetKey(KeyCode.LeftShift) && moveInput > 0f;
    }
}
