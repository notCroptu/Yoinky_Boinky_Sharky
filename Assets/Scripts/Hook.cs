using UnityEngine;

public class Hook : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private SpringJoint _joint;
    [SerializeField] private float _ifFartherThan = 20f;
    private Rigidbody targetBody;
    public Vector3 Velocity => _rigidbody.linearVelocity;
    public bool Active => _active;
    private bool _active;
    private Vector3 _originalPos;
    public bool Hooked => _rigidbody.isKinematic;

    private void Awake()
    {
        _originalPos = transform.localPosition;
        _rigidbody.isKinematic = false;
    }

    private void Start()
    {
        targetBody = _joint.connectedBody;
    }

    private void OnEnable()
    {
        _rigidbody.isKinematic = false;
        _active = true;
        StopThrow();
    }

    private void OnDisable()
    {
        _active = false;
        _rigidbody.isKinematic = true;
        transform.localPosition = _originalPos;
        StopThrow();
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

    public void ThrowHook(Vector3 velocity)
    {
        _rigidbody.isKinematic = false;
        _rigidbody.AddForce(velocity, ForceMode.Force);

        if (_ifFartherThan < Vector3.Distance(targetBody.position, transform.position))
            StopThrow();
        
        _joint.connectedBody = null;
    }

    public void StopThrow()
    {
        if (targetBody == null) return;
        _joint.connectedBody = targetBody;
       targetBody.WakeUp();
    }
}
