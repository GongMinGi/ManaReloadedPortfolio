using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Loadout_Interact : MonoBehaviour, IInteractable, IPointerClickHandler
{
    [SerializeField] GameObject outline;
    [SerializeField] LayerMask playerMask;
    [SerializeField] bool canInteract;

    [Header("Lasdout Setting")]
    [SerializeField] Camera uiCamera;

    [SerializeField] GameObject loadoutUI;
    [SerializeField] GameObject gameUI;

    [SerializeField] GameObject loadoutObj;
    [SerializeField] bool onInteract;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (canInteract)
        {
            Interact();
        }
    }

    public void Interact()
    {
        if (onInteract == false)
        {
            onInteract = true;
            uiCamera.gameObject.SetActive(true);

            loadoutUI.SetActive(true);
            gameUI.SetActive(false);

            loadoutObj.SetActive(true);
        }
        else
        {
            onInteract = false;
            uiCamera.gameObject.SetActive(false);

            loadoutUI.SetActive(false);
            gameUI.SetActive(true);

            loadoutObj.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (playerMask.Contain(other.gameObject.layer))
        {
            outline.SetActive(true);
            canInteract = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (playerMask.Contain(other.gameObject.layer))
        {
            outline.SetActive(false);
            canInteract = false;
        }
    }
}
