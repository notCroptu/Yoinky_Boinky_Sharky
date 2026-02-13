using System.Collections;
using DG.Tweening;
using NaughtyAttributes;
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

    [Button]
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
        DOTween.To(() => transform.position, x => transform.position = x, _initPos, 2f).SetEase(Ease.InOutElastic);
        transform.DORotateQuaternion(_initRot, 2f).SetEase(Ease.InOutElastic);

        yield return new WaitForSeconds(2.2f);

        _rigidbody.isKinematic = false;
    }
}
