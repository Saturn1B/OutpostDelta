using UnityEngine;

public interface IInteractable
{
	public void StartInteract();
	public void EndInteract();
	public bool CanInteract();
}
