using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자: 이예린
/// 
/// 원소 우선순위 데이터를 저장하는 스크립터블 오브젝트 클래스
/// 여러 원소 그룹(ElementGroup)으로 구성되며, 각 그룹은 동일 우선순위 원소들의 집합
/// 
/// 우선순위는 priorityGroups 리스트의 인덱스 순서에 따라 결정됨
/// </summary>
[CreateAssetMenu(fileName = "ElementPriorityData", menuName = "Scriptable Objects/ElementPriorityData")]
public class ElementPriorityData : ScriptableObject
{
    [Header("Element Casting Priority Settings")]
    [Tooltip("Element groups by priority. Lower index = higher priority.")]
    [SerializeField] List<ElementGroup> priorityGroups;

    /// <summary>
    /// 우선순위 그룹 리스트
    /// 인덱스가 낮을수록 우선순위가 낮음
    /// </summary>
    public List<ElementGroup> PriorityGroups => priorityGroups;
}

/// <summary>
/// 개발자: 이예린
/// 
/// 동일 우선순위 내 원소들의 집합을 나타내는 클래스
/// 각 그룹 내에 여러 원소가 포함될 수 있음
/// </summary>
[Serializable]
public class ElementGroup
{
    /// <summary>
    /// 이 그룹에 포함된 원소 리스트
    /// </summary>
    public List<E_CastingType> elements;
}