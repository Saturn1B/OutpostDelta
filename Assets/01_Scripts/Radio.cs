using UnityEngine;

public class Radio : MonoBehaviour
{
	[SerializeField] private Knob knob;
	[SerializeField] private float limMin, limMax;
	[SerializeField] private GameObject arrow;

	private void Start()
	{
		if (limMin == 0) limMin = arrow.transform.localPosition.x - .45f;
		if (limMax == 0) limMax = arrow.transform.localPosition.x + .45f;

		arrow.transform.localPosition = new Vector3(map(knob.knobValue, 0, 1, limMin, limMax), arrow.transform.localPosition.y, arrow.transform.localPosition.z);
	}

	private void Update()
	{
		if (!knob.isChanging) return;

		arrow.transform.localPosition = new Vector3( map(knob.knobValue, 0, 1, limMin, limMax), arrow.transform.localPosition.y, arrow.transform.localPosition.z);
	}

	float map(float s, float a1, float a2, float b1, float b2)
	{
		return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
	}
}
