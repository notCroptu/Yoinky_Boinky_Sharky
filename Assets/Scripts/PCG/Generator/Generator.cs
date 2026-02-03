using System;
using UnityEngine;

[Serializable]
public abstract class Generator
{
    public abstract Vector3[] Generate(Vector3[] vertices, Vector3[] normals, Vector3 position);
}