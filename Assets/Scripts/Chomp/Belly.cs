using System.Collections;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Belly : MonoBehaviour
{
    [SerializeField] private Transform _bellyScale;
    [SerializeField] private Transform _bellyRotation;
    [SerializeField] private Vector3 _minScale = new(0.5f, 0.3f, 0.5f);
    [SerializeField] private Vector3 _maxScale = new(1.5f, 0.9f, 1.5f);
    [SerializeField] private float _scalingAmount = 0.2f;
    private float _current = 0f;
    [SerializeField] private float _diminishingVel = 0.2f;


    [SerializeField] private GameObject _dieCanvas;
    [SerializeField] private Text _dieText;
    [SerializeField] private string _dieStarve;
    [SerializeField] private string _dieExplode;

    [SerializeField] private float _cameraInfluence = 0.8f;
    [SerializeField] private Transform _camera;
    [SerializeField] private Transform _forward;

    [SerializeField] private ParticleSystem _blood;


    [SerializeField] private AudioClip[] _bellySounds;
    [SerializeField] private AudioClip[] _starveSounds;
    [SerializeField] private AudioClip[] _explodeSounds;
    [SerializeField] private AudioSource _source;

    public UnityEvent _onDie;

    private bool Dead = false;
    private float timer;
    private void OnEnable()
    {
        _dieCanvas.SetActive(false);
        _bellyScale.localScale = _minScale;
        _current = 0f;
    }

    [Button]
    public void OnEat()
    {
        if (Dead) return;

        _current += _scalingAmount;
        if (_current > 1f)
        {
            DIE(true);
            return;
        }
        _current = Mathf.Clamp01(_current);

        Vector3 newScale = Vector3.Lerp(_minScale, _maxScale, _current);
        DOTween.To(() => _bellyScale.localScale, x => _bellyScale.localScale = x, newScale, 1.2f).SetEase(Ease.OutElastic);
        timer = 2f;
    }

    public void DIE(bool explode)
    {
        _dieCanvas.SetActive(true);
        _dieCanvas.SetActive(true);
        _dieText.text = explode ? _dieExplode : _dieStarve;

        if (explode)
        {
            Vector3 newScale = _maxScale * 3f;
            Vector3 newPos = _bellyScale.position + Vector3.up;
            DOTween.To(() => _bellyScale.localScale, x => _bellyScale.localScale = x, newScale, 0.4f).SetEase(Ease.OutElastic);
            DOTween.To(() => _bellyScale.position, x => _bellyScale.position = x, newPos, 0.4f).SetEase(Ease.OutElastic);
            _blood.Emit(64);

            Sound.PlaySound(_source, _explodeSounds);
        }
        else
            Sound.PlaySound(_source, _starveSounds);

        _onDie?.Invoke();
    }

    private IEnumerator Starve()
    {
        float time = 40f;
        while (time > 0f)
        {
            if (_current > 0.01f)
                yield break;

            if (Random.value > Mathf.Lerp(0.6f, 0.95f, Mathf.InverseLerp(0f, 30f, time)))
            {
                Sound.PlaySound(_source, _starveSounds);
            }

            time -= Time.timeScale;
            yield return null;
        }

        DIE(false);
    }

    private Coroutine _starving;

    private void Update()
    {
        if (Dead) return;
        if (timer < 0f)
        {
            _current -= _diminishingVel / 1000f * Time.timeScale;
            if (_starving != null)
                _starving = StartCoroutine(Starve());
            _current = Mathf.Clamp01(_current);
            _bellyScale.localScale = Vector3.Lerp(_minScale, _maxScale, _current);
            Debug.Log("BELLYYYYY: " + _current);
        }
        else
            timer -= Time.timeScale;

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
