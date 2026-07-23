using System.Collections;
using UnityEngine;

/// <summary>
/// IInteractable that glides the player camera toward a focus
/// point relative to this object, and glides back to the
/// original position/rotation on second interaction.
/// Disables MovementController3D while focused so it doesn't
/// fight the glide by overwriting camera transform every frame.
/// </summary>
public class FocusInteract : MonoBehaviour, IInteractable
{
    [Header("Focus Target")]
    [SerializeField] private Transform focusPoint; // empty child transform marking where the camera should end up
    [SerializeField] private float glideDuration = 0.6f;

    [Header("References")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private MovementController3D movementController;

    private bool focused = false;

    private Vector3 originalWorldPos;
    private Quaternion originalWorldRot;
    private Transform originalParent;

    private Coroutine glideCoroutine;

    public void OnInteraction()
    {
        if (!focused)
        {
            focused = true;

            // Remember where the camera was so we can return to it
            originalWorldPos = targetCamera.transform.position;
            originalWorldRot = targetCamera.transform.rotation;
            originalParent = targetCamera.transform.parent;

            movementController.enabled = false;

            // Detach camera from player while focused so animating world position/rotation is clean
            targetCamera.transform.SetParent(null, true);

            if (glideCoroutine != null) StopCoroutine(glideCoroutine);
            glideCoroutine = StartCoroutine(Glide(focusPoint.position, focusPoint.rotation, null));
        }
        else
        {
            focused = false;

            if (glideCoroutine != null) StopCoroutine(glideCoroutine);
            glideCoroutine = StartCoroutine(Glide(originalWorldPos, originalWorldRot, () =>
            {
                targetCamera.transform.SetParent(originalParent, true);
                movementController.enabled = true;
            }));
        }
    }

    private IEnumerator Glide(Vector3 targetPos, Quaternion targetRot, System.Action onComplete)
    {
        Vector3 startPos = targetCamera.transform.position;
        Quaternion startRot = targetCamera.transform.rotation;
        float t = 0f;

        while (t < glideDuration)
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / glideDuration);

            targetCamera.transform.position = Vector3.Lerp(startPos, targetPos, normalized);
            targetCamera.transform.rotation = Quaternion.Slerp(startRot, targetRot, normalized);

            yield return null;
        }

        targetCamera.transform.position = targetPos;
        targetCamera.transform.rotation = targetRot;

        onComplete?.Invoke();
    }
}