using UnityEngine;

public class InfiniteParallax : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;

    [Header("Parallax")]
    [Range(0f, 1f)] public float parallaxX = 0.3f;

    [Header("Tiles")]
    public Transform[] tiles;

    private float spriteWidth;
    private float lastCameraX;

    void Start()
    {
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (tiles == null || tiles.Length == 0)
        {
            Debug.LogWarning("Tiles are not assigned in InfiniteParallax on " + gameObject.name);
            return;
        }

        SpriteRenderer sr = tiles[0].GetComponent<SpriteRenderer>();

        if (sr == null)
        {
            Debug.LogWarning("First tile has no SpriteRenderer on " + gameObject.name);
            return;
        }

        spriteWidth = sr.bounds.size.x;
        lastCameraX = cameraTransform.position.x;
    }

    void LateUpdate()
    {
        if (cameraTransform == null || tiles == null || tiles.Length == 0)
            return;

        float cameraDeltaX = cameraTransform.position.x - lastCameraX;

        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].position += new Vector3(cameraDeltaX * parallaxX, 0f, 0f);
        }

        RecycleTiles();

        lastCameraX = cameraTransform.position.x;
    }

    void RecycleTiles()
    {
        Transform leftTile = tiles[0];
        Transform rightTile = tiles[0];

        for (int i = 1; i < tiles.Length; i++)
        {
            if (tiles[i].position.x < leftTile.position.x)
                leftTile = tiles[i];

            if (tiles[i].position.x > rightTile.position.x)
                rightTile = tiles[i];
        }

        float cameraLeftEdge = cameraTransform.position.x - 10f;
        float cameraRightEdge = cameraTransform.position.x + 10f;

        if (leftTile.position.x + spriteWidth < cameraLeftEdge)
        {
            leftTile.position = new Vector3(
                rightTile.position.x + spriteWidth,
                leftTile.position.y,
                leftTile.position.z
            );
        }

        if (rightTile.position.x - spriteWidth > cameraRightEdge)
        {
            rightTile.position = new Vector3(
                leftTile.position.x - spriteWidth,
                rightTile.position.y,
                rightTile.position.z
            );
        }
    }
}