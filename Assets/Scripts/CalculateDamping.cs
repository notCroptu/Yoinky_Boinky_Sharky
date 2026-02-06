using UnityEngine;

public class CalculateDamping : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private MeshFilter _mesh;

    private Vector2 _linearDamp = new(1f, 5f);
    private Vector2 _angularDamp = new(0.5f, 2f);
    private Vector2 _minMaxVolume = new(0.1f, 2f);

    private void Start()
    {
        if (_rigidbody == null)
            _rigidbody = GetComponent<Rigidbody>();
        if (_mesh == null)
            _mesh = GetComponent<MeshFilter>();

        if (_mesh == null || _rigidbody == null)
            return;

        Mesh mesh = _mesh.mesh;

        float volume = mesh.bounds.size.x * mesh.bounds.size.y * mesh.bounds.size.z;
        float scale = Mathf.InverseLerp(_minMaxVolume.x, _minMaxVolume.y, volume);

        // Apply damping based on volume
        _rigidbody.linearDamping = Mathf.Lerp(_linearDamp.x, _linearDamp.y, scale);
        _rigidbody.angularDamping = Mathf.Lerp(_angularDamp.x, _angularDamp.y, scale);
    }
}
