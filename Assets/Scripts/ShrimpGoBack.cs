using System.Collections;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

public class ShrimpGoBack : MonoBehaviour
{
    [SerializeField] private float _goBackAfterSeconds = 10f;
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private CanvasGroup _tutorial;
    private Vector3 _initPos;
    private Quaternion _initRot;
    private Coroutine _cor;

    private void Start()
    {
        _initPos = transform.position;
        _initRot = transform.rotation;
    }

    [Button]
    public void GoBack()
    {
        if (_cor != null)
            StopCoroutine(_cor);
        _cor = StartCoroutine(StartWait());
    }

    private IEnumerator StartWait()
    {
        DOTween.To(() => _tutorial.alpha, x => _tutorial.alpha = x, 0f, 1f).SetEase(Ease.InOutSine);

        yield return new WaitForSeconds(_goBackAfterSeconds);

        _rigidbody.isKinematic = true;
        DOTween.To(() => transform.position, x => transform.position = x, _initPos, 2f).SetEase(Ease.InOutElastic);
        transform.DORotateQuaternion(_initRot, 2f).SetEase(Ease.InOutElastic);

        yield return new WaitForSeconds(2.2f);

        DOTween.To(() => _tutorial.alpha, x => _tutorial.alpha = x, 1f, 1f).SetEase(Ease.InOutSine);

        _rigidbody.isKinematic = false;
    }
}
