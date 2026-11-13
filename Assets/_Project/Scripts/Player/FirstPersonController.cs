using ExtensaoZero.Audio;
using ExtensaoZero.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ExtensaoZero.Player
{
    /// <summary>
    /// Controlador em primeira pessoa. Movimento local; em rede o dono envia a posição
    /// ao host, que replica. Não há física de ragdoll nem perseguição.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class FirstPersonController : MonoBehaviour
    {
        public Camera Camera { get; private set; }
        public Transform CameraTransform { get; private set; }
        public CharacterController Character { get; private set; }
        public bool IsRunning { get; private set; }
        public bool InputEnabled { get; private set; } = true;

        public float walkSpeed = 2.55f;
        public float runSpeed = 4.15f;
        public float gravity = -18f;
        public float mouseSensitivity = 1.4f;
        public float fieldOfView = 72f;
        public bool invertY;
        public float headBobAmount = 0.028f;

        private float m_Pitch;
        private float m_VerticalVelocity;
        private float m_BobTime;
        private float m_NextFootstep;
        private Vector3 m_CameraBase;
        private bool m_Remote;

        private void Awake()
        {
            Character = GetComponent<CharacterController>();
            Character.height = 1.8f;
            Character.radius = 0.32f;
            Character.center = new Vector3(0f, 0.9f, 0f);
            Character.slopeLimit = 45f;
            Character.stepOffset = 0.3f;

            var cameraGo = new GameObject("PlayerCamera");
            cameraGo.transform.SetParent(transform, false);
            cameraGo.transform.localPosition = new Vector3(0f, 1.62f, 0f);
            Camera = cameraGo.AddComponent<Camera>();
            Camera.fieldOfView = fieldOfView;
            Camera.nearClipPlane = 0.03f;
            Camera.farClipPlane = 90f;
            // Céu noturno sólido na cor da névoa: o horizonte some na neblina.
            Camera.clearFlags = CameraClearFlags.SolidColor;
            Camera.backgroundColor = Environment.LightingRig.FogColor;
            Camera.tag = "MainCamera";
            cameraGo.AddComponent<AudioListener>();
            CameraTransform = cameraGo.transform;
            m_CameraBase = cameraGo.transform.localPosition;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Start()
        {
            ApplySettings();
        }

        public void ApplySettings()
        {
            var settings = GameManager.Instance != null ? GameManager.Instance.Settings : null;
            if (settings != null)
            {
                mouseSensitivity = settings.mouseSensitivity;
                invertY = settings.invertY;
                fieldOfView = settings.fieldOfView;
            }

            if (Camera != null) Camera.fieldOfView = fieldOfView;
        }

        public void SetInputEnabled(bool enabled)
        {
            InputEnabled = enabled;
        }

        public void SetNetworkControlled(bool owner)
        {
            m_Remote = !owner;
            if (Character != null) Character.enabled = owner;
            if (Camera != null) Camera.gameObject.SetActive(owner);
            SetInputEnabled(owner);
        }

        public void SetCameraActive(bool active)
        {
            if (Camera != null) Camera.gameObject.SetActive(active);
        }

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            if (Character != null) Character.enabled = false;
            transform.SetPositionAndRotation(position, rotation);
            if (Character != null) Character.enabled = !m_Remote;
            m_Pitch = 0f;
            m_VerticalVelocity = 0f;
        }

        public void SetFieldOfView(float value)
        {
            fieldOfView = Mathf.Clamp(value, 60f, 100f);
            if (Camera != null) Camera.fieldOfView = fieldOfView;
        }

        public void SetSensitivity(float value)
        {
            mouseSensitivity = Mathf.Clamp(value, 0.3f, 4f);
        }

        public void SetInvertY(bool value)
        {
            invertY = value;
        }

        private void Update()
        {
            if (m_Remote) return;
            var manager = GameManager.Instance;
            var canLook = InputEnabled && Cursor.lockState == CursorLockMode.Locked;
            if (!canLook) return;
            if (manager != null && manager.State != GameState.Playing) return;

            Look();
            Move();
        }

        private void Look()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;
            var delta = mouse.delta.ReadValue() * (mouseSensitivity * 0.09f);
            var yaw = delta.x;
            m_Pitch += delta.y * (invertY ? 1f : -1f);
            m_Pitch = Mathf.Clamp(m_Pitch, -88f, 88f);
            transform.Rotate(0f, yaw, 0f, Space.Self);
            if (CameraTransform != null)
            {
                CameraTransform.localRotation = Quaternion.Euler(m_Pitch, 0f, 0f);
            }
        }

        private void Move()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || Character == null) return;

            var x = 0f;
            var z = 0f;
            if (keyboard.aKey.isPressed) x -= 1f;
            if (keyboard.dKey.isPressed) x += 1f;
            if (keyboard.sKey.isPressed) z -= 1f;
            if (keyboard.wKey.isPressed) z += 1f;

            var move = transform.right * x + transform.forward * z;
            if (move.sqrMagnitude > 1f) move.Normalize();

            IsRunning = keyboard.leftShiftKey.isPressed && z > 0f;
            var speed = IsRunning ? runSpeed : walkSpeed;
            if (Character.isGrounded && m_VerticalVelocity < 0f) m_VerticalVelocity = -2f;
            m_VerticalVelocity += gravity * Time.deltaTime;

            var velocity = move * speed + Vector3.up * m_VerticalVelocity;
            Character.Move(velocity * Time.deltaTime);

            HeadBob(move.sqrMagnitude, speed);
        }

        private void HeadBob(float moving, float speed)
        {
            if (CameraTransform == null) return;
            if (moving > 0.01f && Character.isGrounded)
            {
                m_BobTime += Time.deltaTime * (IsRunning ? 11f : 7.2f);
                var offset = new Vector3(
                    Mathf.Cos(m_BobTime * 0.5f) * headBobAmount * 0.5f,
                    Mathf.Sin(m_BobTime) * headBobAmount,
                    0f);
                CameraTransform.localPosition = Vector3.Lerp(CameraTransform.localPosition, m_CameraBase + offset, 12f * Time.deltaTime);

                if (Time.time >= m_NextFootstep)
                {
                    m_NextFootstep = Time.time + (IsRunning ? 0.32f : 0.48f);
                    var audio = GameManager.Instance != null ? GameManager.Instance.Audio : null;
                    audio?.PlayWorldOneShot(ProceduralAudio.Footstep(), transform.position, 0.22f, AudioCategory.Sfx);
                }
            }
            else
            {
                CameraTransform.localPosition = Vector3.Lerp(CameraTransform.localPosition, m_CameraBase, 9f * Time.deltaTime);
            }
        }
    }
}
