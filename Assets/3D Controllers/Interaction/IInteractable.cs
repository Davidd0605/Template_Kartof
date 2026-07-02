using UnityEngine;

/// <summary>
/// Interface marking a component so the 
/// Interact3D script can recognize it.
/// Additionaly specifiyng behaviour on
/// interaction start and exit.
/// 
/// Author: Damyan
/// </summary>
public interface IInteractable 
{
    public void OnInteraction();
}
