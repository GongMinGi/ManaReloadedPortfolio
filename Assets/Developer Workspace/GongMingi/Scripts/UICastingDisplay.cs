using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



/// <summary>
/// 개발자: 공민기
/// 
/// 캐스팅 UI 슬롯을 담당하는 컴포넌트입니다.
/// - slotImages 배열로 슬롯들을 보유하고, elementColorMap에 각 원소 타입-별 색상을 매핑해둔 뒤
/// - UpdateSlot()으로 특정 인덱스의 이미지 색을 변경하고, ResetAll()로 전체 초기화합니다.
/// </summary>
public class UICastingDisplay : MonoBehaviour
{

    #region field and property
    [SerializeField] private Image[] slotImages;
    [SerializeField] private Color defaultColor = Color.black;

    [System.Serializable] private struct ElementColor
    {
        public E_CastingType type;
        public Color color;
    }

    [SerializeField] private ElementColor[] elementColors;

    private readonly Dictionary<E_CastingType, Color> elementColorMap = new();


    #endregion


    /// <summary>
    /// 초기화 루틴.
    /// - 인스펙터에 지정된 elementColors 배열을 딕셔너리 elementColorMap에 옮겨
    ///   <see cref="E_CastingType"/>, <see cref="Color"/> 매핑을 만든다.
    ///   => 인스펙터에서 정해놓은 속성별 색상을 할당해놓는다는 의미
    /// - 호출 직후 <see cref="ResetAll"/>을 실행해 모든 슬롯을 기본색(defaultColor)으로 설정한다.
    /// </summary>
    private void Awake()
    {
        foreach(var element in elementColors)
        {
            elementColorMap[element.type] = element.color;
        }
        ResetAll();
    }


    /// <summary>
    /// 지정한 슬롯 인덱스에 해당 원소 타입 색상을 적용한다.
    /// - 인덱스가 범위를 벗어나면 아무 작업도 하지 않는다.  
    /// - 매핑에 색상이 없을 경우 Color.white를 사용하고,
    ///   투명도(alpha 채널)는 1로 강제하여 완전 불투명하게 표시한다.
    ///   
    /// => 위에서 가져온 색상값을 실제로 UI에 적용하는 함수
    /// </summary>
    /// <param name="type">적용할 원소 타입</param>
    /// <param name="index">업데이트할 슬롯 인덱스</param>
    public void UpdateSlot(E_CastingType type, int index)
    {
        if (index < 0 || index >= slotImages.Length) return;

        Color baseCol = elementColorMap.TryGetValue(type, out var color) ? color : Color.white;

        baseCol.a = 1f;

        slotImages[index].color = baseCol;
    }


    /// <summary>
    /// 모든 슬롯 이미지를 기본색(<c>defaultColor</c>)으로 초기화한다.
    /// </summary>
    public void ResetAll()
    {
        foreach (Image img in slotImages) img.color = defaultColor;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
