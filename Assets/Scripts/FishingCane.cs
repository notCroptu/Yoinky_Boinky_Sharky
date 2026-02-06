using UnityEngine;

public class FishingCane : MonoBehaviour
{
    [SerializeField] private Hook _hook;

    private void Start()
    {
        _hook.enabled = false;
    }

    public void PickUp()
    {
        _hook.enabled = true;
    }

    public void Drop()
    {
        _hook.enabled = false;
    }
}
