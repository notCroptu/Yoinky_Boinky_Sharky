using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

public class Fin : MonoBehaviour
{
    [SerializeField] private Animator _animator;


        [Header("Grip")]

        [SerializeField]
        Vector2 m_GripRightRange = new Vector2(-0.0125f, -0.011f);

        [SerializeField]
        XRInputValueReader<float> m_GripInput = new XRInputValueReader<float>("Grip");

        void OnEnable()
        {
            if (m_GripRightRange == null)
            {
                enabled = false;
                Debug.LogWarning($"Controller Animator component missing references on {gameObject.name}", this);
                return;
            }
            
            m_GripInput?.EnableDirectActionIfModeUsed();
        }

        void OnDisable()
        {
            m_GripInput?.DisableDirectActionIfModeUsed();
        }

        void Update()
        {
            if (m_GripInput != null)
            {
                var gripVal = m_GripInput.ReadValue();
                _animator.SetFloat("Blend", Mathf.Lerp(m_GripRightRange.x, m_GripRightRange.y, gripVal));
            }
        }
}
