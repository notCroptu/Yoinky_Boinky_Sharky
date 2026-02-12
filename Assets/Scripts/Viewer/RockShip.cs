using System;
using UnityEngine;

public class RockShip : MonoBehaviour
{

    [SerializeField] private float _bobbingMultiplier = 0.5f;
    [SerializeField] private float _pitchMultiplayer = 0.4f;
    [SerializeField] private float _rollMultiplier = 0.4f;
    private float _originalY;
    private Quaternion _startRotation;

    private void Start()
    {
        _originalY = transform.position.y;
        _startRotation = transform.rotation;
    }

    private void Update()
    {
        // bob up and down
        float bob = (float)Math.Sin(Time.time) * _bobbingMultiplier;
        transform.position = transform.position + new Vector3(0f, bob, 0f);

        Quaternion newRotation = _startRotation;
        // side to side
        float roll = Mathf.Sin(Time.time * _rollMultiplier);
        newRotation *= Quaternion.AngleAxis(roll, Vector3.forward);

        // front to back
        float pitch = Mathf.Sin(Time.time * _pitchMultiplayer);
        newRotation *= Quaternion.AngleAxis(pitch, Vector3.left);

        transform.rotation = newRotation;
    }
}