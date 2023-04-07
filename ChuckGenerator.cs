using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChuckGenerator : MonoBehaviour
{
    [SerializeField] private CreatePolygon chunkPrefab; // Reference to the chunk prefab to be spawned
    [SerializeField] private int gridWidth = 100; // Width of the grid
    [SerializeField] private int gridHeight = 100; // Height of the grid
    [SerializeField] private int meshSize = 10; // Size of the mesh
    [SerializeField] private int distanceLOD = 0; // LOD distance threshold
    //[SerializeField] private TerrainType[] terrainTypes;
    [SerializeField] private MeshRenderer chunkRenderer;
    [SerializeField] private float mapScale;
    [SerializeField] private Material material; // Material to apply to the chunks
    [SerializeField] private NoiseMapGeneration noiseGenerator; // Reference to the noise generator script
    [SerializeField] private float noiseScale = 10f; // Scale of the noise

    private CreatePolygon[,] chunks; // 2D array to hold references to the chunks

    IEnumerator SpawnChunks()
    {
        ChunkMaker();

        // Yield to the next frame
        yield return null;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Initialize the chunks array
        chunks = new CreatePolygon[gridWidth, gridHeight];

        // Start the coroutine to spawn the chunks
        StartCoroutine(SpawnChunks());
    }

    // Update is called once per frame
    void Update()
    {

    }

    float DistanceFromCenter(float distance)
    {
        // Set the subdivisions of the chunk based on the distance from the center of the grid
        if (distance > 19)
        {
            distanceLOD = 1;
        }
        else if (distance > 13)
        {
            distanceLOD = 2;
        }
        else if (distance > 9)
        {
            distanceLOD = 3;
        }
        else if (distance > 5)
        {
            distanceLOD = 4;
        }
        else if (distance > 2)
        {
            distanceLOD = 5;
        }
        else
        {
            distanceLOD = 6;
        }

        return distance;
    }

    //temp Function to store the code from the Update Function
    void ChunkMaker()
    {
        meshSize = CreatePolygon.meshSize;

        material = chunkPrefab.GetComponent<MeshRenderer>().sharedMaterial;

        // Generate the noise map
        float[,] heightMap = noiseGenerator.GenerateNoiseMap(gridWidth, gridHeight, mapScale);

        // Generate a texture based on the height map
        Texture2D tileTexture = BuildTexture(heightMap);

        // Apply the texture to the material
        material.mainTexture = tileTexture;
        //material.mainTextureScale = new Vector2(1f / noiseScale, 1f / noiseScale);

        // Loop through the grid and spawn the chunks
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                // Calculate the position of the chunk based on its distance from the center of the grid
                Vector3 position = new Vector3((x - gridWidth / 2) * meshSize, 0, (y - gridHeight / 2) * meshSize);

                // Instantiate the chunk prefab at the calculated position and store a reference to it in the chunks array
                chunks[x, y] = Instantiate(chunkPrefab, position, Quaternion.identity, transform);

                // Set the material of the chunk
                chunks[x, y].GetComponent<MeshRenderer>().material = material;

                // Calculate the distance from the center of the grid to the current chunk
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(gridWidth / 2, gridHeight / 2));

                // Set the subdivisions of the chunk based on the distance from the center of the grid
                DistanceFromCenter(distance);

                // Set the subdivisions of the chunk based on the distanceLOD variable
                chunks[x, y].subdivisions = distanceLOD;
            }
        }
    }

    private Texture2D BuildTexture(float[,] heightMap)
    {
        int tileDepth = heightMap.GetLength(0);
        int tileWidth = heightMap.GetLength(1);
        Color[] colorMap = new Color[tileDepth * tileWidth];

        for (int zIndex = 0; zIndex < tileDepth; zIndex++)
        {
            for (int xIndex = 0; xIndex < tileWidth; xIndex++)
            {
                // Transform the 2D map index is an Array index
                int colorIndex = zIndex * tileWidth + xIndex;
                float height = heightMap[zIndex, xIndex];

                // Choose the terrain type based on the height value
                //TerrainType terrainType = ChooseTerrainType(height);

                // Assign the terrain type color to the coordinate
                colorMap[colorIndex] = Color.Lerp(Color.black, Color.white, height);
            }
        }

        // Create a new texture and set its pixel colors
        Texture2D tileTexture = new Texture2D(tileWidth, tileDepth);
        tileTexture.wrapMode = TextureWrapMode.Clamp;
        tileTexture.SetPixels(colorMap);
        tileTexture.Apply();

        return tileTexture;
    }
}