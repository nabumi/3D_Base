using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SystemicOverload.Phase1
{
    /// <summary>
    /// Samples a dedicated Phase 1 action map and exposes stable gameplay input state.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class InputProvider : MonoBehaviour
    {
        public enum ControlDeviceKind
        {
            None,
            KeyboardMouse,
            Gamepad,
            Other
        }

        public enum LookDeviceKind
        {
            None,
            Pointer,
            Gamepad,
            Other
        }

        private const string DefaultActionAssetResourcesPath = "Input/Phase1Gameplay";
        private const string DefaultActionAssetEditorPath = "Assets/Resources/Input/Phase1Gameplay.inputactions";
        private const string GameplayMapName = "Player";
        private const string MoveActionName = "Move";
        private const string LookActionName = "Look";
        private const string PointerPositionActionName = "PointerPosition";
        private const string ZoomActionName = "Zoom";
        private const string PrimaryHoldActionName = "PrimaryHold";
        private const string SecondaryHoldActionName = "SecondaryHold";
        private const string AttackActionName = "Attack";

        [Header("Action Asset")]
        [SerializeField] private InputActionAsset inputActionsAsset;
        [SerializeField] private string resourcesFallbackPath = DefaultActionAssetResourcesPath;

        [Header("Movement")]
        [SerializeField] private bool normalizeDiagonalInput = true;
        [SerializeField] private bool enableDualMouseForwardMove = true;
        [SerializeField] private float dualMouseForwardAmount = 1.0f;

        [Header("Gamepad")]
        [SerializeField] private float gamepadLookDeadzone = 0.15f;

        private InputActionAsset runtimeInputActions;
        private InputActionMap gameplayMap;
        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction pointerPositionAction;
        private InputAction zoomAction;
        private InputAction primaryHoldAction;
        private InputAction secondaryHoldAction;
        private InputAction attackAction;
        private bool callbacksBound;
        private bool initializationFailed;

        public Vector2 RawMoveInput { get; private set; }
        public Vector2 MoveInput { get; private set; }
        public Vector2 PointerScreenPosition { get; private set; }
        public Vector2 LookInput { get; private set; }
        public float ZoomDelta { get; private set; }
        public bool IsPrimaryHeld { get; private set; }
        public bool IsSecondaryHeld { get; private set; }
        public bool IsLeftMouseHeld => IsPrimaryHeld;
        public bool IsRightMouseHeld => IsSecondaryHeld;
        public bool IsDualMouseForwardHeld => IsDualInputForwardHeld;
        public bool IsDualInputForwardHeld => IsPrimaryHeld && IsSecondaryHeld;
        public ControlDeviceKind LastUsedDeviceKind { get; private set; }
        public LookDeviceKind CurrentLookDeviceKind { get; private set; }
        public Vector2 PointerLookDelta => CurrentLookDeviceKind == LookDeviceKind.Pointer ? LookInput : Vector2.zero;
        public Vector2 GamepadLookInput => CurrentLookDeviceKind == LookDeviceKind.Gamepad ? LookInput : Vector2.zero;
        public bool HasGamepadLookInput => GamepadLookInput.sqrMagnitude > gamepadLookDeadzone * gamepadLookDeadzone;
        public bool IsUsingGamepad => LastUsedDeviceKind == ControlDeviceKind.Gamepad;
        public bool ShouldAlignCharacterToCamera => IsSecondaryHeld || HasGamepadLookInput;
        public bool ShouldBlockPointerFacing => IsPrimaryHeld && !HasGamepadLookInput;
        public bool IsCameraLookHeld => IsPrimaryHeld || IsSecondaryHeld;

        /// <summary>
        /// 이번 프레임에 공격 입력이 눌렸는지 여부입니다. Phase 1 전용 씬에서 Attack 액션이 없으면 항상 false입니다.
        /// </summary>
        public bool WasAttackPressedThisFrame { get; private set; }

        private void Reset()
        {
            TryAssignDefaultAssetInEditor();
        }

        private void OnValidate()
        {
            dualMouseForwardAmount = Mathf.Max(0.0f, dualMouseForwardAmount);
            gamepadLookDeadzone = Mathf.Clamp01(gamepadLookDeadzone);
            if (string.IsNullOrWhiteSpace(resourcesFallbackPath))
            {
                resourcesFallbackPath = DefaultActionAssetResourcesPath;
            }

            TryAssignDefaultAssetInEditor();
        }

        private void OnEnable()
        {
            if (!EnsureInputActionsInitialized())
            {
                return;
            }

            BindCallbacks();
            gameplayMap.Enable();
            SampleActions();
        }

        private void Update()
        {
            if (!EnsureInputActionsInitialized())
            {
                return;
            }

            SampleActions();
        }

        private void OnDisable()
        {
            if (gameplayMap != null)
            {
                gameplayMap.Disable();
            }

            UnbindCallbacks();
            ClearRuntimeState();
        }

        private void OnDestroy()
        {
            if (runtimeInputActions != null)
            {
                Destroy(runtimeInputActions);
                runtimeInputActions = null;
            }
        }

        private bool EnsureInputActionsInitialized()
        {
            if (runtimeInputActions != null)
            {
                return true;
            }

            if (initializationFailed)
            {
                return false;
            }

            InputActionAsset sourceAsset = ResolveSourceAsset();
            if (sourceAsset == null)
            {
                initializationFailed = true;
                Debug.LogError("InputProvider could not resolve a Phase 1 InputActionAsset.", this);
                return false;
            }

            runtimeInputActions = Instantiate(sourceAsset);
            gameplayMap = runtimeInputActions.FindActionMap(GameplayMapName, true);
            moveAction = gameplayMap.FindAction(MoveActionName, true);
            lookAction = gameplayMap.FindAction(LookActionName, true);
            pointerPositionAction = gameplayMap.FindAction(PointerPositionActionName, true);
            zoomAction = gameplayMap.FindAction(ZoomActionName, true);
            primaryHoldAction = gameplayMap.FindAction(PrimaryHoldActionName, true);
            secondaryHoldAction = gameplayMap.FindAction(SecondaryHoldActionName, true);
            attackAction = gameplayMap.FindAction(AttackActionName, false);
            if (attackAction == null)
            {
                Debug.LogWarning(
                    $"InputProvider: '{AttackActionName}' 액션을 찾을 수 없습니다. Phase1Gameplay.inputactions를 갱신했는지 확인하세요.",
                    this);
            }

            return true;
        }

        private void BindCallbacks()
        {
            if (callbacksBound)
            {
                return;
            }

            moveAction.performed += OnGameplayActionPerformed;
            lookAction.performed += OnGameplayActionPerformed;
            pointerPositionAction.performed += OnGameplayActionPerformed;
            zoomAction.performed += OnGameplayActionPerformed;
            primaryHoldAction.performed += OnGameplayActionPerformed;
            secondaryHoldAction.performed += OnGameplayActionPerformed;
            if (attackAction != null)
            {
                attackAction.performed += OnGameplayActionPerformed;
            }

            callbacksBound = true;
        }

        private void UnbindCallbacks()
        {
            if (!callbacksBound || moveAction == null)
            {
                callbacksBound = false;
                return;
            }

            moveAction.performed -= OnGameplayActionPerformed;
            lookAction.performed -= OnGameplayActionPerformed;
            pointerPositionAction.performed -= OnGameplayActionPerformed;
            zoomAction.performed -= OnGameplayActionPerformed;
            primaryHoldAction.performed -= OnGameplayActionPerformed;
            secondaryHoldAction.performed -= OnGameplayActionPerformed;
            if (attackAction != null)
            {
                attackAction.performed -= OnGameplayActionPerformed;
            }

            callbacksBound = false;
        }

        private void OnGameplayActionPerformed(InputAction.CallbackContext context)
        {
            LastUsedDeviceKind = ClassifyControlDevice(context.control.device);
        }

        private void SampleActions()
        {
            IsPrimaryHeld = primaryHoldAction.IsPressed();
            IsSecondaryHeld = secondaryHoldAction.IsPressed();

            RawMoveInput = moveAction.ReadValue<Vector2>();
            MoveInput = ApplyDualMouseForward(PrepareMoveInput(RawMoveInput));

            Vector2 pointerPosition = pointerPositionAction.ReadValue<Vector2>();
            PointerScreenPosition = pointerPosition.sqrMagnitude > 0.0f
                ? pointerPosition
                : new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

            LookInput = lookAction.ReadValue<Vector2>();
            CurrentLookDeviceKind = ClassifyLookDevice(lookAction.activeControl?.device);
            ZoomDelta = zoomAction.ReadValue<float>();
            WasAttackPressedThisFrame = attackAction != null && attackAction.WasPressedThisFrame();
        }

        private Vector2 PrepareMoveInput(Vector2 sourceMoveInput)
        {
            if (normalizeDiagonalInput && sourceMoveInput.sqrMagnitude > 1.0f)
            {
                sourceMoveInput.Normalize();
            }

            return sourceMoveInput;
        }

        private Vector2 ApplyDualMouseForward(Vector2 sourceMoveInput)
        {
            if (!enableDualMouseForwardMove || !IsDualInputForwardHeld)
            {
                return sourceMoveInput;
            }

            Vector2 composedInput = sourceMoveInput + Vector2.up * dualMouseForwardAmount;
            if (normalizeDiagonalInput && composedInput.sqrMagnitude > 1.0f)
            {
                composedInput.Normalize();
            }

            return composedInput;
        }

        private void ClearRuntimeState()
        {
            RawMoveInput = Vector2.zero;
            MoveInput = Vector2.zero;
            PointerScreenPosition = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            LookInput = Vector2.zero;
            ZoomDelta = 0.0f;
            IsPrimaryHeld = false;
            IsSecondaryHeld = false;
            LastUsedDeviceKind = ControlDeviceKind.None;
            CurrentLookDeviceKind = LookDeviceKind.None;
            WasAttackPressedThisFrame = false;
        }

        private InputActionAsset ResolveSourceAsset()
        {
            if (inputActionsAsset != null)
            {
                return inputActionsAsset;
            }

            if (!string.IsNullOrWhiteSpace(resourcesFallbackPath))
            {
                inputActionsAsset = Resources.Load<InputActionAsset>(resourcesFallbackPath);
            }

            return inputActionsAsset;
        }

        private static LookDeviceKind ClassifyLookDevice(InputDevice device)
        {
            if (device == null)
            {
                return LookDeviceKind.None;
            }

            if (device is Mouse || device is Pen || device is Touchscreen)
            {
                return LookDeviceKind.Pointer;
            }

            if (device is Gamepad)
            {
                return LookDeviceKind.Gamepad;
            }

            return LookDeviceKind.Other;
        }

        private static ControlDeviceKind ClassifyControlDevice(InputDevice device)
        {
            if (device == null)
            {
                return ControlDeviceKind.None;
            }

            if (device is Gamepad)
            {
                return ControlDeviceKind.Gamepad;
            }

            if (device is Keyboard || device is Mouse || device is Pen || device is Touchscreen)
            {
                return ControlDeviceKind.KeyboardMouse;
            }

            return ControlDeviceKind.Other;
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void TryAssignDefaultAssetInEditor()
        {
            if (inputActionsAsset != null)
            {
                return;
            }

#if UNITY_EDITOR
            inputActionsAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(DefaultActionAssetEditorPath);
#endif
        }
    }
}
