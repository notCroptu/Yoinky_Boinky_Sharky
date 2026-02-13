using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

public class Hook : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private SpringJoint _joint;
    [SerializeField] private Vector2 _normalSpringHamper;
    [SerializeField] private Vector2 _HoldSpringHamper;
    [SerializeField] private float _ifFartherThan = 20f;
    private Rigidbody targetBody;
    public Vector3 Velocity => _rigidbody.linearVelocity;
    public bool Active => _active;
    private bool _active;
    private Vector3 _originalPos;
    public bool Hooked => _rigidbody.isKinematic;

    [SerializeField]
    XRInputValueReader<float> _triggerAction = new XRInputValueReader<float>("trigger");
    private void Update()
    {
        // if holdtrigger button xr then change to holdspringhamper, otherwise use normalspringhamper
        if (!_active) return;
        if (_triggerAction == null) return;

        bool isHolding = _triggerAction.ReadValue() > 0.5f;

        if (isHolding)
        {
            _joint.spring = _HoldSpringHamper.x;
            _joint.damper = _HoldSpringHamper.y;
        }
        else
        {
            _joint.spring = _normalSpringHamper.x;
            _joint.damper = _normalSpringHamper.y;
        }
    }

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
        _active = true;
        ConnectBody();
    }

    private void OnDisable()
    {
        _rigidbody.isKinematic = false;
        _active = false;
        transform.localPosition = _originalPos;
        ConnectBody();
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
        _rigidbody.AddForce(velocity, ForceMode.Force);

        if (_ifFartherThan < Vector3.Distance(targetBody.position, transform.position))
            ConnectBody();

        _joint.connectedBody = null;
    }

    public void ConnectBody()
    {
        Debug.Log("Connected hook");
        if (targetBody == null) return;
        _joint.connectedBody = targetBody;
        targetBody.WakeUp();
    }

    public void GoLine()
    {
        Debug.Log("Connected hook");
        if (targetBody == null) return;
        _joint.connectedBody = targetBody;
        targetBody.WakeUp();
    }
}
