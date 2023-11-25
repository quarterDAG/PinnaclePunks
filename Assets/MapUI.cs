using UnityEngine;

public class MapUI : MonoBehaviour
{
    [SerializeField] private int mapIndex;

    private void OnTriggerEnter2D ( Collider2D collision )
    {
        InputIcon icon = collision.GetComponent<InputIcon>();
        if (icon != null)
        {
            icon.SetSelectedMap(mapIndex);
        }
    }

    private void OnTriggerExit2D ( Collider2D collision )
    {
        InputIcon icon = collision.GetComponent<InputIcon>();
        if (icon != null && icon.GetSelectedMap() == mapIndex)
        {
            icon.SetSelectedMap(-1);
        }
    }
}
