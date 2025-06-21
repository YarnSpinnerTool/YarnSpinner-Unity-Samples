using UnityEngine;

#nullable enable

// Disable this warning, because depending on the compiler defines, certain
// variables may not end up getting accessed
#pragma warning disable CS0414 // The field 'X' is assigned but its value is never used

namespace Yarn.Unity.Samples
{

    public abstract class InputAxisBase
    {
#if USE_INPUTSYSTEM && ENABLE_INPUT_SYSTEM
        [SerializeField] protected UnityEngine.InputSystem.InputActionReference? inputActionReference;
#else
        // If the input system isn't available, don't drop the reference to any
        // previously-configured input action
        [SerializeField] protected ScriptableObject? inputActionReference;
#endif

        public void Enable()
        {
#if USE_INPUTSYSTEM && ENABLE_INPUT_SYSTEM
            if (inputActionReference != null && inputActionReference.action != null)
            {
                inputActionReference.action.Enable();
            }
#endif
        }
    }

    [System.Serializable]
    public class InputAxisVector2 : InputAxisBase
    {
        [SerializeField] string? legacyInputHorizontalAxis = "Horizontal";
        [SerializeField] string? legacyInputVerticalAxis = "Vertical";

        public Vector2 Value
        {
            get
            {
#if USE_INPUTSYSTEM && ENABLE_INPUT_SYSTEM
                if (inputActionReference != null && inputActionReference.action != null)
                {
                    return inputActionReference.action.ReadValue<Vector2>();
                }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
                if (!(string.IsNullOrEmpty(legacyInputHorizontalAxis) || string.IsNullOrEmpty(legacyInputVerticalAxis)))
                {
                    return new Vector2(
                        Input.GetAxisRaw(legacyInputHorizontalAxis),
                        Input.GetAxisRaw(legacyInputVerticalAxis)
                    );
                }
#endif
                return Vector2.zero;
            }
        }
    }

    [System.Serializable]
    public class InputAxisButton : InputAxisBase
    {
        [SerializeField] string? legacyInputAxis = "Jump";

        public bool WasPressedThisFrame
        {
            get
            {
#if USE_INPUTSYSTEM && ENABLE_INPUT_SYSTEM
                if (inputActionReference != null && inputActionReference.action != null)
                {
                    return inputActionReference.action.WasPressedThisFrame();
                }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
                if (!string.IsNullOrEmpty(legacyInputAxis))
                {
                    return Input.GetButtonDown(legacyInputAxis);
                }
#endif
                return false;
            }
        }

        public bool IsPressed
        {
            get
            {
#if USE_INPUTSYSTEM && ENABLE_INPUT_SYSTEM
                if (inputActionReference != null && inputActionReference.action != null)
                {
                    return inputActionReference.action.IsPressed();
                }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
                if (!string.IsNullOrEmpty(legacyInputAxis))
                {
                    return Input.GetButton(legacyInputAxis);
                }
#endif
                return false;
            }
        }

    }
}