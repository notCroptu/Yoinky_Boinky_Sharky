using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Feedback;

public class Chomp : MonoBehaviour
{
    [SerializeField] private Belly _belly;
    [SerializeField] private Transform _leftController;
    [SerializeField] private Transform _rightController;

    [SerializeField] private float _resetDistance = 0.8f;
    [SerializeField] private float _chompDistance = 0.3f;
    [SerializeField] private float _speedThreshold = 0.3f;
    [SerializeField] private LayerMask _targetLayer;
    [SerializeField] private float _rayDistance = 3f;
    [SerializeField] private float _horizontalTolerance = 0.25f;

    [SerializeField] private AudioClip[] _chompSounds;
    [SerializeField] private AudioSource _chompSource;

    private Vector3 _prevLeftPos;
    private Vector3 _prevRightPos;
    private bool isChomping = false;

    private void Update()
    {
        float horizontalDistance = Vector2.Distance(
            new Vector2(_leftController.position.x, _leftController.position.z),
            new Vector2(_rightController.position.x, _rightController.position.z));

        float verticalDistance = Mathf.Abs(_leftController.position.y - _rightController.position.y);

        float leftDeltaY = _leftController.position.y - _prevLeftPos.y;
        float rightDeltaY = _rightController.position.y - _prevRightPos.y;

        bool movingOpposite = leftDeltaY * rightDeltaY < 0;
        bool movingFastEnough = Mathf.Abs(leftDeltaY) > _speedThreshold &&
            Mathf.Abs(rightDeltaY) > _speedThreshold;

        if (!isChomping && horizontalDistance < _horizontalTolerance && verticalDistance < _chompDistance && movingOpposite && movingFastEnough)
            TryEatTarget();
        else if (verticalDistance > _resetDistance)
            isChomping = false;

        _prevLeftPos = _leftController.position;
        _prevRightPos = _rightController.position;
    }

    [SerializeField] private SimpleHapticFeedback _leftHap;
    [SerializeField] private SimpleHapticFeedback _rightHap;

    private void TryEatTarget()
    {
        isChomping = true;

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, _rayDistance, _targetLayer))
        {
            Member member = hit.collider.GetComponent<Member>();
            if (member != null)
            {
                _leftHap.hapticImpulsePlayer.SendHapticImpulse(1f, 0.6f);
                _rightHap.hapticImpulsePlayer.SendHapticImpulse(1f, 0.6f);
                Sound.PlaySound(_chompSource, _chompSounds);
                member.Eat();
                _belly.OnEat();
            }
        }
    }
}
