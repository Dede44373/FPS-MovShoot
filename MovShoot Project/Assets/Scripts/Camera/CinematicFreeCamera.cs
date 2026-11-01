using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// A smooth free-fly camera for finding and recording establishing shots in Play mode.
///
/// Setup:
///  1. Create an empty object, add a Camera component and this script. Leave the object
///     active (the Camera component switches itself on and off). Don't tag it MainCamera.
///  2. Press Play and tap the Toggle Key (F8). Your Main Camera hands over to this one,
///     starting from the exact view it had, with the same post-processing.
///  3. Fly around, record with the Unity Recorder (Source: Game View), press F8 again to
///     hand control back.
///
/// Controls (old Input Manager, same as Invector):
///   Hold right mouse  look around           WASD            move
///   E / Q             up / down             Shift / Ctrl    fast / slow
///   Mouse wheel       change move speed     Alt + wheel     zoom (field of view)
///   Z / C             roll (dutch tilt)     X               level the roll
///   1 to 5            glide to a saved view Shift + 1 to 5  save the current view
///   P                 freeze time           H               hide / show all UI
///   F1                show / hide the help text
/// </summary>
[RequireComponent(typeof(Camera))]
public class CinematicFreeCamera : MonoBehaviour
{
    [System.Serializable]
    public class Bookmark
    {
        public bool Saved;
        public Vector3 Position;
        public Vector3 Euler;
        public float Fov = 60f;
    }

    [Header("Activation")]
    [SerializeField] private KeyCode ToggleKey = KeyCode.F8;
    [SerializeField] private bool StartActive = false;

    [Header("Movement")]
    [SerializeField] private float MoveSpeed = 6f;
    [SerializeField] private float FastMultiplier = 3f;
    [SerializeField] private float SlowMultiplier = 0.2f;
    [Tooltip("Higher = snappier, lower = floatier. 3 to 6 feels cinematic.")]
    [SerializeField] private float MoveSmoothing = 4f;

    [Header("Looking")]
    [SerializeField] private float LookSensitivity = 2f;
    [Tooltip("Higher = snappier, lower = slower, heavier pans.")]
    [SerializeField] private float LookSmoothing = 6f;
    [SerializeField] private float RollSpeed = 25f;

    [Header("Zoom")]
    [SerializeField] private float ZoomSmoothing = 6f;
    [SerializeField] private float MinFov = 15f;
    [SerializeField] private float MaxFov = 100f;

    [Header("Saved views (Shift + 1-5 saves, 1-5 glides to it)")]
    [SerializeField] private float GlideTime = 2.5f;
    [SerializeField] private Bookmark[] Bookmarks = new Bookmark[5];

    [Header("While active")]
    [Tooltip("Turns off the player's input and the follow camera so WASD doesn't also move the character.")]
    [SerializeField] private bool DisablePlayerControl = true;
    [Tooltip("The camera uses its own audio listener, so sound is heard from where the camera is.")]
    [SerializeField] private bool CameraIsAudioListener = true;
    [Tooltip("Extra components to switch off while this camera is in use.")]
    [SerializeField] private Behaviour[] ExtraDisable;
    [SerializeField] private bool ShowHelp = true;

    // Names of the Invector scripts that are switched off while flying.
    private static readonly string[] PlayerScriptNames = { "vThirdPersonInput", "vThirdPersonCamera" };

    private Camera m_cam;
    private Camera m_mainCam;
    private AudioListener m_mainListener;
    public AudioListener m_ownListener;
    private readonly List<Behaviour> m_disabled = new List<Behaviour>();
    private readonly List<Canvas> m_hiddenCanvases = new List<Canvas>();

    private bool m_active;
    private bool m_uiHidden;
    private bool m_frozen;
    private float m_savedTimeScale = 1f;

    private float m_yaw, m_pitch, m_roll;
    private float m_targetYaw, m_targetPitch, m_targetRoll;
    private float m_fov, m_targetFov;
    private Vector3 m_velocity;

    private bool m_gliding;
    private float m_glideT;
    private Vector3 m_glideFromPos, m_glideToPos;
    private Quaternion m_glideFromRot, m_glideToRot;
    private float m_glideFromFov, m_glideToFov;

