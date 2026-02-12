using System.Collections;
using UnityEngine;

public class Spine : Member
{
    [SerializeField] protected GameObject _fishermin;
    [SerializeField] protected GameObject _painFace;
    [SerializeField] protected GameObject _death;
    protected override IEnumerator ShowFace()
    {
        _blood.transform.position = transform.position;
        _blood.Emit(48);

        yield return new WaitForSeconds(0.5f);
        
        foreach (GameObject go in _normal)
            go.SetActive(false);
        foreach (GameObject go in _pain)
            go.SetActive(false);
        
        _painFace.SetActive(true);

        Sound.PlaySound(_screamSource, _screamSounds);
            
        yield return new WaitForSeconds(2f);
        _death.SetActive(true);
        yield return new WaitForSeconds(2f);

        Destroy(_fishermin);
    }
}
