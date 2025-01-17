using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIControls : MonoBehaviour
{
	public GameObject planetObject;

	private PlanetGenerator planetGenerator;
	private float noiseScale = 1.0f;
	private int noiseType = 0;
	private string[] noiseTypes = { "OpenSimplex2", "OpenSimplex2S", "Cellular", "Perlin", "ValueCubic", "Value" };
	private int fractalType = 0;
	private string[] fractalTypes = { "None", "FBm", "Ridged", "PingPong", "DWProg", "DWIndep" };
	private float octavesCount = 1f;
	private float lacunarity = 1f;
	private float persistence = 1f;

	private void Start()
	{
		planetGenerator = planetObject.GetComponent<PlanetGenerator>();
		noiseScale = planetGenerator.noiseScale;
		noiseType = (int)planetGenerator.noiseType;
		fractalType = (int)planetGenerator.fractalType;
		octavesCount = planetGenerator.octaves;
		lacunarity = planetGenerator.lacunarity;
		persistence = planetGenerator.persistence;
	}

	private void OnGUI()
	{
		GUI.Box(new Rect(10, 10, 350, 295), "Planet Generation");
		GUI.Label(new Rect(15, 30, 100, 30), "Noise Type");
		noiseType = GUI.SelectionGrid(new Rect(15, 50, 340, 50), noiseType, noiseTypes, 3);
		GUI.Label(new Rect(15, 105, 90, 30), "Noise Scale");
		noiseScale = GUI.HorizontalSlider(new Rect(105, 110, 170, 20), noiseScale, 0.1f, 10f);
		GUI.Label(new Rect(290, 105, 90, 30), noiseScale.ToString());
		GUI.Label(new Rect(15, 130, 90, 30), "Fractal Type");
		fractalType = GUI.SelectionGrid(new Rect(15, 150, 340, 50), fractalType, fractalTypes, 3);
		GUI.Label(new Rect(15, 205, 90, 30), "Octaves");
		octavesCount = Mathf.Floor(GUI.HorizontalSlider(new Rect(105, 210, 170, 20), octavesCount, 1f, 10f));
		GUI.Label(new Rect(290, 205, 90, 30), octavesCount.ToString());
		GUI.Label(new Rect(15, 225, 90, 30), "Lacunarity");
		lacunarity = GUI.HorizontalSlider(new Rect(105, 230, 170, 20), lacunarity, 0.1f, 5f);
		GUI.Label(new Rect(290, 225, 90, 30), lacunarity.ToString());
		GUI.Label(new Rect(15, 245, 90, 30), "Persistence");
		persistence = GUI.HorizontalSlider(new Rect(105, 250, 170, 20), persistence, 0.1f, 5f);
		GUI.Label(new Rect(290, 245, 90, 30), persistence.ToString());
		if (GUI.Button(new Rect(15, 270, 340, 30), "Generate New Planet"))
		{
			planetGenerator.noiseType = (FastNoiseLite.NoiseType)noiseType;
			planetGenerator.noiseScale = noiseScale;
			planetGenerator.fractalType = (FastNoiseLite.FractalType)fractalType;
			planetGenerator.octaves = (int)octavesCount;
			planetGenerator.lacunarity = lacunarity;
			planetGenerator.persistence = persistence;
			planetGenerator.GeneratePlanet();
		}
	}
}
