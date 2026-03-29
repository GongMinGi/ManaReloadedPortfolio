using UnityEngine;
using UnityEngine.EventSystems;

public class InteractPoint : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] PopupController popup;
    [SerializeField] GameObject outline;
    bool canInteract = false;

    public void BeginInteract()
    {
        popup.RequestOpen();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (canInteract == true)
        {
            BeginInteract();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        canInteract = true;
        outline.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        canInteract = false;
        outline.SetActive(false);
    }
}