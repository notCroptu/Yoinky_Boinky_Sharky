using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;

public class Ragdoll : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private Collider _mainCollider;
    [SerializeField] private Rigidbody _mainRigidbody;
    // [SerializeField] private SkinnedMeshRenderer _skinnedMeshRenderer;
    [SerializeField] private GameObject _ragdollSetup;

    private Rigidbody[] _rigidbodies;
    public List<Collider> Colliders { get; private set; }

    private void Awake()
    {
        /*_storage = new GameObject("Ragdoll Setup Storage");
        _storage.transform.parent = transform;

        _ragdollSetup.transform.parent = _storage.transform;

        _regularSetup = Instantiate(_ragdollSetup, transform);
        _regularSetup.name = _ragdollSetup.name;

        Rigidbody[] rbs = _regularSetup.GetComponentsInChildren<Rigidbody>(true);
        CharacterJoint[] cjs = _regularSetup.GetComponentsInChildren<CharacterJoint>(true);
        Collider[] cs = _regularSetup.GetComponentsInChildren<Collider>(true);
        // IInteractable[] is = _regularSetup.GetComponentsInChildren<IInteractable>(true);

        foreach (CharacterJoint cj in cjs)
            Destroy(cj);
        foreach (Rigidbody rb in rbs)
            Destroy(rb);
        foreach (Collider c in cs)
            Destroy(c);
        // foreach (IInteractable i in is)
        //    Destroy(i);

        _storage.SetActive(false);*/

        _rigidbodies = GetComponentsInChildren<Rigidbody>(true);
        _ragdollSetup.SetActive(false);
        _animator.enabled = true;
        _mainCollider.enabled = true;

        Colliders = GetComponentsInChildren<Collider>(true).ToList();
        
        if (_mainCollider != null)
            Colliders.Remove(_mainCollider);

        // _skinnedMeshRenderer.rootBone = _regularSetup.transform;
    }

    [Button("Activate Ragdoll")]
    public void ActivateRagdoll()
    {
        if (!Application.isPlaying) return;

        _animator.enabled = false;
        _mainCollider.enabled = false;
        _mainRigidbody.isKinematic = true;
        _ragdollSetup.SetActive(true);

        /*_regularSetup.transform.parent = _storage.transform;
        _ragdollSetup.transform.parent = transform;
        _skinnedMeshRenderer.rootBone = _ragdollSetup.transform;*/

        Destroy(_mainRigidbody);
        Destroy(_mainCollider);
    }

    [Button("Activate Ragdoll with Impulse")]
    public void DebugActivateRagdollWithImpulse()
    {
        ActivateRagdoll(Vector3.forward * 10, new Vector3(0f, 0f, 0.8f));
    }

    public void ActivateRagdoll(Vector3 force, Vector3 position)
    {
        ActivateRagdoll();
        Push(force, position);
    }

    public void Push(Vector3 force, Vector3 position)
    {
        foreach (Rigidbody rb in _rigidbodies)
            rb.AddForceAtPosition(force, position, ForceMode.Impulse);
    }
}
