using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlanetGenerator : MonoBehaviour
{
    public GameObject player;

    [Header("Planet Settings")]
    public float radius = 1f;
    public float heightMultiplier = 0.1f;
    public int seed = 42;
    public Material planetMaterial;

    [Header("LOD Settings")]
    public int lodLevels = 3;
    public int maxResolution = 128;
    public float distanceTreshold = 550f;
    public float overlapDistance = 50f;

    [Header("Noise Settings")]
    public float noiseScale = 1f;
    public int octaves = 4;
    public float persistence = 0.5f;
    public float lacunarity = 2f;

    private FastNoiseLite noise;
    private List<GameObject> planetFaces;

    void Start()
    {
        noise = new FastNoiseLite(seed);
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        noise.SetFrequency(noiseScale);
        noise.SetFractalType(FastNoiseLite.FractalType.Ridged);
        noise.SetFractalOctaves(octaves);
        noise.SetFractalLacunarity(lacunarity);
        noise.SetFractalGain(persistence);

        planetFaces = new List<GameObject>();
        GeneratePlanet();
    }

	private void FixedUpdate()
	{
	    foreach (var face in planetFaces)
        {
            var meshRenderer = face.GetComponent<MeshRenderer>();
            float distance = Vector3.Distance(meshRenderer.bounds.ClosestPoint(player.transform.position), player.transform.position);
            int meshLodLevel = Int32.Parse(face.GetComponent<MeshFilter>().sharedMesh.name.Split(":")[0]);
			if (distance >= distanceTreshold * (lodLevels - meshLodLevel) && distance <= distanceTreshold * (lodLevels - meshLodLevel) + distanceTreshold + overlapDistance)
			{
				meshRenderer.enabled = true;
			}
            else if (distance >= distanceTreshold * (lodLevels - meshLodLevel) && meshLodLevel == 1)
            {
				meshRenderer.enabled = true;
            }
			else
			{
				meshRenderer.enabled = false;
			}
        }

        bool[] lookup = new bool[planetFaces.Count];
        int activeLodLevel = -1;
        foreach (var child in planetFaces)
        {
            var childMeshRenderer = child.GetComponent<MeshRenderer>();
            var childNameData = child.GetComponent<MeshFilter>().sharedMesh.name.Split(":");
            int parentId = Int32.Parse(childNameData[1]);
            var parent = planetFaces[parentId];
            var parentMeshRenderer = parent.GetComponent<MeshRenderer>();
            if (!parentMeshRenderer.enabled && childMeshRenderer.enabled)
            {
				int meshLodLevel = Int32.Parse(childNameData[0]);
                if (meshLodLevel > activeLodLevel) activeLodLevel = meshLodLevel;
                lookup[parentId] = true;
            }
            if (parentMeshRenderer.enabled && childMeshRenderer.enabled && !ReferenceEquals(parent, child))
            {
                childMeshRenderer.enabled = false;
            }
        }

        foreach (var child in planetFaces)
        {
            var childMeshFilter = child.GetComponent<MeshFilter>();
            var childNameData = child.GetComponent<MeshFilter>().sharedMesh.name.Split(":");
			int meshLodLevel = Int32.Parse(childNameData[0]);
            int parentId = Int32.Parse(childNameData[1]);
            if (lookup[parentId] && meshLodLevel == activeLodLevel)
            {
                child.GetComponent<MeshRenderer>().enabled = true;
            }
        }
	}

	public void GeneratePlanet()
    {
        // Generate cube sphere
        CreateCubeSphere();
    }

    void CreateCubeSphere()
    {
        // Define directions for the 6 cube faces
        Vector3[] directions = {
            Vector3.up,
            Vector3.down,
            Vector3.left,
            Vector3.right,
            Vector3.forward,
            Vector3.back
        };

        for (int lodLevel = 1; lodLevel <= lodLevels; lodLevel++)
        {
			foreach (var dir in directions)
			{
				for (int y = 0; y < Math.Pow(2, lodLevel); y++)
				{
					for (int x = 0; x < Math.Pow(2, lodLevel); x++)
					{
						var face = new GameObject("Face", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
                        var faceMeshRenderer = face.GetComponent<MeshRenderer>();
						face.transform.SetParent(gameObject.transform, false);

						var mesh = new Mesh();
                        mesh.name = lodLevel.ToString();
                        faceMeshRenderer.material = planetMaterial;
                        /*if (lodLevel == 1)
                        {
							face.GetComponent<MeshRenderer>().material.color = Color.red;
                        }
                        if (lodLevel == 2)
                        {
							face.GetComponent<MeshRenderer>().material.color = Color.green;
                        }
                        if (lodLevel == 3)
                        {
							face.GetComponent<MeshRenderer>().material.color = Color.blue;
                        }*/
                        //face.GetComponent<MeshRenderer>().enabled = false;

						ConstructFace(dir, (maxResolution * lodLevel) / (int)(Math.Pow(2, lodLevel)), maxResolution * lodLevel, x, y, mesh);

						mesh.RecalculateNormals();
						face.GetComponent<MeshFilter>().sharedMesh = mesh;
                        face.GetComponent<MeshCollider>().sharedMesh = mesh;

						planetFaces.Add(face);
					}
				}
			}
        }

        for (int i = 0; i < planetFaces.Count; i++)
        {
            var childMeshFilter = planetFaces[i].GetComponent<MeshFilter>();
            int childLodLevel = Int32.Parse(childMeshFilter.sharedMesh.name.Split(":")[0]);
            bool foundParent = false;
            for (int j = 0; j < planetFaces.Count; j++)
            {
				int pparentLodLevel = Int32.Parse(planetFaces[j].GetComponent<MeshFilter>().sharedMesh.name.Split(":")[0]);
                if (childLodLevel - pparentLodLevel != 1) continue;
                if (i == j) continue;
                if (planetFaces[j].GetComponent<MeshRenderer>().bounds.Contains(planetFaces[i].GetComponent<MeshRenderer>().bounds.center))
                {
                    foundParent = true;
                    Debug.Log(i.ToString() + ":" + j.ToString());
                    childMeshFilter.sharedMesh.name = childMeshFilter.sharedMesh.name + ":" + j.ToString();
                }
            }
            if (!foundParent) childMeshFilter.sharedMesh.name = childMeshFilter.sharedMesh.name + ":" + i.ToString();
        }
        int[] lookupFix = Enumerable.Repeat(1, planetFaces.Count).ToArray();
        for (int i = 0; i < planetFaces.Count; i++)
        {
            var meshFilter = planetFaces[i].GetComponent<MeshFilter>();
            var faceData = meshFilter.sharedMesh.name.Split(":");
            if (faceData.Length == 3)
            {
                Debug.Log("Malformed face data found. Face ID " + i.ToString() + " - " + meshFilter.sharedMesh.name);
                meshFilter.sharedMesh.name = faceData[0] + ":" + faceData[lookupFix[Int32.Parse(faceData[1])]];
                Debug.Log("Fixed: " + meshFilter.sharedMesh.name);
                lookupFix[Int32.Parse(faceData[1])]++;
            }
        }
    }

    void ConstructFace(Vector3 localUp, int resolution, int dirResolution, int offsetX, int offsetY, Mesh mesh)
    {
        // Initialize lists for vertices and triangles
        var vertices = new System.Collections.Generic.List<Vector3>();
        var triangles = new System.Collections.Generic.List<int>();
        var uvs = new System.Collections.Generic.List<Vector2>();

        // Get the axes for the face
        Vector3 axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        Vector3 axisB = Vector3.Cross(localUp, axisA);

        int faceResolution = resolution + 1;
        int vertStartIndex = vertices.Count;

        // Create vertices for the face
        for (int y = 0; y < faceResolution; y++)
        {
            for (int x = 0; x < faceResolution; x++)
            {
                float xPercent = (x + (offsetX * resolution)) / (float)(dirResolution);
                float yPercent = (y + (offsetY * resolution)) / (float)(dirResolution);
                uvs.Add(new Vector2(xPercent, yPercent));

                Vector3 pointOnUnitCube = localUp +
                                          (xPercent - 0.5f) * 2f * axisA +
                                          (yPercent - 0.5f) * 2f * axisB;

                Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;

                // Apply noise for heightmap
                float elevation = CalculateNoise(pointOnUnitSphere);

                vertices.Add(pointOnUnitSphere * radius * (1 + elevation * heightMultiplier));
            }
        }

        // Create triangles for the face
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = vertStartIndex + x + y * (resolution + 1);
                int iNextRow = i + resolution + 1;

                // First triangle of quad
                triangles.Add(i);
				triangles.Add(i + 1);
				triangles.Add(iNextRow);

                // Second triangle of quad
                triangles.Add(i + 1);
                triangles.Add(iNextRow + 1);
                triangles.Add(iNextRow);
            }
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
    }

    float CalculateNoise(Vector3 pointOnUnitSphere)
    {
        float noiseValue = noise.GetNoise(pointOnUnitSphere.x, pointOnUnitSphere.y, pointOnUnitSphere.z);
        float normalizedNoiseValue = (noiseValue + 1f) / 2f;

        return normalizedNoiseValue;
    }

    /*float CalculateNoise(Vector3 pointOnUnitSphere)
    {
        // Initialize noise parameters
        float noiseValue = 0;
        float frequency = noiseScale;
        float amplitude = 1;
        float maxPossibleHeight = 0;

        // Seed the noise
        System.Random prng = new System.Random(seed);
        Vector3[] octaveOffsets = new Vector3[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000);
            float offsetY = prng.Next(-100000, 100000);
            float offsetZ = prng.Next(-100000, 100000);
            octaveOffsets[i] = new Vector3(offsetX, offsetY, offsetZ);

            maxPossibleHeight += amplitude;
            amplitude *= persistence;
        }

        amplitude = 1;

        // Sum octaves
        for (int i = 0; i < octaves; i++)
        {
            Vector3 samplePoint = pointOnUnitSphere * frequency + octaveOffsets[i];

            // Use Perlin noise
            float perlinValue = Mathf.PerlinNoise(samplePoint.x, samplePoint.y);
            noiseValue += perlinValue * amplitude;

            frequency *= lacunarity;
            amplitude *= persistence;
        }

        // Normalize
        noiseValue = noiseValue / maxPossibleHeight;

        return noiseValue;
    }*/
}
