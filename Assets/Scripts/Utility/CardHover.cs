using UnityEngine;
using UnityEngine.EventSystems;

public class CardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject descriptionBackground; // Reference to the description background
    private bool isHovered = false; // Flag to check hover state

    // Called when the mouse enters the card area (image)
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isHovered)
        {
            descriptionBackground.SetActive(true);
            isHovered = true; // Set hover flag
        }
    }

    // Called when the mouse exits the card area (image)
    public void OnPointerExit(PointerEventData eventData)
    {
        if (isHovered)
        {
            descriptionBackground.SetActive(false);
            isHovered = false; // Reset hover flag
        }
    }
}
