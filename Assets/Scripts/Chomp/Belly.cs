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
    private float _current = 0.4f;
    [SerializeField] private float _diminishingVel = 0.2f;


    [SerializeField] private GameObject _dieCanvas;
    [SerializeField] private Text _dieText;
    private string _dieStarve = "You fucking starved !! LMAO! L";
    private string _dieExplode = "WOW! You phat fuck!!\nYou fucking exploded!";

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
        ParticleSystem.EmissionModule emission = _blood.emission;
        emission.enabled = false;

        _dieCanvas.SetActive(false);
        _bellyScale.localScale = _minScale;
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
        if (Dead) return;
        Dead = true;
        StartCoroutine(DieCoroutine(explode));
    }

    private IEnumerator DieCoroutine(bool explode)
    {
        _dieCanvas.SetActive(true);
        _dieCanvas.SetActive(true);
        _dieText.text = explode ? _dieExplode : _dieStarve;

        if (explode)
        {
            Sound.PlaySound(_source, _explodeSounds);
            
            Vector3 newScale = _maxScale * 1.4f;
            DOTween.To(() => _bellyScale.localScale, x => _bellyScale.localScale = x, newScale, 1.6f).SetEase(Ease.InOutElastic);

            yield return new WaitForSeconds(1.657f);

            ParticleSystem.EmissionModule emission = _blood.emission;
            emission.enabled = true;
            _blood.Play();
            
            newScale = _maxScale * 3f;
            Vector3 newPos = _bellyScale.position + Vector3.up*2f;
            DOTween.To(() => _bellyScale.localScale, x => _bellyScale.localScale = x, newScale, 0.5f).SetEase(Ease.InOutElastic);
            DOTween.To(() => _bellyScale.position, x => _bellyScale.position = x, newPos, 1f);

            yield return new WaitForSeconds(1f);
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
            {
                Debug.Log("Starving break;");
                _starving = null;
                yield break;
            }

            if (Random.value > Mathf.Lerp(0.6f, 0.95f, Mathf.InverseLerp(0f, 40f, time)))
            {
                Sound.PlaySound(_source, _starveSounds);
            }

            Debug.Log("Starving! " + time);

            time -= Time.timeScale;
            yield return null;
        }

        DIE(false);
        _starving = null;
    }

    private Coroutine _starving;

    private void Update()
    {
        if (Dead) return;
        if (timer < 0f)
        {
            _current -= _diminishingVel / 1000f * Time.timeScale;

            if (_starving == null && _current < 0f)
            {
                _starving = StartCoroutine(Starve());
            }
            
            _current = Mathf.Clamp01(_current);
            _bellyScale.localScale = Vector3.Lerp(_minScale, _maxScale, _current);
        }
        else
            timer -= Time.timeScale;

        if (_camera == null || _forward == null) return;

        Vector3 cameraDir = _camera.forward;
        cameraDir.y = 0;
        cameraDir.Normalize();

        Quaternion targetRotation = Quaternion.LookRotation(cameraDir, Vector3.up);
        _bellyRotation.rotation = targetRotation;
    }
}
