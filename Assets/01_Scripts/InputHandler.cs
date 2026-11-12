using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputHandler : MonoBehaviour
{
	[SerializeField] private Interactor interactor;

	private void Awake()
	{
		if (interactor == null)
			interactor = FindFirstObjectByType<Interactor>();
	}

	public void OnInteract(InputValue value) { interactor.TryInteract(value.isPressed); }
}
