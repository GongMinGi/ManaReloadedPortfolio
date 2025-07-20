using UnityEngine;


/// <summary>
/// * 작성자 : 공민기
/// 
///  - 아이템 여러개를 한 꺼번에 보관할 컨테이너로 사용
///  - 인스펙터에서 검색/ 어드레서블관리?(검색해볼거) 용이
///  - 런타임에서 해당 클래스 한 개만 로드하면 끝.
/// </summary>
[CreateAssetMenu(menuName = "Game/Sample Item Database", fileName ="SampleItemDatabase")]
public class ParsingSampleDataBase : ScriptableObject
{
    public ParsingSampleData[] sampleItems;
}
