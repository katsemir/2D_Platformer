using UnityEngine;

public class MobileControlsUI : MonoBehaviour
{
    [Header("Player")]
    public PlayerController playerController;

    [Header("Buttons")]
    public GameObject dashButton;

    private void Start()
    {
        if (playerController == null)
        {
            playerController = FindFirstObjectByType<PlayerController>();
        }

        UpdateDashButtonVisibility();
    }

    private void Update()
    {
        UpdateDashButtonVisibility();
    }

    private void UpdateDashButtonVisibility()
    {
        if (dashButton == null)
            return;

        if (playerController == null)
        {
            dashButton.SetActive(false);
            return;
        }

        dashButton.SetActive(playerController.IsDashUnlocked());
    }

    public void LeftButtonDown()
    {
        if (playerController != null)
        {
            playerController.MobileMoveLeftDown();
        }
    }

    public void RightButtonDown()
    {
        if (playerController != null)
        {
            playerController.MobileMoveRightDown();
        }
    }

    public void MoveButtonUp()
    {
        if (playerController != null)
        {
            playerController.MobileMoveButtonUp();
        }
    }

    public void JumpButtonPressed()
    {
        if (playerController != null)
        {
            playerController.MobileJump();
        }
    }

    public void AttackButtonPressed()
    {
        if (playerController != null)
        {
            playerController.MobileAttack();
        }
    }

    public void DashButtonPressed()
    {
        if (playerController != null)
        {
            playerController.MobileDash();
        }
    }
}