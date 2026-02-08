using UnityEngine;

public class BubbleTrail : MonoBehaviour
{
    [SerializeField] private ParticleSystem _particleSystem;
    [SerializeField] private Rigidbody _targetRb;
    [SerializeField] private Vector2 _speeds = new(5f, 11f);
    [SerializeField] private Vector2 _rateOverTime = new(0f, 5f);

    private ParticleSystem.EmissionModule _module;

    private void Awake()
    {
        _module = _particleSystem.emission;
    }

    private void Update()
    {
        float speed = _targetRb.linearVelocity.magnitude;

        // make new color alpha vary by lerping with speed
        float rate = Mathf.InverseLerp(_speeds.x, _speeds.y, speed);

        _module.rateOverTime = Mathf.Lerp(_rateOverTime.x, _rateOverTime.y, rate);
    }
}
