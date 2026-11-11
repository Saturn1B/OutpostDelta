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

    [Header("Import from image")]
    public Texture2D sourceImage;
    [Range(0f, 1f)] public float threshold = 0.5f;   // pixels darker than this count as "shape"
    public bool invert;                              // tick if your shape is white on black
    public int pointCount = 256;

    static readonly Vector2Int[] dirs = {            // clockwise, y up
    new Vector2Int(-1, 0), new Vector2Int(-1, 1), new Vector2Int(0, 1), new Vector2Int(1, 1),
    new Vector2Int(1, 0),  new Vector2Int(1, -1), new Vector2Int(0, -1), new Vector2Int(-1, -1)
};

    [ContextMenu("Import From Image")]
    public void ImportFromImage()
    {
        if (sourceImage == null) { Debug.LogError("Assign a source image first."); return; }

        int w = sourceImage.width, h = sourceImage.height;
        Color[] px;
        try { px = sourceImage.GetPixels(); }
        catch { Debug.LogError("Enable Read/Write on the texture import settings."); return; }

        bool[] ink = new bool[w * h];
        for (int i = 0; i < px.Length; i++)
        {
            float lum = px[i].grayscale;
            bool dark = lum < threshold && px[i].a > 0.5f;
            ink[i] = invert ? (px[i].a > 0.5f && lum >= threshold) : dark;
        }

        bool Ink(int x, int y) => x >= 0 && y >= 0 && x < w && y < h && ink[y * w + x];

        // first shape pixel, scanning bottom to top
        Vector2Int start = new Vector2Int(-1, -1);
        for (int y = 0; y < h && start.x < 0; y++)
            for (int x = 0; x < w; x++)
                if (Ink(x, y)) { start = new Vector2Int(x, y); break; }

        if (start.x < 0) { Debug.LogError("No shape pixels found. Try 'invert' or change the threshold."); return; }

        // Moore-neighbour contour tracing
        var contour = new System.Collections.Generic.List<Vector2>();
        Vector2Int c = start;
        int b = 0;                                   // we "arrived" from the west
        contour.Add(c);
        for (int guard = 0; guard < w * h * 2; guard++)
        {
            bool moved = false;
            for (int i = 1; i <= 8; i++)
            {
                int d = (b + i) % 8;
                Vector2Int n = c + dirs[d];
                if (!Ink(n.x, n.y)) continue;

                Vector2Int prev = c + dirs[(d + 7) % 8];     // the empty pixel checked just before
                Vector2Int diff = prev - n;
                for (int k = 0; k < 8; k++) if (dirs[k] == diff) { b = k; break; }

                c = n; moved = true;
                break;
            }
            if (!moved || c == start) break;
            contour.Add(c);
        }

        if (contour.Count < 8) { Debug.LogError("Contour too small."); return; }

        // resample evenly along the closed path
        int N = Mathf.Max(32, pointCount);
        int m = contour.Count;
        float[] cum = new float[m + 1];
        for (int i = 0; i < m; i++)
            cum[i + 1] = cum[i] + Vector2.Distance(contour[i], contour[(i + 1) % m]);
        float total = cum[m];

        var pts = new Vector2[N];
        int seg = 0;
        for (int i = 0; i < N; i++)
        {
            float target = total * i / N;
            while (seg < m - 1 && cum[seg + 1] < target) seg++;
            float t = (target - cum[seg]) / Mathf.Max(1e-5f, cum[seg + 1] - cum[seg]);
            pts[i] = Vector2.Lerp(contour[seg], contour[(seg + 1) % m], t);
        }

        // centre and normalise to -1..1
        Vector2 min = pts[0], max = pts[0];
        foreach (var p in pts) { min = Vector2.Min(min, p); max = Vector2.Max(max, p); }
        Vector2 centre = (min + max) * 0.5f;
        float scale = Mathf.Max(max.x - min.x, max.y - min.y) * 0.5f;
        for (int i = 0; i < N; i++) pts[i] = (pts[i] - centre) / scale;

        points = pts;
        coef = null;                                  // rebuild Fourier coefficients
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
        Debug.Log($"Imported {N} points from {sourceImage.name}");
    }
}
