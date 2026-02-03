using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [Header("Terrain Generation")]
    [SerializeField] private Transform _fog;
    [SerializeField] private float _border;
    private float _correctedBorder;
    [SerializeField] private bool _canyon = true;

    [Header("Chunk Generation")]
    [SerializeField] private Mesh _chunkMesh;
    [SerializeField] private GameObject _chunkPrefab;
    private Transform _chunkHolder;

    [Header("Chunk Tesselation")]
    [SerializeField][Range(0, 4)] private int _subdivisions;
    [HideInInspector][SerializeReference] private Tesselator tesselator;

    [SerializeField] private FractionalBrownianPerlin _fbm;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying) return;

        Setup();
    }
#endif

    private void Start()
    {
        Setup();
    }

    private void Setup()
    {
        if (_chunkMesh == null) return;

        _activeChunkCoords = new();
        _inactiveChunks = new();

        tesselator = new Tesselator(_chunkMesh, _subdivisions);
        // Debug.Log("Subdividing, wanted: " + _subdivisions + " current: " + tesselator.CurrentSubdivisions);
        tesselator.Generate();

        _verts = tesselator.SubdividedMesh.vertices;
        _norms = tesselator.SubdividedMesh.normals;

        _chunkHolder = _chunkPrefab.transform.parent;

        if (Application.isPlaying)
        {
            _chunkPrefab.transform.SetParent(_chunkHolder.transform.parent);
            _chunkPrefab.SetActive(false);
        }

        _chunkSize = _chunkMesh.bounds.size.x;

        // make the border have a slight increase to account for middle points of 10x10 chunks that fall just short of the radius
        _correctedBorder = _border + Mathf.Sqrt(_chunkSize * _chunkSize + _chunkSize * _chunkSize);

        StartCoroutine(DelayedSetup());
    }

    private bool _setup = false;

    private IEnumerator DelayedSetup()
    {
        yield return null;

        // Debug.Log("Begin Delayed Setup. inactive and active counts: " + _inactiveChunks.Count + " " + _activeChunkCoords.Count);

        if (Application.isPlaying)
            _chunkPrefab.GetComponent<MeshFilter>().mesh = Instantiate(tesselator.SubdividedMesh);
        else
            _chunkPrefab.GetComponent<MeshFilter>().sharedMesh = Instantiate(tesselator.SubdividedMesh);

        for (int i = _chunkHolder.childCount - 1; i > 0; i--)
        {
            if (Application.isPlaying)
                Destroy(_chunkHolder.GetChild(i).gameObject);
            else
                DestroyImmediate(_chunkHolder.GetChild(i).gameObject);
        }

        _setup = true;

        if (!Application.isPlaying)
        {
            // UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
            Update();
        }
    }

    // can be chunk coord because it only needs to check on the map if its yet loaded or not, for other operations I rely on needed and child lists
    private HashSet<Vector2Int> _activeChunkCoords;
    // needs to be transforms because i need access to game object
    private HashSet<Transform> _inactiveChunks;
    private float _chunkSize;
    private void Update()
    {
        if (!_setup) return;

        Vector2Int centerChunk = WorldToChunk(_fog.position);
        int radius = Mathf.CeilToInt((_correctedBorder + _fog.lossyScale.x / 2f) / _chunkSize);
        int sqrRadius = radius * radius;
        // Debug.Log("radius(x10): " + radius + " a: " + (_correctedBorder + _fog.localScale.x) + " center: " + centerChunk + " a: " + _fog.position);

        // set needed chunks for this frame
        HashSet<Vector2Int> needed = new();

        for (int x = -radius; x <= radius; x++)
        {
            for (int z = -radius; z <= radius; z++)
            {
                if (sqrRadius < (x * x + z * z))
                    continue;

                Vector2Int coord = new Vector2Int(centerChunk.x + x, centerChunk.y + z);
                needed.Add(coord);

                if (!_activeChunkCoords.Contains(coord)) // if needed coord isn't activated yet
                {
                    Vector3 pos = ChunkToWorld(coord);
                    GenerateChunk(pos); // take from inactive or create new, and generate visuals
                    _activeChunkCoords.Add(coord); // add coord to active chunks no mather the previous procedure
                }
            }
        }

        // recycle chunks that are no longer needed
        for (int i = _chunkHolder.childCount - 1; i >= 0; i--)
        {
            Transform chunk = _chunkHolder.GetChild(i);
            Vector2Int coord = WorldToChunk(chunk.position);

            if (!needed.Contains(coord) && chunk.gameObject.activeSelf) // if the active chunk is not needed
            {
                chunk.gameObject.SetActive(false); // disable it
                _inactiveChunks.Add(chunk); // add it to inactive
                _activeChunkCoords.Remove(coord); // remove it from active
            }
        }
    }

    private Vector2Int WorldToChunk(Vector3 pos)
    {
        return new Vector2Int(
            Mathf.RoundToInt(pos.x / _chunkSize),
            Mathf.RoundToInt(pos.z / _chunkSize)
        );
    }

    private Vector3 ChunkToWorld(Vector2Int pos)
    {
        return new Vector3(pos.x * _chunkSize,
            0f,
            pos.y * _chunkSize
        );
    }

    private Vector3[] _verts;
    private Vector3[] _norms;
    private void GenerateChunk(Vector3 position)
    {
        GameObject newChunk;

        if (_inactiveChunks.Any())
        {
            newChunk = _inactiveChunks.First().gameObject;
            _inactiveChunks.Remove(newChunk.transform);
            newChunk.SetActive(true);
        }
        else
        {
            newChunk = Instantiate(_chunkPrefab, _chunkHolder);
            newChunk.SetActive(true);
        }

        newChunk.transform.position = position;

        // Always create a copy of the base tesseled mesh instead of using the prefab directly
        Mesh newMesh = new Mesh
        {
            vertices = (Vector3[])_verts.Clone(),
            normals = (Vector3[])_norms.Clone(),
            triangles = tesselator.SubdividedMesh.triangles,
            uv = tesselator.SubdividedMesh.uv
        };

        newMesh.vertices = _fbm.Generate(newMesh.vertices, newMesh.normals, position);

        newMesh.RecalculateNormals();
        newMesh.RecalculateBounds();
        newMesh.RecalculateTangents();

        // Assign to filter and collider

        MeshFilter mf = newChunk.GetComponent<MeshFilter>();
        MeshCollider mc = newChunk.GetComponent<MeshCollider>();

        if (Application.isPlaying)
        {
            mf.mesh = newMesh;
            mc.sharedMesh = newMesh;
        }
        else
        {
            mf.sharedMesh = newMesh;
            mc.sharedMesh = newMesh;
        }
    }
}
