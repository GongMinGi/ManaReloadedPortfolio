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

    [SerializeField] private Image[] slotImages;                    // 캐스팅 될 속성의 색을 표시할 이미지
    [SerializeField] private Color defaultColor = Color.black;      // 속성이 캐스팅되지 않았을 때의 

    [System.Serializable] private struct ElementColor
    {
        public E_CastingType type;           // 인스펙터에서 설정하는 들고 갈 속성 타입
        public Color color;                  // 설정한 속성이 ui 에서 표시될 색깔 설정
    }

    [SerializeField] private ElementColor[] elementColors;  // 속성 - 색깔을 리스트로 editor에 표시

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
        if (index < 0 || index >= slotImages.Length) return;        // 슬롯 이미지 최대 길이를 넘어가면 리턴

        Color baseCol = elementColorMap.TryGetValue(type, out var color) ? color : Color.white;  // 타입에 지정된 색상이 없으면, 흰색으로 설정 

        baseCol.a = 1f;                     // UI 에 표시되는 색상이 투명해지지 않도록 alpha 값을 1로 고정

        slotImages[index].color = baseCol;  // 타입마다 지정된 색상을 UI 변수에 할당
    }


    /// <summary>
    /// 모든 슬롯 이미지를 기본색(<c>defaultColor</c>)으로 초기화한다.
    /// </summary>
    public void ResetAll()
    {
        foreach (Image img in slotImages) img.color = defaultColor;     // ui의 캐스팅 슬롯들을 모두 캐스팅 되지 않은 상태 (검은색)로 초기화
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
