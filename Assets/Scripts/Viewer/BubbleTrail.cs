using UnityEngine;

public class BubbleTrail : MonoBehaviour
{
    [SerializeField] private ParticleSystem _particleSystem;
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private Vector2 _speeds = new(5f, 11f);
    [SerializeField] private Vector2 _rateOverTime = new(0f, 5f);

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
            enabled = false;
        }
    }

    private void Update()
    {
        float speed = _rigidbody.linearVelocity.magnitude;

        // make new color alpha vary by lerping with speed
        float rate = Mathf.InverseLerp(_speeds.x, _speeds.y, speed);

        _module.rateOverTime = Mathf.Lerp(_rateOverTime.x, _rateOverTime.y, rate);
    }
}
