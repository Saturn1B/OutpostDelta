using UnityEngine;

[CreateAssetMenu(menuName = "Radio/Shape")]
public class ShapeAsset : ScriptableObject
{
	public Vector2[] points;
	const int maxK = 64;

	Vector2[] coef;
	public int maxHarmonic => maxK;

	void BuildCoefficients()
	{
		int n = points.Length;
		coef = new Vector2[2 * maxK + 1];
		for (int k = -maxK; k <= maxK; k++)
		{
			float re = 0, im = 0;
			for (int j = 0; j < n; j++)
			{
				float a = -2f * Mathf.PI * k * j / n;
				float c = Mathf.Cos(a), s = Mathf.Sin(a);
				// (x + iy)(cos a + i sin a)
				re += points[j].x * c - points[j].y * s;
				im += points[j].x * s + points[j].y * c;
			}
			coef[k + maxK] = new Vector2(re / n, im / n);
		}
	}

	public Vector2 Sample(float u, int K)
	{
		if (points == null || points.Length < 3) return Vector2.zero;
		if (coef == null || coef.Length != 2 * maxK + 1) BuildCoefficients();

		K = Mathf.Clamp(K, 0, maxK);
		float x = 0, y = 0;
		for (int k = -K; k <= K; k++)
		{
			Vector2 c = coef[k + maxK];
			float a = 2f * Mathf.PI * k * u;
			float cs = Mathf.Cos(a), sn = Mathf.Sin(a);
			x += c.x * cs - c.y * sn;
			y += c.x * sn + c.y * cs;
		}
		return new Vector2(x, y);
	}

	[ContextMenu("Generate Heart")]
	void GenerateHeart()
	{
		int n = 256;
		points = new Vector2[n];
		for (int i = 0; i < n; i++)
		{
			float t = i / (float)n * Mathf.PI * 2f;
			float x = 16f * Mathf.Pow(Mathf.Sin(t), 3);
			float y = 13f * Mathf.Cos(t) - 5f * Mathf.Cos(2 * t)
					- 2f * Mathf.Cos(3 * t) - Mathf.Cos(4 * t);
			points[i] = new Vector2(x, y) / 17f;   // roughly -1..1
		}
		coef = null;   // force the Fourier coefficients to be rebuilt
#if UNITY_EDITOR
		UnityEditor.EditorUtility.SetDirty(this);
#endif
	}
}
