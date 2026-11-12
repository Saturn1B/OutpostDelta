using UnityEngine;
using UnityEngine.InputSystem;

public class Interactor : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private Camera viewCamera;
    [SerializeField] private float rayDistance = 3f;

	private IInteractable current;

	public bool IsInteracting => current != null;

	public void TryInteract(bool isPressed)
	{
        if (viewCamera == null) return;

		if (isPressed)
		{
			//while camera is fix, ray come from mouse pose and not camera center, revert back once player movement is done
			Ray ray = /*new Ray(viewCamera.transform.position, viewCamera.transform.forward);*/viewCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

			if (!Physics.Raycast(ray, out RaycastHit hit, rayDistance)) return;

			if (hit.collider.GetComponentInParent<IInteractable>() is IInteractable target && target.CanInteract())
			{
				current = target;
				current.StartInteract();
			}
		}
		else
		{
			if (current != null)
			{
				current.EndInteract();
				current = null;
			}
		}
	}
}