    private void Awake()
    {
        m_cam = GetComponent<Camera>();
        m_cam.enabled = false;

        if (Bookmarks == null || Bookmarks.Length != 5)
        {
            Bookmark[] fresh = new Bookmark[5];
            for (int i = 0; i < 5; i++) { fresh[i] = (Bookmarks != null && i < Bookmarks.Length && Bookmarks[i] != null) ? Bookmarks[i] : new Bookmark(); }
            Bookmarks = fresh;
        }
    }

    private void Start()
    {
        if (StartActive) { SetActive(true); }
    }

    private void OnDisable()
    {
        if (m_active) { SetActive(false); }
    }

    private void Update()
    {
        if (Input.GetKeyDown(ToggleKey)) { SetActive(!m_active); }
        if (!m_active) { return; }

        // Unscaled, so the camera keeps working while time is frozen or the game is paused.
        float dt = Mathf.Min(Time.unscaledDeltaTime, 0.1f);

        HandleHotkeys();

        if (m_gliding)
        {
            UpdateGlide(dt);
            return;
        }

        UpdateLook(dt);
        UpdateMove(dt);
        UpdateZoom(dt);
    }

    //--------------------------------------
    // Switching on and off
    //--------------------------------------

    private void SetActive(bool on)
    {
        if (on == m_active) { return; }

        if (on)
        {
            m_mainCam = Camera.main;
            if (m_mainCam == m_cam)
            {
                Debug.LogError("[CinematicFreeCamera] This camera is tagged MainCamera. Change its tag to Untagged, so it can take over from your real main camera.", this);
                return;
            }

            if (m_mainCam != null)
            {
                // Same view, same lens, same post-processing as the camera it replaces.
                m_cam.CopyFrom(m_mainCam);
                transform.SetPositionAndRotation(m_mainCam.transform.position, m_mainCam.transform.rotation);

                UniversalAdditionalCameraData main = m_mainCam.GetUniversalAdditionalCameraData();
                UniversalAdditionalCameraData mine = m_cam.GetUniversalAdditionalCameraData();
                if (main != null && mine != null)
                {
                    mine.renderPostProcessing = main.renderPostProcessing;
                    mine.volumeLayerMask = main.volumeLayerMask;
                    mine.volumeTrigger = main.volumeTrigger != null ? main.volumeTrigger : m_mainCam.transform;
                    mine.antialiasing = main.antialiasing;
                }

                m_cam.depth = m_mainCam.depth + 10f;
                m_mainListener = m_mainCam.GetComponent<AudioListener>();
            }

            SyncFromTransform();
            m_fov = m_targetFov = m_cam.fieldOfView;

            m_cam.enabled = true;
            if (m_mainCam != null) { m_mainCam.enabled = false; }

            if (CameraIsAudioListener)
            {
                if (m_ownListener == null) { m_ownListener = gameObject.AddComponent<AudioListener>(); }
                m_ownListener.enabled = true;
                if (m_mainListener != null) { m_mainListener.enabled = false; }
            }

            DisableForFlying();
            m_savedTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;
            m_active = true;
        }
        else
        {
            m_active = false;
            m_gliding = false;

            if (m_frozen) { m_frozen = false; }
            Time.timeScale = m_savedTimeScale;

            ShowUI();
            RestoreDisabled();

            if (m_ownListener != null) { m_ownListener.enabled = false; }
            if (m_mainListener != null) { m_mainListener.enabled = true; }

            if (m_mainCam != null) { m_mainCam.enabled = true; }
            m_cam.enabled = false;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void DisableForFlying()
    {
        m_disabled.Clear();

        if (DisablePlayerControl)
        {
            MonoBehaviour[] all = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
            {
                MonoBehaviour b = all[i];
                if (b == null || !b.enabled) { continue; }

                string typeName = b.GetType().Name;
                for (int n = 0; n < PlayerScriptNames.Length; n++)
                {
                    if (typeName == PlayerScriptNames[n])
                    {
                        b.enabled = false;
                        m_disabled.Add(b);
                        break;
                    }
                }
            }
        }

        if (ExtraDisable != null)
        {
            for (int i = 0; i < ExtraDisable.Length; i++)
            {
                if (ExtraDisable[i] != null && ExtraDisable[i].enabled)
                {
                    ExtraDisable[i].enabled = false;
                    m_disabled.Add(ExtraDisable[i]);
                }
            }
        }
    }

    private void RestoreDisabled()
    {
        for (int i = 0; i < m_disabled.Count; i++)
        {
            if (m_disabled[i] != null) { m_disabled[i].enabled = true; }
        }
        m_disabled.Clear();
    }

    //--------------------------------------
    // Hotkeys: UI, freeze, saved views, help
    //--------------------------------------

    private void HandleHotkeys()
    {
        if (Input.GetKeyDown(KeyCode.H)) { if (m_uiHidden) { ShowUI(); } else { HideUI(); } }
        if (Input.GetKeyDown(KeyCode.F1)) { ShowHelp = !ShowHelp; }

        if (Input.GetKeyDown(KeyCode.P))
        {
            m_frozen = !m_frozen;
            Time.timeScale = m_frozen ? 0f : m_savedTimeScale;
        }

        bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        for (int i = 0; i < 5; i++)
        {
            if (!Input.GetKeyDown(KeyCode.Alpha1 + i)) { continue; }

            if (shift) { SaveBookmark(i); }
            else { GlideTo(i); }
        }
    }

    private void HideUI()
    {
        m_uiHidden = true;
        m_hiddenCanvases.Clear();

        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        for (int i = 0; i < canvases.Length; i++)
        {
            if (canvases[i] != null && canvases[i].enabled)
            {
                canvases[i].enabled = false;
                m_hiddenCanvases.Add(canvases[i]);
            }
        }
    }

    private void ShowUI()
    {
        m_uiHidden = false;

        for (int i = 0; i < m_hiddenCanvases.Count; i++)
        {
            if (m_hiddenCanvases[i] != null) { m_hiddenCanvases[i].enabled = true; }
        }
        m_hiddenCanvases.Clear();
    }

    private void SaveBookmark(int index)
    {
        Bookmark b = Bookmarks[index];
        b.Saved = true;
        b.Position = transform.position;
        b.Euler = transform.eulerAngles;
        b.Fov = m_cam.fieldOfView;
        Debug.Log("[CinematicFreeCamera] Saved view " + (index + 1) + ": position " + b.Position + ", rotation " + b.Euler + ", fov " + b.Fov.ToString("F0"), this);
    }

    private void GlideTo(int index)
    {
        Bookmark b = Bookmarks[index];
        if (b == null || !b.Saved) { return; }

        m_glideFromPos = transform.position;
        m_glideFromRot = transform.rotation;
        m_glideFromFov = m_cam.fieldOfView;
        m_glideToPos = b.Position;
        m_glideToRot = Quaternion.Euler(b.Euler);
        m_glideToFov = b.Fov;
        m_glideT = 0f;
        m_gliding = true;
        m_velocity = Vector3.zero;
    }

    private void UpdateGlide(float dt)
    {
        // Moving or looking yourself takes over from the glide.
        bool takeOver = Input.GetMouseButtonDown(1) ||
                        Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) ||
                        Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D);

        m_glideT += dt / Mathf.Max(0.05f, GlideTime);
        float t = Mathf.Clamp01(m_glideT);
        float eased = t * t * (3f - 2f * t);   // smoothstep: slow start, slow end

        transform.position = Vector3.Lerp(m_glideFromPos, m_glideToPos, eased);
        transform.rotation = Quaternion.Slerp(m_glideFromRot, m_glideToRot, eased);
        m_cam.fieldOfView = Mathf.Lerp(m_glideFromFov, m_glideToFov, eased);

        if (t >= 1f || takeOver)
        {
            m_gliding = false;
            SyncFromTransform();
            m_fov = m_targetFov = m_cam.fieldOfView;
        }
    }

