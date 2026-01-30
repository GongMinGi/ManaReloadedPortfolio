using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class CastleGate_Interact : MonoBehaviour, IInteractable, IPointerClickHandler
{
    [SerializeField] GameObject outline;
    [SerializeField] LayerMask playerMask;
    [SerializeField] bool isOutGate;
    [SerializeField] bool canInteract;

    public void OnPointerClick(PointerEventData eventData)
    {
        if(canInteract)
        {
            Interact();
        }
    }

    private void Interact()
    {
        GameModeManager.GameLogicManager.ChageDayNightState();
        if (isOutGate == true)
        {
            GameModeManager.ExitGame();
        }
        else
        {
            // 게임 씬으로 넘어가야 함
            // 기존 게임 루프처럼 진행. but 하나의 씬에서
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(playerMask.Contain(other.gameObject.layer))
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
