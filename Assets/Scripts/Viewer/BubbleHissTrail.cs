using UnityEngine;

public class BubbleHissTrail : MonoBehaviour
{
    [SerializeField] private ParticleSystem _particleSystem;
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private Vector2 _speeds = new(5f, 11f);
    [SerializeField] private Vector2 _rateOverTime = new(0f, 5f);
    private Transform _center;
    [SerializeField] private float _followInterpolation = 0.9f;

    private Vector3 _currentPos;

    [SerializeField] private AudioSource _source;
    [SerializeField] private Vector2 _minMax;
    public AnimationCurve _speedToVolume = AnimationCurve.EaseInOut(0f, 0.2f, 1, 1);
    public AnimationCurve _speedToPitch = AnimationCurve.EaseInOut(0f, 0.2f, 1, 1);

    private ParticleSystem.EmissionModule _module;

    private void Awake()
    {
        _module = _particleSystem.emission;
    }

    private void Start()
    {
        if (_rigidbody == null)
        {
            Debug.Log("Rigidbody not assigned. ");
            _rigidbody = GetComponentInParent<Rigidbody>();
        }
        else
            _center = _rigidbody.transform;
    }

    private void Update()
    {
        float speed = _rigidbody.linearVelocity.magnitude;

        // make new color alpha vary by lerping with speed
        float rate = Mathf.InverseLerp(_speeds.x, _speeds.y, speed);
        Debug.Log("Rate: " + rate);

        _module.rateOverTime = Mathf.Lerp(_rateOverTime.x, _rateOverTime.y, rate);
        UpdateVolume();
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        // Different interpolation help when visualizing the path ahead (the camera will inherently look to the velocity direction if follow > look)
        // must use exponential interpolation of the del because smoothness depends on frame rate?
        float t = 1f - Mathf.Exp(-_followInterpolation * Time.fixedDeltaTime);
        _currentPos = Vector3.Lerp(_currentPos, _center.position, _followInterpolation);
        transform.position = _currentPos;
    }

    private void UpdateVolume()
    {
        float value = Mathf.InverseLerp(_minMax.x, _minMax.y, _rigidbody.linearVelocity.magnitude);
        _source.volume = _speedToVolume.Evaluate(value);
        _source.pitch = _speedToPitch.Evaluate(value);
    }
}
