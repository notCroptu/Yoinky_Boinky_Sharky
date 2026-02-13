using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

public class Pause : MonoBehaviour
{
    [SerializeField] private GameObject _pause;
    [SerializeField] private XRInputValueReader<bool> button = new XRInputValueReader<bool>("<XRController>{LeftHand}/primaryButton");

    private bool wasPressed = false;

    void OnEnable()
    {
        if (button == null)
        {
            enabled = false;
            Debug.LogWarning($"Controller Animator component missing references on {gameObject.name}", this);
            return;
        }

        button.EnableDirectActionIfModeUsed();
    }

    void OnDisable()
    {
        button.DisableDirectActionIfModeUsed();
    }

    void Update()
    {
        if (button == null) return;

        bool isPressed = button.ReadValue();

        if (isPressed && !wasPressed)
        {
            _pause.SetActive(!_pause.activeSelf);
        }

        wasPressed = isPressed;
    }
}
