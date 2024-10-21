using UnityEngine;

public class IdleGun : MonoBehaviour
{
    private float breathSpeed = 1.0f;
    private float breathAmount = 0.02f;
    private float walkSpeed = 10.0f;
    private float walkAmount = 0.05f;

    private Vector3 originalPosition;

    void Awake()
    {
        originalPosition = transform.localPosition;

        VRMovement vrMovement = FindAnyObjectByType<VRMovement>();

        if (vrMovement != null)
        {
            vrMovement.OnUpdateGunMovement += UpdateGunMovement;
        }
    }

    private void UpdateGunMovement(bool isWalking)
    {
        if (isWalking)
        {
            float walkX = originalPosition.x + Mathf.Sin(Time.time * walkSpeed) * walkAmount;
            transform.localPosition = new Vector3(walkX, originalPosition.y, originalPosition.z);
        }
        else
        {
            float newY = originalPosition.y + Mathf.Sin(Time.time * breathSpeed) * breathAmount;
            transform.localPosition = new Vector3(originalPosition.x, newY, originalPosition.z);
        }
    }
}