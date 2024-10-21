using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VRMovement : MonoBehaviour
{
    private ActionBasedContinuousMoveProvider moveProvider;
    public event UpdateGunMovementDelegate OnUpdateGunMovement;
    public delegate void UpdateGunMovementDelegate(bool isWalking);

    void Start()
    {
        moveProvider = GetComponent<ActionBasedContinuousMoveProvider>();

        moveProvider.leftHandMoveAction.action.performed += ctx =>
        {
            moveProvider.moveSpeed = 5.0f;
        };

        moveProvider.leftHandMoveAction.action.canceled += _ =>
        {
            moveProvider.moveSpeed = 0.0f;
        };
    }

    void Update()
    {
        // 카메라 방향과 상관없이 이동
        bool isWalking = moveProvider.moveSpeed > 0;
        OnUpdateGunMovement?.Invoke(isWalking);
    }
}
