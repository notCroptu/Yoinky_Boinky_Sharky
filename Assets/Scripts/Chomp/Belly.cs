using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

public class Belly : MonoBehaviour
{
    [SerializeField] private Transform _bellyScale;
    [SerializeField] private Transform _bellyRotation;
    [SerializeField] private Vector3 _minScale = new(0.5f, 0.3f, 0.5f);
    [SerializeField] private Vector3 _maxScale = new(1.5f, 0.9f, 1.5f);
    [SerializeField] private float _scalingAmount = 0.2f;
    private float _current = 0f;

    [SerializeField] private float _cameraInfluence = 0.8f;
    [SerializeField] private Transform _camera;
    [SerializeField] private Transform _forward;
    

    private void Start()
    {
        _bellyScale.localScale = _minScale;
    }

    [Button]
    public void OnEat()
    {
        _current += _scalingAmount;
        _current = Mathf.Clamp01(_current);

        Vector3 newScale = Vector3.Lerp(_minScale, _maxScale, _current);
        DOTween.To(() => _bellyScale.localScale, x => _bellyScale.localScale = x, newScale, 1.2f).SetEase(Ease.OutElastic);
    }

    private void Update()
    {
        if (_camera == null || _forward == null) return;

        Vector3 cameraDir = _camera.position - _bellyRotation.position;
        cameraDir.y = 0;
        cameraDir.Normalize();

        Vector3 forwardDir = _forward.forward;
        forwardDir.y = 0;
        forwardDir.Normalize();

       Vector3 blendedDir = Vector3.Slerp(forwardDir, cameraDir, _cameraInfluence).normalized;

        if (blendedDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(blendedDir, Vector3.up);
            _bellyRotation.rotation = targetRotation;
        }
    }
}
