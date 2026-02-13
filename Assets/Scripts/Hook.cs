using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class Hook : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private SpringJoint _joint;
    [SerializeField] private Vector2 _normalSpringHamper;
    [SerializeField] private Vector2 _HoldSpringHamper;
    [SerializeField] private XRGrabInteractable _cane;
    [SerializeField] private float _ifFartherThan = 20f;
    private Rigidbody targetBody;
    public Vector3 Velocity => _rigidbody.linearVelocity;
    public bool Active => _active;
    private bool _active;
    private Vector3 _originalPos;
    public bool Hooked => _rigidbody.isKinematic;

    private void OnEnable()
    {
        _active = true;
        ConnectBody();

        _cane.activated.AddListener(OnActivate);
        _cane.deactivated.AddListener(OnDeactivate);
    }

    private void OnDisable()
    {
        _rigidbody.isKinematic = false;
        _active = false;
        transform.localPosition = _originalPos;
        ConnectBody();

        _cane.activated.RemoveListener(OnActivate);
        _cane.deactivated.RemoveListener(OnDeactivate);
    }
    
    private void OnActivate(ActivateEventArgs args)
    {
        Debug.Log("HOOKING!!!");
        _joint.spring = _HoldSpringHamper.x;
        _joint.damper = _HoldSpringHamper.y;
    }

    private void OnDeactivate(DeactivateEventArgs args)
    {
        _joint.spring = _normalSpringHamper.x;
        _joint.damper = _normalSpringHamper.y;
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
