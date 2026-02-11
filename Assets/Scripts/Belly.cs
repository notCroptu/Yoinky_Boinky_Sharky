using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

public class Belly : MonoBehaviour
{
    [SerializeField] private Transform belly;
    [SerializeField] private Vector3 _minScale = new(0.5f, 0.3f, 0.5f);
    [SerializeField] private Vector3 _maxScale = new(1.5f, 0.9f, 1.5f);
    [SerializeField] private float _scalingAmount = 0.2f;
    private float _current = 0f;

    [Button]
    public void OnEat()
    {
        _current += _scalingAmount;
        _current = Mathf.Clamp01(_current);

        Vector3 newScale = Vector3.Lerp(_minScale, _maxScale, _current);
        DOTween.To(() => belly.localScale, x => belly.localScale = x, newScale, 2).SetEase(Ease.OutBounce);
    }
}
