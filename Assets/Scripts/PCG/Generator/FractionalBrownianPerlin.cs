using System;
using UnityEngine;

[Serializable]
public class FractionalBrownianPerlin : Generator
{
    [SerializeField] private Vector2 _offset;
    [SerializeField][Range(0, 40f)] private float _height = 5f;
    [SerializeField][Range(1, 5)] private int _octaves = 2;
    [SerializeField][Range(0.01f, 10f)] private float _lacunarity = 2.0f;
    [SerializeField][Range(0.01f, 1f)] private float _gain = 0.5f;
    [SerializeField][Range(0.01f, 1f)] private float _frequency = 0.5f;
    [SerializeField][Range(0.01f, 1f)] private float _amplitude = 0.5f;

    public override Vector3[] Generate(Vector3[] vertices, Vector3[] normals, Vector3 position)
    {
        Vector3[] newVertices = vertices;

        // map 2D perlin noise to 3D
        for (int i = 0; i < vertices.Length; i++)
        {
            // if (normals[i].y !> 0.99f) continue;

            float value = FBM(vertices[i] + position);
            value *= _height;
            value += vertices[i].y;
            Vector3 replace = new Vector3(vertices[i].x, value, vertices[i].z);
            newVertices[i] = replace;
        }

        return newVertices;
    }

    public float FBM(Vector3 vector)
    {
        float amplitude = _amplitude;

        float value = 0f;
        float frequency = _frequency;

        Vector2 vectorXZ = new Vector2(vector.x + _offset.x, vector.z + _offset.y);

        // Loop octaves and find correct height
        for (int i = 0; i < _octaves; i++)
        {
            value += amplitude * Mathf.PerlinNoise(vectorXZ.x * frequency, vectorXZ.y * frequency);
            frequency *= _lacunarity;
            amplitude *= _gain;
        }

        return value;
    }
}