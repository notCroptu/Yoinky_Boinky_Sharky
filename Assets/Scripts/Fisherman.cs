using System.Collections;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class Fisherman : MonoBehaviour
{
    [SerializeField] private GameObject _mainObject;
    [SerializeField] private Transform _origin;
    [SerializeField] private Collider _collider;
    [SerializeField][Min(0f)] private float _radius = 50f;
    [SerializeField][Min(0f)] private float _addedHeight = 5f;
    [SerializeField][Min(0f)] private float _time = 3f;
    [SerializeField][Min(0f)] private float _movingDuration = 1f;

    [SerializeField] private Transform _playerTransform;
    [SerializeField] private Rigidbody _hookTransform;
    [SerializeField] private Animator _hookAnim;
    [SerializeField] private XRGrabInteractable _hookGrabInteractable;
    [SerializeField] private Transform _hookPoint;
    [SerializeField] private Transform _startPoint;
    [SerializeField] private Transform _fishingPoint;
    [SerializeField] private LineRenderer _fishingLine;
    [SerializeField] private SplineContainer _spline;
    [SerializeField][Min(3)] private int _lineResolution = 30;
    [SerializeField] private float _hookTargetY = 2f;
    [SerializeField][Min(0f)] private float _triggeringDistance = 0.6f;

    [SerializeField] private GameObject[] FishermanPrefabs;
    [SerializeField][Min(1)] private int _initializeFisherAtATime = 4;
    [SerializeField] private Transform _fisherPool;
    [SerializeField] private GameObject _fishingCane;
    [SerializeField][Min(1f)] private float _pullingForce;
    [SerializeField][Min(0f)] private float _spawnRadius = 5f;

    [SerializeField][Min(0.1f)] private float _waitTime = 2.3f;
    [SerializeField][Range(0f, 1f)] private float _possibility = 0.2f;
    [SerializeField] private AudioClip[] _fallsounds;
    [SerializeField] private AudioSource _fallSource;
    [SerializeField] private AudioClip[] _pullSounds;
    [SerializeField] private AudioSource _pullSource;

    private void Awake()
    {
        Spline spline = _spline.Spline;
        spline.Clear();

        spline.Add(new BezierKnot(_startPoint.position));
        spline.Add(new BezierKnot(_fishingPoint.position));
        spline.Add(new BezierKnot(_fishingPoint.position));
        spline.Add(new BezierKnot(_hookTransform.position));

        spline.SetTangentMode(0, TangentMode.AutoSmooth);
        spline.SetTangentMode(1, TangentMode.AutoSmooth);
        spline.SetTangentMode(2, TangentMode.AutoSmooth);
        spline.SetTangentMode(3, TangentMode.AutoSmooth);

        Application.targetFrameRate = 90;
    }

    private Quaternion _originalHookRot;
    private void Start()
    {
        _originalHookRot = _hookTransform.rotation;
        _hookTransform.isKinematic = true;

        _mainObject.transform.position = _origin.transform.position;
        _fisherPool.gameObject.SetActive(false);

        for (int i = 0; i < _initializeFisherAtATime; i++)
        {
            InitNewFisher();
        }

        ChooseNewLocation();
    }


    private Coroutine _cor;
    public void PickUp()
    {
        if (_cor != null)
            StopCoroutine(_cor);
        _cor = StartCoroutine(PullHook());
        _hookAnim.StopPlayback();
    }

    private Vector3 _lastPos;
    private IEnumerator PullHook()
    {
        yield return new WaitForSeconds(1f);
        XRBaseInputInteractor controllerInteractor = _hookGrabInteractable.firstInteractorSelecting as XRBaseInputInteractor;
        _lastPos = _playerTransform.InverseTransformPoint(_hookTransform.position);

        float blendTimer = 0f;
        float blendDuration = 2f;
        bool isLingering = false;
        float lingerDuration = 0f;
        float lingerTimer = 0f;

        while (true)
        {
            if (isLingering)
            {
                _midPointBlend = 0f;
                lingerTimer += Time.deltaTime;

                if (lingerTimer >= lingerDuration)
                {
                    isLingering = false;
                }
            }
            else
            {
                blendTimer += Time.deltaTime;
                float tRaw = blendTimer / blendDuration % 2f;
                float t = (tRaw < 1f) ? tRaw : 2f - tRaw;
                _midPointBlend = Mathf.Lerp(1f, 0f, t);

                if (_midPointBlend <= 0.05f)
                {
                    isLingering = true;
                    lingerTimer = 0f;
                    lingerDuration = Random.Range(2f, 4f);
                    _midPointBlend = 0f;
                }
            }

            if (_midPointBlend <= 0.05f)
            {
                Vector3 worldDis = _hookTransform.position - _lastPos;

                if (controllerInteractor != null)
                    controllerInteractor.SendHapticImpulse(Mathf.Clamp(worldDis.magnitude, 0.5f, 1f), 0.2f);

                float pullAmount = Vector3.Dot(worldDis, -_playerTransform.up);

                Debug.Log("Pulling with NOT enough force of: " + worldDis.magnitude + " pullAmount: " + pullAmount);

                if (pullAmount > 0f && worldDis.magnitude >= _triggeringDistance/2)
                {
                    Sound.PlaySound(_pullSource, _pullSounds);

                    if (worldDis.magnitude >= _triggeringDistance)
                    {
                        Debug.Log("Pulling with enough force of: " + worldDis.magnitude + " pullAmount: " + pullAmount);
                        ReleaseHook();
                        ReverseFished(worldDis);
                        break;
                    }
                }

                _lastPos = _hookTransform.position;
            }

            yield return null;
        }

        DOTween.To(() => _midPointBlend, x => _midPointBlend = x, 1f, 2f).SetEase(Ease.OutSine);
    }

    [Button]
    public void DoUnoPull()
    {
        Sound.PlaySound(_pullSource, _pullSounds);

        if (Random.value < 0.3f)
        {
            ReleaseHook();
            ReverseFished(Vector3.down);
        }
    }

    /*private void ReleaseHook()
    {
        if (_hookGrabInteractable == null) return;

        if (_hookGrabInteractable.isSelected )
        {
            foreach (IXRSelectInteractor interactor in _hookGrabInteractable.interactorsSelecting)
            {
                if (interactor != null)
                {
                    _hookGrabInteractable.interactionManager.SelectExit(interactor, _hookGrabInteractable);
                    Debug.Log("Hook forcibly released!");
                }
            }
        }
    }*/

    private void ReleaseHook()
    {
        if (_hookGrabInteractable == null) return;

        if (_hookGrabInteractable.isSelected)
        {
            IXRSelectInteractor interactor = _hookGrabInteractable.firstInteractorSelecting;
            if (interactor != null)
            {
                _hookGrabInteractable.interactionManager.SelectExit(interactor, _hookGrabInteractable);
                Debug.Log("Hook forcibly released!");
            }
        }
    }

    public void Drop()
    {
        if (_cor != null)
            StopCoroutine(_cor);
        
        _hookAnim.StartPlayback();

        DOTween.To(() => _midPointBlend, x => _midPointBlend = x, 1f, 2f).SetEase(Ease.OutSine);
        _hookTransform.rotation = _originalHookRot;
    }
    private void InitNewFisher()
    {
        GameObject go = Instantiate(FishermanPrefabs[Random.Range(0, FishermanPrefabs.Length)], _fisherPool);
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

    public void ReverseFished(Vector3 direction)
    {
        Debug.Log("Reverse Fishing!");
        Sound.PlaySound(_fallSource, _fallsounds);
        Vector3 newFishingPos = _startPoint.localPosition; // *_spawnRadius;

        Transform fisher = _fisherPool.GetChild(0);
        fisher.transform.localPosition = newFishingPos;
        fisher.transform.Rotate(new Vector3(Random.Range(-90f, 90f), Random.Range(-90f, 90f), Random.Range(-90f, 90f)));
        Rigidbody rb = fisher.GetComponentInChildren<Rigidbody>();
        fisher.SetParent(null);

        if (rb != null)
        {
            rb.AddForce(direction * _pullingForce, ForceMode.Impulse);
            rb.AddTorque(direction * _pullingForce * 4f, ForceMode.Impulse);
        }

        if (_fishingCane.activeInHierarchy == false)
        {
            _fishingCane.transform.localPosition = newFishingPos;
            _fishingCane.transform.Rotate(new Vector3(Random.Range(-90f, 90f), Random.Range(-90f, 90f), Random.Range(-90f, 90f)));
            Rigidbody rbC = _fishingCane.GetComponentInChildren<Rigidbody>();
            _fishingCane.transform.SetParent(null);

            if (rbC != null)
            {
                rbC.AddForce(direction * _pullingForce, ForceMode.Impulse);
                rbC.AddTorque(direction * _pullingForce * 2f, ForceMode.Impulse);
            }
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

    private float _midPointBlend = 1f;
    [SerializeField] private float amplitude = 4f;
    [SerializeField] private float speed1 = 1f;
    [SerializeField] private float speed2 = 0.7f;

    private void UpdateSpline()
    {
        Spline spline = _spline.Spline;

        Vector3 startPos = _startPoint.position;
        Vector3 endPos = _hookPoint.position;

        Vector3 mid1Base = Vector3.Lerp(startPos, endPos, 1f / 3f);
        Vector3 mid2Base = Vector3.Lerp(startPos, endPos, 2f / 3f);

        Vector3 mid1Osc = new Vector3(
            Mathf.Sin(Time.time * speed1) * amplitude,
            Mathf.Cos(Time.time * speed1 * 0.5f) * amplitude,
            Mathf.Sin(Time.time * speed1 * 1.2f) * amplitude
        );

        Vector3 mid2Osc = new Vector3(
            Mathf.Sin(Time.time * speed2 + 1f) * amplitude,
            Mathf.Cos(Time.time * speed2 * 0.7f + 2f) * amplitude,
            Mathf.Sin(Time.time * speed2 * 1.5f + 3f) * amplitude
        );

        Vector3 mid1 = Vector3.Lerp(mid1Base, mid1Base + mid1Osc, _midPointBlend);
        Vector3 mid2 = Vector3.Lerp(mid2Base, mid2Base + mid2Osc, _midPointBlend);

        spline.SetKnot(0, new BezierKnot(startPos));
        spline.SetKnot(1, new BezierKnot(mid1));
        spline.SetKnot(2, new BezierKnot(mid2));
        spline.SetKnot(3, new BezierKnot(endPos));

        _fishingLine.positionCount = _lineResolution;
        for (int i = 0; i < _lineResolution; i++)
        {
            float t = i / (float)(_lineResolution - 1);
            _fishingLine.SetPosition(i, _spline.EvaluatePosition(t));
        }
    }

    private void FixedUpdate()
    {
        if (!_hookGrabInteractable.isSelected && _hookTransform.linearVelocity.magnitude < 0.1f &&
            (_hookTransform.position.y < (_hookTargetY - 2f) || _hookTransform.position.y > (_hookTargetY + 2f)))
        {
            RBGoToY(_hookTargetY, _hookTransform,Vector3.zero);
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

        Vector3 newVelocity = new Vector3((Random.value * 2 - 1f) * _radius, 0f, (Random.value * 2 - 1f) * _radius);
        RBGoToY(_hookTargetY, _hookTransform, newVelocity);
    }

    private void RBGoToY(float targetY, Rigidbody rb, Vector3 plus)
    {
        if (rb.isKinematic) return;

        float deltaH = targetY - rb.position.y;
        
        float velocityRequired = Mathf.Sign(deltaH) * Mathf.Sqrt(2 * Mathf.Abs(deltaH)) + (deltaH * rb.linearDamping);

        Vector3 newVelocity = plus + new Vector3(0f, velocityRequired * rb.mass, 0f) -  rb.linearVelocity;

        rb.AddForce(newVelocity, ForceMode.Impulse);
    }
}
