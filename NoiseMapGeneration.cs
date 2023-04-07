using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseMapGeneration : MonoBehaviour
{
    public float[,] GenerateNoiseMap(int mapDepth, int mapWidth, float scale)
    {
        // create an empty noise map with the mapDepth and mapWidth coordinates
        // create an empty 2D array to store the noise map
        float[,] noiseMap = new float[mapDepth, mapWidth];

        // ensure that the scale is not zero or negative
        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        // loop through all coordinates in the noise map
        for (int zIndex = 0; zIndex < mapDepth; zIndex++)
        {
            for (int xIndex = 0; xIndex < mapWidth; xIndex++)
            {
                // calculate sample indices based on the coordinates and the scale
                // calculate the x and y coordinates of the sample point based on the noise scale
                float sampleX = xIndex / scale;
                float sampleZ = zIndex / scale;
                // generate noise value using PerlinNoise
                // generate a Perlin noise value at the sample point using Mathf.PerlinNoise
                float noise = Mathf.PerlinNoise(sampleX, sampleZ);
                // store the noise value in the noise map at the current coordinate
                noiseMap[zIndex, xIndex] = noise;
            }
        }
        return noiseMap;
    }
}
