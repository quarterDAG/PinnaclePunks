using UnityEngine;

public class DragDrop : MonoBehaviour
{
    public GameObject enemyPrefab;
    private bool isDragging = false;
    private Vector3 offset;

    void Update ()
    {
        if (isDragging)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(mousePos.x + offset.x, mousePos.y + offset.y, transform.position.z);
        }

        if (Input.GetMouseButtonDown(0) && IsMouseOverObject())
        {
            StartDragging();
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            StopDragging();
        }
    }

    bool IsMouseOverObject ()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D collider = GetComponent<Collider2D>();

        if (collider.OverlapPoint(mousePos))
        {
            return true;
        }
        return false;
    }

    void StartDragging ()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset = transform.position - mousePos;
        isDragging = true;
    }

    void StopDragging ()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Instantiate(enemyPrefab, worldPosition, Quaternion.identity);
        isDragging = false;
    }
}
