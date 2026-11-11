using System.Collections.Generic;
using UnityEngine;

public enum ShapeMode
{
	None,
	OnStations,
	Everywhere
}

[System.Serializable]
public class ShapeStation
{
	public string name;
	public float frequency = 100.0f;
	public float width = .25f;
	public ShapeAsset shape;
}

[CreateAssetMenu(menuName = "Radio/Level Config")]
public class LevelRadioConfig : ScriptableObject
{
	public float minFreq = 88f, maxFreq = 108f;

	[Header("Default sine: cycles visible on screen")]
	public float minCycles = 2f, maxCycles = 14f;

	public ShapeMode _shapeMode = ShapeMode.OnStations;

	[Header("OnStations")]
	public List<ShapeStation> stations;

	[Header("Everywhere")]
	public ShapeAsset globalShape;
}
