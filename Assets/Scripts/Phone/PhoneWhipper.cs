using System;
using UnityEngine;
using UnityEngine.InputSystem;
public class PhoneWhipper : MonoBehaviour
{   

    [SerializeField] private PhoneUIController phoneUI;
    [SerializeField] private MovementController3D movementController;

    private bool phoneOpen = false;

    public void OnTogglePhone(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (!phoneOpen)
        {
            if (!movementController.enabled) return; // refuse to open if movement is already locked (e.g. FocusInteract active)
            Debug.Log("whipped that shit out");
            phoneOpen = true;
        }
        else
        {
            phoneOpen = false;
        }

        phoneUI.TogglePhone(phoneOpen);
        movementController.enabled = !phoneOpen;
    }

        public void debug2FAEvent(InputAction.CallbackContext context)
    {   
        if (!context.performed) return;
        EventBus<Phone2FAEvent>.Raise(new Phone2FAEvent{correctNumber = 42});
        Debug.Log("2FA number event raised");
    }
}
