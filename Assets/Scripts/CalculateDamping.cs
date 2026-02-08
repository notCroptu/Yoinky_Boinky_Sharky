using NaughtyAttributes;
using UnityEngine;

public class CalculateDamping : MonoBehaviour
{
    [SerializeField] private Rigidbody[] _rigidbody;
    [SerializeField] private MeshFilter _mesh;

    private Vector2 _linearDamp = new(1f, 5f);
    private Vector2 _angularDamp = new(0.5f, 2f);
    private Vector2 _minMaxVolume = new(0.01f, 1f);

    [Button]
    private void RecalculateDamping()
    {
        if (_rigidbody == null)
            _rigidbody = GetComponentsInChildren<Rigidbody>();

        if (_rigidbody == null)
            return;

        float scale = 1f;

        if (_mesh != null)
        {
            Mesh mesh = _mesh.mesh;
            scale = Mathf.InverseLerp(_minMaxVolume.x, _minMaxVolume.y, mesh.bounds.size.magnitude);
        }

        // Apply damping based on volume
        foreach (Rigidbody rb in _rigidbody)
        {
            if (_mesh == null)
            {
                Collider col = rb.GetComponent<Collider>();
                scale = Mathf.InverseLerp(_minMaxVolume.x, _minMaxVolume.y, col.bounds.size.magnitude);
                Debug.Log("Scale of " + rb.gameObject.name + " was: " + scale);
            }

            rb.linearDamping = Mathf.Lerp(_linearDamp.x, _linearDamp.y, scale);
            rb.angularDamping = Mathf.Lerp(_angularDamp.x, _angularDamp.y, scale);
        }
    }
}