    //--------------------------------------
    // Look, move, zoom (all smoothed)
    //--------------------------------------

    private void SyncFromTransform()
    {
        Vector3 e = transform.eulerAngles;
        m_yaw = m_targetYaw = e.y;
        m_pitch = m_targetPitch = e.x > 180f ? e.x - 360f : e.x;
        m_roll = m_targetRoll = e.z > 180f ? e.z - 360f : e.z;
        m_velocity = Vector3.zero;
    }

    private void UpdateLook(float dt)
    {
        bool looking = Input.GetMouseButton(1);

        if (looking)
        {
            m_targetYaw += Input.GetAxisRaw("Mouse X") * LookSensitivity;
            m_targetPitch -= Input.GetAxisRaw("Mouse Y") * LookSensitivity;
            m_targetPitch = Mathf.Clamp(m_targetPitch, -89f, 89f);
        }

        if (Input.GetKey(KeyCode.Z)) { m_targetRoll += RollSpeed * dt; }
        if (Input.GetKey(KeyCode.C)) { m_targetRoll -= RollSpeed * dt; }
        if (Input.GetKeyDown(KeyCode.X)) { m_targetRoll = 0f; }

        // Exponential smoothing: frame-rate independent, so pans stay silky at any FPS.
        float k = 1f - Mathf.Exp(-LookSmoothing * dt);
        m_yaw = Mathf.Lerp(m_yaw, m_targetYaw, k);
        m_pitch = Mathf.Lerp(m_pitch, m_targetPitch, k);
        m_roll = Mathf.Lerp(m_roll, m_targetRoll, k);

        transform.rotation = Quaternion.Euler(m_pitch, m_yaw, m_roll);
    }

