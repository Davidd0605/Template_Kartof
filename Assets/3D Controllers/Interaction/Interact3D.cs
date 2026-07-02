using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Class facilitating the interaction of the runner
/// with interactables. It is only responsible for 
/// triggering an interaction with the objects, not
/// for remembering any state, that is the responsibility 
/// of each object(as each object migth want to define different behavior).
/// 
/// Author: Damyan
/// </summary>
public class Interact3D : MonoBehaviour
{
    [Header("Interaction Parameters")]
    [SerializeField] private float interactionDistance = 10f;

    [Header("State")]
    private IInteractable currentInteractable;

    [Header("Input values")]
    private bool interactRequested = false;

    [Header("References")]
    [SerializeField] private Camera runnerCamera;
    [SerializeField] private LayerMask interactablesMask;
    [SerializeField] private GameObject uiRef; // to be improved upon(events? or however we handle UI)
    
    private void Update()
    {

        if(!CanInteract()) return;

        if (interactRequested )
        {
            currentInteractable?.OnInteraction();

            // Consume the request so it only triggers once
            interactRequested = false;
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            interactRequested = true;
        }
    }

    /// <summary>
    /// Method that checks if any interactable is on the
    /// crossair and stores it for potential interaction.
    /// Also for now updates UI.
    /// </summary>
    /// <returns></returns>
    private bool CanInteract()
    {
        Ray ray = new Ray(runnerCamera.transform.position, runnerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance, interactablesMask))
        {
            if(hit.collider.TryGetComponent<IInteractable>(out currentInteractable))
            {
                uiRef.SetActive(true);
            } else
            {
                uiRef.SetActive(false);
            }
            return true;
        }
        else
        {
            uiRef.SetActive(false);
            return false;
        }
    }
}
