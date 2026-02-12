using System.Collections;
using UnityEngine;
using UnityEngine.XR.Content.Interaction;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class FishingCane : MonoBehaviour
{
    [SerializeField] private Transform _fishingCaneForward;
    [SerializeField] private Transform _throwingPos;
    [SerializeField] private Hook _hook;
    [SerializeField] private float _angleThreshold = 40f;
    [SerializeField] private float _velocityThreshold = 3f;
    [SerializeField] private float _throwingVelocity = 3f;
    [SerializeField] private XRKnob _wheel;
    [SerializeField] private XRGrabInteractable _cane;
    [SerializeField][Range(0f, 1f)] private float _neededValueToPull = 0.6f;
    [SerializeField][Min(0f)] private float _triggeringDistance = 0.6f;
    [SerializeField] private AudioClip[] _pullSounds;
    [SerializeField] private AudioSource _pullSource;
    [SerializeField] private AudioClip[] _wooshSounds;
    [SerializeField] private AudioSource _wooshSource;
    [SerializeField] private AudioSource _reelSource;

    private void Start()
    {
        _hook.enabled = false;
    }

    private Coroutine _cor;
    public void PickUp()
    {
        _hook.enabled = true;
        if (_cor != null)
            StopCoroutine(_cor);
        _cor = StartCoroutine(ThrowHook());
    }

    private Vector3 _lastPos;
    private IEnumerator ThrowHook()
    {
        _lastPos = _throwingPos.position;

        while (true)
        {
            float angle = Vector3.Angle(_hook.Velocity, _fishingCaneForward.forward);
            if ( angle < _angleThreshold && _hook.Velocity.magnitude > _velocityThreshold)
            {
                Debug.Log("Pushed hook when magnitude was: " + _hook.Velocity.magnitude + " and angle was: " + angle);
                _hook.ThrowHook(_fishingCaneForward.forward *_throwingVelocity);
            }
            
            if (_hook.Hooked)
            {
                foreach (IXRSelectInteractor interactor in _cane.interactorsSelecting)
                {
                    XRBaseInputInteractor controllerInteractor = interactor as XRBaseInputInteractor;
                    if (controllerInteractor != null)
                        controllerInteractor.SendHapticImpulse(1f, 0.4f);
                }

                break;
            }
                
            yield return null;
        }

        _hook.StopThrow();

        _lastPos = _throwingPos.position;

        while (true)
        {
            foreach (IXRSelectInteractor interactor in _wheel.interactorsSelecting)
            {
                XRBaseInputInteractor controllerInteractor = interactor as XRBaseInputInteractor;
                if (controllerInteractor != null)
                    controllerInteractor.SendHapticImpulse(_wheel.value, 0.2f);
            }

            if (_wheel.value > _neededValueToPull)
            {
                Vector3 dis = _throwingPos.position - _lastPos;
                Sound.PlaySound(_pullSource, _pullSounds);

                if (dis.magnitude >= _triggeringDistance)
                {
                    Sound.PlaySound(_wooshSource, _wooshSounds);

                    _hook.PullHook(dis);
                    Debug.Log("Pulled with enough force. ");

                    XRBaseInputInteractor controllerInteractor = _cane.firstInteractorSelecting as XRBaseInputInteractor;
                    if (controllerInteractor != null)
                        controllerInteractor.SendHapticImpulse(1f, 0.4f);

                    break;
                }
            }

            _lastPos = _throwingPos.position;
            _wheel.value -= Time.deltaTime * _wheelVelocity;
            _wheel.value = Mathf.Max(_wheel.value, 0f);

            yield return null;
        }
    }

    [SerializeField] private float _wheelVelocity = 1f;
    private float lastWheelValue = 0f;
    private void Update()
    {
        float wheelVelocity = Mathf.Abs(_wheel.value - lastWheelValue);
        _reelSource.volume = Mathf.Clamp01(wheelVelocity * 10f);

        lastWheelValue = _wheel.value;
    }

    [SerializeField] private Transform _startPoint;
    [SerializeField] private LineRenderer _fishingLine;
    [SerializeField][Min(2)] private int _lineResolution;
    
    private void LateUpdate()
    {
        UpdateSpline();
    }

    private Vector3 _lastStartPos;
    private Vector3 _lastHookPos;
    private void UpdateSpline()
    {
        _fishingLine.positionCount = 2;

        if (_lastStartPos != _startPoint.position)
        {
            _fishingLine.SetPosition(0, _startPoint.position);
            _lastStartPos = _startPoint.position;
        }
        if (_lastHookPos != _hook.transform.position)
        {
            _fishingLine.SetPosition(1, _hook.transform.position);
            _lastHookPos = _hook.transform.position;
        }
    }

    public void Drop()
    {
        if (_cor != null)
            StopCoroutine(_cor);
        _hook.enabled = false;
    }
}
