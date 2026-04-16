using UnityEngine;
using TMPro;

public class ShopInteractable : MonoBehaviour
{
    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;
    public TMP_Text interactHintText;
    public ShopUI shopUI;

    private bool playerInRange = false;
    private PlayerController currentPlayer;

    private void Start()
    {
        SetHintVisible(false);

        if (shopUI == null)
        {
            shopUI = FindFirstObjectByType<ShopUI>();
        }

        if (shopUI == null)
        {
            Debug.LogWarning("ShopInteractable: ShopUI not found in scene.");
        }
    }

    private void Update()
    {
        if (!playerInRange)
            return;

        if (shopUI == null)
            return;

        if (shopUI.IsOpen)
            return;

        if (Input.GetKeyDown(interactKey))
        {
            shopUI.OpenShop(currentPlayer);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        currentPlayer = collision.GetComponent<PlayerController>();

        if (currentPlayer == null)
        {
            currentPlayer = collision.GetComponentInParent<PlayerController>();
        }

        playerInRange = true;
        SetHintVisible(true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        playerInRange = false;
        currentPlayer = null;
        SetHintVisible(false);
    }

    public void InteractFromMobileButton()
    {
        if (!playerInRange)
            return;

        if (shopUI == null)
            return;

        if (shopUI.IsOpen)
            return;

        shopUI.OpenShop(currentPlayer);
    }

    private void SetHintVisible(bool visible)
    {
        if (interactHintText != null)
        {
            interactHintText.gameObject.SetActive(visible);
        }
    }
}