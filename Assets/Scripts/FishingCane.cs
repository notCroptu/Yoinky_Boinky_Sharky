using System.Collections;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.XR.Content.Interaction;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class FishingCane : MonoBehaviour
{
    [SerializeField] private Transform _throwingPos;
    [SerializeField] private Hook _hook;
    [SerializeField] private XRKnob _wheel;
    [SerializeField][Range(0f, 1f)] private float _neededValueToPull = 0.6f;
    [SerializeField][Min(0f)] private float _triggeringDistance = 0.6f;

    private void Awake()
    {
        Spline spline = _spline.Spline;
        spline.Clear();

        spline.Add(new BezierKnot(_startPoints[_startPoints.Length-1].position));
        spline.Add(new BezierKnot(_hookPoint.position));

        spline.SetTangentMode(0, TangentMode.AutoSmooth);
        spline.SetTangentMode(1, TangentMode.AutoSmooth);
    }

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
            Vector3 dis = _lastPos - _throwingPos.position;

            if ( _hook.Velocity.magnitude < dis.magnitude && dis.magnitude >= _triggeringDistance)
            {
                _hook.PullHook(dis *10f);
                Debug.Log("Pushed hook. ");
            }

            _lastPos = _throwingPos.position;

            if (_hook.Hooked)
                break;
                
            yield return null;
        }

        while (true)
        {
            if (_hook.Hooked)
            {
                if (_wheel.value > _neededValueToPull)
                {
                    Vector3 dis = _throwingPos.position - _lastPos;

                    if (dis.magnitude >= _triggeringDistance)
                    {
                        _hook.PullHook(dis);
                        Debug.Log("Pulled with enough force. ");
                        break;
                    }
                }
            }

            _lastPos = _throwingPos.position;
            _wheel.value -= Time.deltaTime;
            _wheel.value = Mathf.Max(_wheel.value, 0f);

            yield return null;
        }
    }

    [SerializeField] private Transform _hookPoint;
    [SerializeField] private Transform[] _startPoints;
    [SerializeField] private LineRenderer _fishingLine;
    [SerializeField] private SplineContainer _spline;
    [SerializeField][Min(2)] private int _lineResolution;
    
    private void LateUpdate()
    {
        UpdateSpline();
    }

    private void UpdateSpline()
    {
        Spline spline = _spline.Spline;

        _fishingLine.positionCount = _lineResolution;
        
        for (int i = 0; i < _startPoints.Length - 1; i++)
        {
            _fishingLine.SetPosition(i, _startPoints[i].position);
        }

        spline.SetKnot(0, new BezierKnot(_startPoints[_startPoints.Length-1].position));
        spline.SetKnot(1, new BezierKnot(_hookPoint.position));

        for (int i = _startPoints.Length - 1; i < _fishingLine.positionCount; i++)
        {
            float t = (float)(i - (_startPoints.Length - 1)) / (_fishingLine.positionCount - _startPoints.Length);
            _fishingLine.SetPosition(i, _spline.EvaluatePosition(t));
        }
    }

    public void Drop()
    {
        if (_cor != null)
            StopCoroutine(_cor);
        _hook.enabled = false;
    }
}
