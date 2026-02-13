using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

public class Pause : MonoBehaviour
{
    [SerializeField] private GameObject _pause;
    [SerializeField]  private XRInputValueReader<bool> button;

    private bool wasPressed = false;

    void OnEnable()
    {
        if (button == null)
        {
            enabled = false;
            Debug.LogWarning($"Controller Animator component missing references on {gameObject.name}", this);
            return;
        }

        _pause.SetActive(false);
        button.EnableDirectActionIfModeUsed();
        button.inputAction.Enable();

    }

    void OnDisable()
    {
        button.DisableDirectActionIfModeUsed();
    }

    void Update()
    {
        if (button == null) return;

        bool isPressed = button.inputAction.IsPressed();

        if (isPressed && !wasPressed)
        {
            Debug.Log("PAUSED!!");
            _pause.SetActive(!_pause.activeSelf);
        }

        wasPressed = isPressed;
    }
}
