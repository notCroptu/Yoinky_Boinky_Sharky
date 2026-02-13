using System.Collections;
using NaughtyAttributes;
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

    [Button]
    public void PickUp()
    {
        _hook.enabled = true;
        if (_cor != null)
            StopCoroutine(_cor);
        _cor = StartCoroutine(ThrowHook());
    }

    private IEnumerator ThrowHook()
    {
        XRBaseInputInteractor controllerInteractor = _cane.firstInteractorSelecting as XRBaseInputInteractor;

        Vector3 lastPos = _throwingPos.position;

        while (true)
        {
            Vector3 worldDis = _throwingPos.position - lastPos;
            float pullAmount = Vector3.Dot(worldDis, _fishingCaneForward.forward);

            if (pullAmount < 0f &&  worldDis.magnitude > _velocityThreshold)
            {
                Debug.Log("Pushed hook when magnitude was: " + _hook.Velocity.magnitude );
                _hook.ThrowHook(_fishingCaneForward.forward * _throwingVelocity);
            }
            else if (pullAmount > 0.1f)
            {
                _hook.GoLine();
            }

            if (_hook.Hooked)
            {
                foreach (IXRSelectInteractor interactor in _cane.interactorsSelecting)
                {
                    XRBaseInputInteractor ctrl = interactor as XRBaseInputInteractor;
                    if (ctrl != null)
                        ctrl.SendHapticImpulse(1f, 0.4f);
                }

                break;
            }

            lastPos = _throwingPos.position;

            yield return null;
        }

        lastPos = _throwingPos.position;

        _wheel.value = 0.5f;

        while (true)
        {
            foreach (IXRSelectInteractor interactor in _wheel.interactorsSelecting)
            {
                XRBaseInputInteractor ctrl = interactor as XRBaseInputInteractor;
                if (ctrl != null)
                    ctrl.SendHapticImpulse(_wheel.value, 0.2f);
            }

            if (_wheel.value > _neededValueToPull)
            {
                Vector3 worldDis = _throwingPos.position - lastPos;
                Vector3 direction = _throwingPos.position - _hook.transform.position;

                if (controllerInteractor != null)
                    controllerInteractor.SendHapticImpulse(Mathf.Clamp(worldDis.magnitude, 0.5f, 1f), 0.2f);

                float pullAmount = Vector3.Dot(worldDis, direction);

                Debug.Log("Pulling with NOT enough force of: " + worldDis.magnitude + " pullAmount: " + pullAmount);

                if (pullAmount > 0f && worldDis.magnitude >= _triggeringDistance/2)
                {
                    Sound.PlaySound(_pullSource, _pullSounds);

                    if (worldDis.magnitude >= _triggeringDistance)
                    {
                        Debug.Log("Pulling with enough force of: " + worldDis.magnitude + " pullAmount: " + pullAmount);
                        Sound.PlaySound(_wooshSource, _wooshSounds);

                        _hook.PullHook(worldDis);
                        Debug.Log("Pulled with enough force. ");

                        if (controllerInteractor != null)
                            controllerInteractor.SendHapticImpulse(1f, 0.4f);

                        break;
                    }
                }
            }

            lastPos = _throwingPos.position;
            _wheel.value -= Time.deltaTime * _wheelVelocity;

            if (_wheel.value < 0f)
            {
                ReleaseHook();
                break;
            }

            _wheel.value = Mathf.Max(_wheel.value, 0f);

            yield return null;
        }
    }

    private void ReleaseHook()
    {
        if (_cane == null) return;

        if (_cane.isSelected)
        {
            IXRSelectInteractor interactor = _cane.firstInteractorSelecting;
            if (interactor != null)
            {
                _cane.interactionManager.SelectExit(interactor, _cane);
                Debug.Log("Cane forcibly released!");
            }
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
