using UnityEngine;
using UnityEngine.InputSystem;


public class Knob : MonoBehaviour, IInteractable
{
	[Header("Rotation")]
	[SerializeField] private Vector3 rotationAxis = Vector3.up;
	[SerializeField] private float minAngle = -135f;
	[SerializeField] private float maxAngle = 135f;
	[SerializeField] private float degreesPerPixel = 0.5f;
	[SerializeField] private bool invert = false;

	[Header("Output")]
	[Range(0f, 1f)] public float knobValue;
	public bool isChanging;
	public bool isHeld;

	private float currentAngle;
	private Quaternion startRotation;

    private void Awake()
    {
        startRotation = transform.localRotation;
        currentAngle = Mathf.Lerp(minAngle, maxAngle, knobValue);
        ApplyRotation();
    }

    private void Update()
    {
        isChanging = false;
        if (!isHeld || Mouse.current == null) return;

        float delta = Mouse.current.delta.ReadValue().x * degreesPerPixel;
        if (invert) delta = -delta;
        if (Mathf.Approximately(delta, 0f)) return;

        float newAngle = Mathf.Clamp(currentAngle + delta, minAngle, maxAngle);
        if (Mathf.Approximately(newAngle, currentAngle)) return; // hit a limit, nothing changed

        currentAngle = newAngle;
        knobValue = Mathf.InverseLerp(minAngle, maxAngle, currentAngle);
        isChanging = true;
        ApplyRotation();
    }

    private void ApplyRotation()
    {
        transform.localRotation = startRotation * Quaternion.AngleAxis(-currentAngle, rotationAxis);
    }

    public bool CanInteract() => true;

    public void StartInteract() => isHeld = true;

    public void EndInteract()
    {
        isHeld = false;
        isChanging = false;
    }
}