    private void UpdateMove(float dt)
    {
        Vector3 input = new Vector3(
            Axis(KeyCode.D, KeyCode.A),
            Axis(KeyCode.E, KeyCode.Q),
            Axis(KeyCode.W, KeyCode.S));

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.0001f && !Input.GetKey(KeyCode.LeftAlt))
        {
            MoveSpeed = Mathf.Clamp(MoveSpeed * (1f + scroll * 3f), 0.2f, 100f);
        }

        float speed = MoveSpeed;
        if (Input.GetKey(KeyCode.LeftShift)) { speed *= FastMultiplier; }
        if (Input.GetKey(KeyCode.LeftControl)) { speed *= SlowMultiplier; }

        Vector3 wanted = transform.right * input.x + Vector3.up * input.y + transform.forward * input.z;
        if (wanted.sqrMagnitude > 1f) { wanted.Normalize(); }
        wanted *= speed;

        m_velocity = Vector3.Lerp(m_velocity, wanted, 1f - Mathf.Exp(-MoveSmoothing * dt));
        transform.position += m_velocity * dt;
    }

    private void UpdateZoom(float dt)
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Input.GetKey(KeyCode.LeftAlt) && Mathf.Abs(scroll) > 0.0001f)
        {
            m_targetFov = Mathf.Clamp(m_targetFov - scroll * 100f, MinFov, MaxFov);
        }

        m_fov = Mathf.Lerp(m_fov, m_targetFov, 1f - Mathf.Exp(-ZoomSmoothing * dt));
        m_cam.fieldOfView = m_fov;
    }

    private static float Axis(KeyCode positive, KeyCode negative)
    {
        float v = 0f;
        if (Input.GetKey(positive)) { v += 1f; }
        if (Input.GetKey(negative)) { v -= 1f; }
        return v;
    }

    //--------------------------------------
    // Help text
    //--------------------------------------

    private void OnGUI()
    {
        if (!m_active || !ShowHelp || m_uiHidden) { return; }

        GUI.Label(new Rect(12, 12, 520, 120),
            "FREE CAMERA   [F8] exit   [F1] hide this\n" +
            "Right mouse: look   WASD/E/Q: move   Shift/Ctrl: fast/slow   Wheel: speed (" + MoveSpeed.ToString("F1") + ")\n" +
            "Alt+Wheel: zoom   Z/C: roll   X: level   P: freeze time" + (m_frozen ? " (FROZEN)" : "") + "\n" +
            "1-5: glide to saved view   Shift+1-5: save view   H: hide UI");
    }
}
