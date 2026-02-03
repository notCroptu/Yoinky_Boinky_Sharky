using System;
using UnityEngine;

/// <summary>
/// Creates a canyon that goes on the z axis only, and the perlin shifts it left or right.
/// </summary>
[Serializable]
public class CanyonGenerator : Generator
{
    // should be player initial position
    [SerializeField] private Vector2 _canyonStart;
    [SerializeField] private Vector2 _canyonWidth = new(2.5f, 6f);
    [SerializeField] private float _canyonDepth = 15f;
    [SerializeField][Range(0.01f, 1f)] private float _baseHeightInfluence = 0.2f;
    [SerializeField] private float _seed = 12345f;

    [SerializeField][Range(0.01f, 1f)] private float _widthFrequency = 0.001f;
    [SerializeField][Range(0.01f, 0.45f)] private float _wallDetail = 0.001f;
    [SerializeField][Range(0.01f, 0.1f)] private float _wanderFrequency = 0.01f;
    [SerializeField] private float _wanderAmplitude = 40f;
    [SerializeField] private AnimationCurve _parabolaShape;

    public override Vector3[] Generate(Vector3[] vertices, Vector3[] normals, Vector3 position)
    {
        Vector3[] newVertices = new Vector3[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            // if (normals[i].y! > 0.99f || (normals[i].y! < -0.99f)) continue; // check if it's top face or bottom face

            // Convert local vertex to world space
            Vector3 worldPos = vertices[i] + position;

            // Find canyon widths by smoothing a variation with Perlin
            float widthNoise = Noise(worldPos.z * _widthFrequency);
            float canyonWidth = Mathf.Lerp(_canyonWidth.x, _canyonWidth.y, widthNoise);
            
            float detailNoise = Noise(worldPos.z * _wallDetail);
            canyonWidth += Mathf.Lerp(_canyonWidth.x, _canyonWidth.y, detailNoise);

            // The most important part, is detecting the center based on z with perlin.
            Vector3 canyonCenter = GetCanyonCenter(worldPos.z);
            float dist = Mathf.Abs(worldPos.x - canyonCenter.x);

            // Normalize distance to get a parabola shape
            float normalized = dist / canyonWidth;
            float carveShape = _parabolaShape != null 
                ? _parabolaShape.Evaluate(normalized) 
                : Mathf.Clamp01(1f - normalized * normalized);

            // Carve further than base height, and leave some of the base heights only
            worldPos.y *= Mathf.Lerp(1f, _baseHeightInfluence, carveShape);
            float carvedY = worldPos.y - carveShape * _canyonDepth - position.y;

            // Store back in local space
            newVertices[i] = new Vector3(vertices[i].x, carvedY , vertices[i].z);
        }

        return newVertices;
    }



    private Vector3 GetCanyonCenter(float z)
    {
        // how far forward from the start (should be player initial position)
        float dist = z - _canyonStart.y;

        // wandering with perlin, 0.5 makes the perlin shift from left to right, rather than only right
        float offsetX = Noise(dist * _wanderFrequency) - 0.5f;
        offsetX *= _wanderAmplitude;

        return new Vector3(_canyonStart.x + offsetX, 0f, z);
    }

    private float Noise(float value)
    {
        //if (_fbm)
        //    return FBM(value, _seed);

        return Mathf.PerlinNoise(value + _seed, value * 0.37f + _seed * 1.13f);
    }

    /*[Header("simple FBM")]
    [SerializeField] private bool _fbm;
    [SerializeField][Range(0.01f, 20)] private float _lacunarity = 2.0f;
    [SerializeField][Range(0.01f, 1f)] private float _gain = 0.5f;
    [SerializeField][Range(0.01f, 1f)] private float _frequency = 0.5f;
    [SerializeField][Range(0.01f, 1f)] private float _amplitude = 0.5f;
    [SerializeField][Range(1, 5)] private int _octaves = 2;
    public float FBM(float value, float seed)
    {
        float y = 0f;
        float amplitude = _amplitude;
        float frequency = _frequency;

        for (int i = 0; i < _octaves; i++)
        {
            y += amplitude * Mathf.PerlinNoise(frequency * value, seed);
            frequency *= _lacunarity;
            amplitude *= _gain;
        }

        return y;
    }*/
}