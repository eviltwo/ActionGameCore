using Steamworks;
using UnityEngine;

namespace ActionGameCoreSamples.Steamworks
{
    [DisallowMultipleComponent]
    public class SteamManager : MonoBehaviour
    {
#if SUPPORT_STEAMWORKS && !DISABLESTEAMWORKS

        private static bool _initialized = false;
        public static bool Initialized => _initialized;

        protected SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;

        private InputActionSetHandle_t _ingameActionSetHandle;
        private static InputHandle_t _inputHandleAllControllers = new InputHandle_t(Constants.STEAM_INPUT_HANDLE_ALL_CONTROLLERS);

        [AOT.MonoPInvokeCallback(typeof(SteamAPIWarningMessageHook_t))]
        protected static void SteamAPIDebugTextHook(int nSeverity, System.Text.StringBuilder pchDebugText)
        {
            Debug.LogWarning(pchDebugText);
        }

        private void Awake()
        {
            if (_initialized)
            {
                throw new System.Exception("Tried to Initialize the SteamAPI twice in one session!");
            }

            DontDestroyOnLoad(gameObject);

            if (!Packsize.Test())
            {
                Debug.LogError("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.", this);
            }

            if (!DllCheck.Test())
            {
                Debug.LogError("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.", this);
            }

            var initResult = SteamAPI.Init();
            if (!initResult)
            {
                Debug.LogError("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.", this);
                return;
            }

            SteamInput.Init(false);
            _ingameActionSetHandle = SteamInput.GetActionSetHandle("InGameControls");

            _initialized = true;
        }

        protected virtual void OnEnable()
        {
            if (!_initialized)
            {
                return;
            }

            if (m_SteamAPIWarningMessageHook == null)
            {
                m_SteamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(SteamAPIDebugTextHook);
                SteamClient.SetWarningMessageHook(m_SteamAPIWarningMessageHook);
            }
        }

        protected virtual void OnDestroy()
        {
            if (!_initialized)
            {
                return;
            }

            SteamInput.Shutdown();
            SteamAPI.Shutdown();
        }

        protected virtual void Update()
        {
            if (!_initialized)
            {
                return;
            }

            SteamAPI.RunCallbacks();
            SteamInput.ActivateActionSet(_inputHandleAllControllers, _ingameActionSetHandle);
        }
#else
	public static bool Initialized => false;
#endif
    }
}
