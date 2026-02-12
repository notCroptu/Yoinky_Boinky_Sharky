using System.Collections;
using NaughtyAttributes;
using UnityEngine;

public class Member : MonoBehaviour
{
    [SerializeField] protected GameObject[] _pain;
    [SerializeField] protected GameObject[] _normal;
    [SerializeField] protected ParticleSystem _blood;

    [SerializeField] protected AudioClip[] _screamSounds;
    [SerializeField] protected AudioSource _screamSource;
    private bool _isEaten = false;

    // se deres scale down para quase 0 e virares 180 graus a joint do neck, elbow ou knee, parece q desaparece como ta nas imagens q mandei
    [Button]
    public void Eat()
    {
        if (_isEaten) return;
        _isEaten = true;

        StartCoroutine(ShowFace());
    }

    protected virtual IEnumerator ShowFace()
    {
        transform.localScale = Vector3.zero;

        _blood.transform.position = transform.position;
        _blood.Emit(16);

        yield return new WaitForSeconds(0.5f);
        
        foreach (GameObject go in _normal)
            go.SetActive(false);
        
        _pain[Random.Range(0, _pain.Length)].SetActive(true);

        Sound.PlaySound(_screamSource, _screamSounds);
            
        yield return new WaitForSeconds(4f);

        foreach(GameObject go in _pain)
            go.SetActive(false);
        
        _normal[Random.Range(0, _normal.Length)].SetActive(true);

        gameObject.SetActive(false);
    }
}
