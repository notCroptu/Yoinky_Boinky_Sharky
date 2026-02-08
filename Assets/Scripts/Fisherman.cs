using System.Collections;
using DG.Tweening;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine;

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
    
    [SerializeField] private GameObject[] FishermanPrefabs;
    [SerializeField] private GameObject FishingCanePrefab;
    [SerializeField][Min(1)] private int _initializeFisherAtATime = 4;
    [SerializeField] private Transform _fisherPool;
    [SerializeField] private Transform _fishingCanePool;
    [SerializeField][Min(1f)] private float _pullingForce;
    [SerializeField][Min(0f)] private float _spawnRadius = 5f;

    [SerializeField][Min(0.1f)] private float _waitTime = 2.3f;
    [SerializeField][Range(0f, 1f)] private float _possibility = 0.2f;


    private void Start()
    {
        _hookTransform.isKinematic = true;
        _hookTransform.transform.position = _origin.transform.position;

        _mainObject.transform.position = _origin.transform.position;
        _fisherPool.gameObject.SetActive(false);

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

        _hookTransform.isKinematic = false;
        Vector3 newVelocity = _origin.position
            + new Vector3(
                (Random.value * 2 - 1f) * _radius,
                -_radius,
                (Random.value * 2 - 1f) * _radius
            );
        _hookTransform.AddForce(newVelocity, ForceMode.Impulse);
    }
}
