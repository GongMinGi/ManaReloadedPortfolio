using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ElementSlot : MonoBehaviour
{
    [SerializeField] PreGameFlowManager gameFlowManager;
    [SerializeField] Image  elementIcon;
    [SerializeField] Sprite fireSprite;
    [SerializeField] Sprite lightSprite;
    [SerializeField] Sprite thunderSprite;
    [SerializeField] Sprite earthSprite;
    [SerializeField] Sprite darknessSprite;
    [SerializeField] Sprite coldSprite;
    [SerializeField] Sprite waterSprite;
    [SerializeField] Sprite airSprite;

    public E_CastingType curType;               // 현재 담당하는 속성
    public Key curKey;                          // 이 슬롯이 담당하는 캐시

    public void OnClickElementInList()
    {
        gameFlowManager.ChangeToSelectedElement(this);
    }

    public void OnClickEquipedElement()
    {
        gameFlowManager.OpenElementList(this);
    }

    public void SetElementToThisSlot(E_CastingType typeToSet)
    {
        curType = typeToSet;
        elementIcon.sprite = typeToSet switch
        {
            E_CastingType.Fire     => fireSprite,
            E_CastingType.Light    => lightSprite,
            E_CastingType.Thunder  => thunderSprite,
            E_CastingType.Earth    => earthSprite,
            E_CastingType.Darkness => darknessSprite,
            E_CastingType.Cold     => coldSprite,
            E_CastingType.Water    => waterSprite,
            E_CastingType.Air      => airSprite,
            _ => null
        };
    }

}
