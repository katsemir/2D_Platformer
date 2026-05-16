using UnityEngine;
using TMPro;

public class ShopInteractable : MonoBehaviour
{
    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;
    public TMP_Text interactHintText;
    public ShopUI shopUI;

    [Header("Touch / Mouse")]
    public Camera mainCamera;

    private bool playerInRange = false;
    private PlayerController currentPlayer;

    private void Start()
    {
        SetHintVisible(false);

        if (shopUI == null)
        {
            shopUI = FindFirstObjectByType<ShopUI>();
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (shopUI == null)
        {
            Debug.LogWarning("ShopInteractable: ShopUI not found in scene.");
        }

        if (mainCamera == null)
        {
            Debug.LogWarning("ShopInteractable: Main Camera not found.");
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
            OpenShop();
            return;
        }

        if (WasShopClickedOrTouched())
        {
            OpenShop();
        }
    }

    private bool WasShopClickedOrTouched()
    {
        if (mainCamera == null)
            return false;

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 worldPoint = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(worldPoint);

            if (hit != null && IsThisShopObject(hit))
            {
                return true;
            }
        }

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                Vector2 worldPoint = mainCamera.ScreenToWorldPoint(touch.position);
                Collider2D hit = Physics2D.OverlapPoint(worldPoint);

                if (hit != null && IsThisShopObject(hit))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool IsThisShopObject(Collider2D hit)
    {
        return hit.gameObject == gameObject || hit.transform.IsChildOf(transform);
    }

    private void OpenShop()
    {
        if (!playerInRange)
            return;

        if (shopUI == null)
            return;

        if (shopUI.IsOpen)
            return;

        if (currentPlayer == null)
        {
            currentPlayer = FindFirstObjectByType<PlayerController>();
        }

        shopUI.OpenShop(currentPlayer);
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
        OpenShop();
    }

    private void SetHintVisible(bool visible)
    {
        if (interactHintText != null)
        {
            interactHintText.gameObject.SetActive(visible);
        }
    }
}