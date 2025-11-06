using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자: 이예린
/// 
/// 게임에서 사용되는 원소별 스프라이트 데이터를 관리하는 ScriptableObject
/// 
/// ScriptableObject를 통해 인스펙터에서 원소-스프라이트 매핑을 설정하고,
/// 런타임에 Dictionary로 빠르게 조회 가능
/// </summary>
[CreateAssetMenu(fileName = "ElementSpriteData", menuName = "Scriptable Objects/Element/ElementSpriteData")]
public class ElementSpriteData : ScriptableObject
{
    /// <summary>
    /// 인스펙터에서 설정 가능한 원소별 스프라이트 리스트
    /// </summary>
    [SerializeField] List<ElementSprite> elementSpriteList = new ();

    private Dictionary<E_CastingType, Sprite> elementSpriteLookup = new ();

    /// <summary>
    /// ScriptableObject 초기화 메서드
    /// 리스트 데이터를 기반으로 Dictionary를 구성
    /// </summary>
    public void Initialization()
    {
        Debug.Log("ElementSpriteData 초기화 진행");
        foreach (var spriteDate in elementSpriteList)
        {
            elementSpriteLookup.Add(spriteDate.Element, spriteDate.Sprite);
        }
    }

    /// <summary>
    /// 지정한 원소 타입에 해당하는 스프라이트 반환
    /// </summary>
    /// <param name="element">조회할 원소 타입</param>
    /// <returns>해당 원소의 스프라이트</returns>
    public Sprite GetElemtSprite(E_CastingType element) => elementSpriteLookup[element];
}

/// <summary>
/// 개발자: 이예린
/// 
/// 원소 타입과 해당 스프라이트를 매핑하는 구조체
/// ScriptableObject 내 리스트에서 관리
/// </summary>
[Serializable]
public struct ElementSprite
{
    [SerializeField] private E_CastingType element;
    [SerializeField] private Sprite sprite;

    public E_CastingType Element => element;
    public Sprite Sprite => sprite;
}