using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
	public GameObject chunkObject;
	MeshFilter meshFilter;
	MeshCollider meshCollider;
	MeshRenderer meshRenderer;

	Vector3Int chunkPosition;

	TerrainPoint[,,] terrainMap;

	List<Vector3> vertices = new List<Vector3>();
	List<int> triangles = new List<int>();
	List<Color> colors = new List<Color>();

	int width { get { return GameData.ChunkWidth; } }
	int height { get { return GameData.ChunkHeight; } }
	float terrainSurface { get { return GameData.terrainSurface; } }

	public Chunk(Vector3Int _position)
	{
		chunkObject = new GameObject();
		chunkObject.name = string.Format("Chunk {0}, {1}", _position.x, _position.z);
		chunkPosition = _position;
		chunkObject.transform.position = chunkPosition;

		meshFilter = chunkObject.AddComponent<MeshFilter>();
		meshCollider = chunkObject.AddComponent<MeshCollider>();
		meshRenderer = chunkObject.AddComponent<MeshRenderer>();
		meshRenderer.material = Resources.Load<Material>("Materials/Terrain");
		chunkObject.transform.tag = "Terrain";
		terrainMap = new TerrainPoint[width + 1, height + 1, width + 1];
		PopulateTerrainMap();
		CreateMeshData();
	}

	void CreateMeshData()
	{

		ClearMeshData();

		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				for (int z = 0; z < width; z++)
				{
					MarchCube(new Vector3Int(x, y, z));

				}
			}
		}

		BuildMesh();

	}

	void PopulateTerrainMap()
	{
		for (int x = 0; x < width + 1; x++)
		{
			for (int z = 0; z < width + 1; z++)
			{
				for (int y = 0; y < height + 1; y++)
				{
					float thisHeight;

					thisHeight = GameData.GetTerrainHeight(x + chunkPosition.x, z + chunkPosition.z);

					terrainMap[x, y, z] = new TerrainPoint((float)y - thisHeight, Random.Range(0, 2));

				}
			}
		}
	}

	void MarchCube(Vector3Int position)
	{
		float[] cube = new float[8];
		for (int i = 0; i < 8; i++)
		{
			cube[i] = SampleTerrain(position + GameData.CornerTable[i]);
		}

		int configIndex = GetCubeConfiguration(cube);

		if (configIndex == 0 || configIndex == 255)
			return;

		int edgeIndex = 0;
		for (int i = 0; i < 5; i++)
		{
			for (int p = 0; p < 3; p++)
			{

				int indice = GameData.TriangleTable[configIndex, edgeIndex];

				if (indice == -1)
					return;

				Vector3 vert1 = position + GameData.CornerTable[GameData.EdgeIndexes[indice, 0]];
				Vector3 vert2 = position + GameData.CornerTable[GameData.EdgeIndexes[indice, 1]];

				Vector3 vertPosition;

				float vert1Sample = cube[GameData.EdgeIndexes[indice, 0]];
				float vert2Sample = cube[GameData.EdgeIndexes[indice, 1]];

				float difference = vert2Sample - vert1Sample;

				if (difference == 0)
					difference = terrainSurface;
				else
					difference = (terrainSurface - vert1Sample) / difference;

				vertPosition = vert1 + ((vert2 - vert1) * difference);
				triangles.Add(VertForIndice(vertPosition, position));
				edgeIndex++;

			}
		}
	}

	int GetCubeConfiguration(float[] cube)
	{
		int configurationIndex = 0;
		for (int i = 0; i < 8; i++)
		{
			if (cube[i] > terrainSurface)
				configurationIndex |= 1 << i;

		}

		return configurationIndex;

	}

	public void PlaceTerrain(Vector3 pos)
	{

		Vector3Int v3Int = new Vector3Int(Mathf.CeilToInt(pos.x), Mathf.CeilToInt(pos.y), Mathf.CeilToInt(pos.z));
		v3Int -= chunkPosition;
		terrainMap[v3Int.x, v3Int.y, v3Int.z].dstToSurface = 0f;
		CreateMeshData();

	}

	public void RemoveTerrain(Vector3 pos)
	{

		Vector3Int v3Int = new Vector3Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
		v3Int -= chunkPosition;
		terrainMap[v3Int.x, v3Int.y, v3Int.z].dstToSurface = 1f;
		CreateMeshData();

	}

	float SampleTerrain(Vector3Int point)
	{
		return terrainMap[point.x, point.y, point.z].dstToSurface;
	}

	int VertForIndice(Vector3 vert, Vector3Int point)
	{
		for (int i = 0; i < vertices.Count; i++)
		{
			if (vertices[i] == vert)
				return i;
		}

		vertices.Add(vert);
		colors.Add((terrainMap[point.x, point.y, point.z].textureID == 1) ? Color.black : Color.red);
		return vertices.Count - 1;
	}

	void ClearMeshData()
	{
		vertices.Clear();
		triangles.Clear();
		colors.Clear();
	}

	void BuildMesh()
	{

		Mesh mesh = new Mesh();
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.colors = colors.ToArray();
		mesh.RecalculateNormals();
		meshFilter.mesh = mesh;
		meshCollider.sharedMesh = mesh;
	}
}