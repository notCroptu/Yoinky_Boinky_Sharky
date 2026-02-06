using UnityEngine;

public class Hook : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;
    public Vector3 Velocity => _rigidbody.linearVelocity;
    public bool Active => _active;
    private bool _active;

    private void Awake()
    {
        _rigidbody.isKinematic = false;
    }

    private void OnEnable()
    {
        _rigidbody.isKinematic = false;
        _active = true;
    }

    private void OnDisable()
    {
        _active = false;
    }

    public void HookHook()
    {
        _rigidbody.isKinematic = true;
    }

    public void PullHook(Vector3 velocity)
    {
        _rigidbody.isKinematic = false;
        _rigidbody.AddForce(velocity, ForceMode.Impulse);
    }
}
