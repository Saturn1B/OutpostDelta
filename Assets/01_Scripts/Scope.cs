using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Scope : MonoBehaviour
{
	public LevelRadioConfig config;
	//[Range(0, 1)] public float knob;
	[SerializeField] private Knob knob;
	public int samples = 512;
	public float scrollSpeed = 2f;
	public float shapeSmoothing = 6f;

	LineRenderer lr;
	float phase;
	float imgAmt;
	ShapeAsset currentShape;

	public float Frequency { get; private set; }

	void Awake() { lr = GetComponent<LineRenderer>(); lr.positionCount = samples; }

	float side = 1;

	void Update()
	{
		Frequency = Mathf.Lerp(config.minFreq, config.maxFreq, knob.knobValue);

		ShapeAsset targetShape = null;
		float target = 0f;

		switch (config._shapeMode)
		{
			case ShapeMode.Everywhere:
				targetShape = config.globalShape; target = 1f;
				break;

			case ShapeMode.OnStations:
				foreach (var s in config.stations)
				{
					if (s.shape == null) continue;
					float d = (Frequency - s.frequency) / s.width;
					float w = Mathf.Exp(-d * d);
					if (w < 0.001f) continue;
					if (w > target) { target = w; targetShape = s.shape; }
				}
				break;
		}

		if (targetShape != null) currentShape = targetShape;
		imgAmt = Mathf.MoveTowards(imgAmt, target, shapeSmoothing * Time.deltaTime);
		if (imgAmt <= 0f) currentShape = null;

		Draw();
	}

	void Draw()
	{
		float cycles = Mathf.Lerp(config.minCycles, config.maxCycles, knob.knobValue);
		phase += Time.deltaTime * scrollSpeed;

		int K = currentShape
			? Mathf.RoundToInt(Mathf.Lerp(2, currentShape.maxHarmonic, imgAmt * imgAmt))
			: 0;

		for (int i = 0; i < samples; i++)
		{
			float u = i / (float)(samples - 1);
			float s = Mathf.Sin(u * Mathf.PI * 2f * cycles + phase);

			Vector2 p = new Vector2(u * 2f - 1f, s);

			if (currentShape != null)
			{
				Vector2 xy = currentShape.Sample(u, K);
				xy.y += s * 0.04f * (1f - imgAmt);
				p = Vector2.Lerp(p, xy, imgAmt);
			}
			lr.SetPosition(i, p);
		}
	}

	void OnGUI() => GUI.Label(new Rect(10, 10, 300, 30), $"{Frequency:F1} MHz");
}