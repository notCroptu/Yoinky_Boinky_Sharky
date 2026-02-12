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
        bool movingFastEnough = (Mathf.Abs(leftDeltaY) + Mathf.Abs(rightDeltaY)) > _speedThreshold;

        if ( horizontalDistance < _horizontalTolerance || verticalDistance < _chompDistance || movingOpposite || movingFastEnough)
            Debug.Log("In corrrect X: " + (horizontalDistance < _horizontalTolerance) + " In correct Y: " + (verticalDistance < _chompDistance) + " vertical y: " + verticalDistance + " moving opp: " + movingOpposite + " fast enough: " + movingFastEnough + " with vel: " + (Mathf.Abs(leftDeltaY) + Mathf.Abs(rightDeltaY)) +  " would win: " + (!isChomping && horizontalDistance < _horizontalTolerance && verticalDistance < _chompDistance && movingOpposite && movingFastEnough));

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

        Vector3 midway = (_leftController.forward.normalized + _rightController.forward.normalized).normalized;
        Vector3 position = (_leftController.position + _rightController.position)/2f;

        position -= midway.normalized * 0.2f;

        if (Physics.SphereCast(position, 0.2f, midway, out RaycastHit hit, _rayDistance, _targetLayer))
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
