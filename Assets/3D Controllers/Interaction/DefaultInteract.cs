using UnityEngine;

/// <summary>
/// Small example of IInteractable 
/// implementation.
/// 
/// Author: Damyan
/// </summary>
public class DefaultInteract : MonoBehaviour, IInteractable
{
    private string testName = "defaultInteractable";

    private bool entered = true;

    public void OnInteraction()
    {
        if(entered)
        {
            entered = false;
            GetComponent<Renderer>().material.SetColor("_BaseColor", Color.green);
            Debug.Log("Interaction started, my name is " + testName);
        } else
        {
            entered = true;
            GetComponent<Renderer>().material.SetColor("_BaseColor", Color.black);
            Debug.Log("Interaction exited");
        }
    }
}
