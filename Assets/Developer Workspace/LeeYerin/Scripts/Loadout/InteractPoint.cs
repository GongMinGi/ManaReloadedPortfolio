using UnityEngine;
using UnityEngine.EventSystems;

public class InteractPoint : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] PopupController popup;
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
    }

    private void OnTriggerExit(Collider other)
    {
        canInteract = false;
    }
}