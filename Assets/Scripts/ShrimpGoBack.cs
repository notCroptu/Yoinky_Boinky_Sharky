using System.Collections;
using DG.Tweening;
using UnityEngine;

public class ShrimpGoBack : MonoBehaviour
{
    [SerializeField] private float _goBackAfterSeconds = 10f;
    [SerializeField] private Rigidbody _rigidbody;
    private Vector3 _initPos;
    private Quaternion _initRot;
    private Coroutine _cor;

    private void Start()
    {
        _initPos = transform.position;
        _initRot = transform.rotation;
    }

    public void GoBack()
    {
        if (_cor != null)
            StopCoroutine(_cor);
        _cor = StartCoroutine(StartWait());
    }

    private IEnumerator StartWait()
    {
        yield return new WaitForSeconds(_goBackAfterSeconds);

        _rigidbody.isKinematic = true;
        DOTween.To(() => transform.position, x => transform.position = x, _initPos, 3f).SetEase(Ease.InBounce);
        transform.rotation = _initRot;

        yield return new WaitForSeconds(3.2f);

        _rigidbody.isKinematic = false;
    }
}
