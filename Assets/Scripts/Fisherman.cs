using System.Collections;
using DG.Tweening;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;

public class Fisherman : MonoBehaviour
{
    [SerializeField] private GameObject _mainObject;
    [SerializeField] private Transform _origin;
    [SerializeField] private Collider _collider;
    [SerializeField][Min(0f)] private float _radius = 50f;
    [SerializeField][Min(0f)] private float _addedHeight = 5f;
    [SerializeField][Min(0f)] private float _time = 3f;
    [SerializeField][Min(0f)] private float _movingDuration = 1f;

    [SerializeField] private Rigidbody _hookTransform;
    [SerializeField] private Transform _hookPoint;
    [SerializeField] private Transform _startPoint;
    [SerializeField] private Transform _fishingPoint;
    [SerializeField] private LineRenderer _fishingLine;
    [SerializeField] private SplineContainer _spline;
    [SerializeField][Min(3)]  private int _lineResolution = 30;
    [SerializeField][Min(0f)] private float _hookThrowForce = 2f;
    
    [SerializeField] private GameObject[] FishermanPrefabs;
    [SerializeField] private GameObject FishingCanePrefab;
    [SerializeField][Min(1)] private int _initializeFisherAtATime = 4;
    [SerializeField] private Transform _fisherPool;
    [SerializeField] private Transform _fishingCanePool;
    [SerializeField][Min(1f)] private float _pullingForce;
    [SerializeField][Min(0f)] private float _spawnRadius = 5f;

    [SerializeField][Min(0.1f)] private float _waitTime = 2.3f;
    [SerializeField][Range(0f, 1f)] private float _possibility = 0.2f;

    private void Awake()
    {
        Spline spline = _spline.Spline;
        spline.Clear();

        spline.Add(new BezierKnot(_startPoint.position));
        spline.Add(new BezierKnot(_fishingPoint.position));
        spline.Add(new BezierKnot(_hookTransform.position));

        spline.SetTangentMode(0, TangentMode.AutoSmooth);
        spline.SetTangentMode(1, TangentMode.AutoSmooth);
        spline.SetTangentMode(2, TangentMode.AutoSmooth);

        Application.targetFrameRate = 90;
    }

    private void Start()
    {
        _hookTransform.isKinematic = true;

        _mainObject.transform.position = _origin.transform.position;
        _fisherPool.gameObject.SetActive(false);
        _fishingCanePool.gameObject.SetActive(false);

        for (int i = 0; i < _initializeFisherAtATime; i++)
        {
            InitNewFisher();
        }

        ChooseNewLocation();
    }

    private void InitNewFisher()
    {
        GameObject go = Instantiate(FishermanPrefabs[Random.Range(0, FishermanPrefabs.Length)], _fisherPool);
        go.transform.position = Vector3.zero;
        go = Instantiate(FishingCanePrefab, _fishingCanePool);
        go.transform.position = Vector3.zero;
    }

    private void OnTriggerEnter(Collider other)
    {
        Hook hook = other.GetComponent<Hook>();
        if (hook != null && hook.Active)
        {
            hook.HookHook();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Hook hook = other.GetComponent<Hook>();
        if (hook != null && hook.Active)
        {
            ReverseFished(hook.Velocity);
        }
    }

    [Button]
    public void ReverseFish() => ReverseFished(Random.insideUnitSphere);
    public void ReverseFished(Vector3 direction)
    {
        Vector3 newFishingPos = new Vector3(direction.x, 0f, direction.z).normalized;

        Transform fisher = _fisherPool.GetChild(0);
        fisher.transform.localPosition = newFishingPos * _spawnRadius;
        fisher.transform.Rotate(new Vector3(Random.Range(-90f, 90f), Random.Range(-90f, 90f), Random.Range(-90f, 90f)));
        Rigidbody rb = fisher.GetComponentInChildren<Rigidbody>();
        fisher.SetParent(null);

        if (rb != null)
        {
            rb.AddForce(direction * _pullingForce, ForceMode.Impulse);
            rb.AddTorque(direction * _pullingForce*4f, ForceMode.Impulse);
        }

        Transform fishingCane = _fishingCanePool.GetChild(0);
        fishingCane.transform.localPosition = newFishingPos * _spawnRadius;
        fishingCane.transform.Rotate(new Vector3(Random.Range(-90f, 90f), Random.Range(-90f, 90f), Random.Range(-90f, 90f)));
        Rigidbody rbC = fishingCane.GetComponentInChildren<Rigidbody>();
        fishingCane.SetParent(null);

        if (rbC != null)
        {
            rbC.AddForce(direction * _pullingForce, ForceMode.Impulse);
            rbC.AddTorque(direction * _pullingForce*2f, ForceMode.Impulse);
        }

        
        Vector3 newPos = _mainObject.transform.position + new Vector3(0f, _addedHeight, 0f);

        _mainObject.transform.DOMove(newPos, _time);
        _hookTransform.isKinematic = true;
        _hookTransform.transform.DOMove(newPos, _time);

    

        InitNewFisher(); // add new to pool to fetch later
        _collider.enabled = false;

        StartCoroutine(TryChooseNewLocation());
    }

    private IEnumerator TryChooseNewLocation()
    {
        YieldInstruction wfs = new WaitForSeconds(_waitTime);

        do
        {
            yield return wfs;
        } while (Random.Range(0f, 1f) > _possibility);

        ChooseNewLocation();
    }

    private void LateUpdate()
    {
        UpdateSpline();
    }

    private void UpdateSpline()
    {
        Spline spline = _spline.Spline;

        _fishingPoint.position = (_startPoint.position + _hookPoint.position) / 2f;

        spline.SetKnot(0, new BezierKnot(_startPoint.position));
        spline.SetKnot(1, new BezierKnot(_fishingPoint.position));
        spline.SetKnot(2, new BezierKnot(_hookPoint.position));

        _fishingLine.positionCount = _lineResolution;

        for (int i = 0; i < _lineResolution; i++)
        {
            float t = i / (_lineResolution - 1f);
            _fishingLine.SetPosition(i, _spline.EvaluatePosition(t));
        }
    }

    private void ChooseNewLocation()
    {
        Vector3 newPos = _origin.position
            + new Vector3(
                (Random.value * 2 - 1f) * _radius * 2,
                _addedHeight,
                (Random.value * 2 - 1f) * _radius * 2
            );

        _mainObject.transform.position = newPos;

        Debug.Log("Chose new location. Of: " + newPos);

        newPos.y = _origin.position.y;

        _mainObject.transform.DOMove(newPos, _time);
        _collider.enabled = true;

        _startPoint.localPosition = new Vector3(0f, _startPoint.localPosition.y, 0f) + new Vector3(Random.value * 2 - 1f, 0f, Random.value * 2 - 1f).normalized * _spawnRadius;
        _hookTransform.transform.localPosition = _startPoint.localPosition;

        _hookTransform.isKinematic = false;
        Vector3 newVelocity = new Vector3(
                (Random.value * 2 - 1f) * _radius,
                -_radius * _hookThrowForce,
                (Random.value * 2 - 1f) * _radius
            );
        
        _hookTransform.AddForce(newVelocity, ForceMode.Impulse);
    }
}
