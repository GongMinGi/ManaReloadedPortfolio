using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 개발자: 공민기
/// 
/// 싱글톤으로 존재하며, 플레이어가 입력한 원소 조합(List<E_CastingType>;)을
/// 문자열 → 해시값으로 변환해 빠르게 조회할 수 있도록 관리하는 중앙 매니저입니다.
/// - 인스펙터에 등록된 SkillHashTable 목록을 딕셔너리(해시 → 테이블)로 변환해 캐싱하고
/// - GetCastTypeHashTable()로 조합을 유일 해시로 변환한 뒤 GetSkill()을 통해 대응 스킬을 반환합니다.
/// 
/// </summary>
public class SkillCastingManager : MonoBehaviour
{
    #region Field and Property
    // 싱글톤
    private static SkillCastingManager instance;

    // 인스펙터에서 ScriptableObject 목록을 직접 넣어두는 공간
    public List<SkillHashTable> allSkilltable = null;

    // 런타임에 빠른 조회를 위해 변환해 두는 딕셔너리 ( 해시값 => 해시 테이블)
    protected Dictionary<int, SkillHashTable> allSkillTableDict = new Dictionary<int, SkillHashTable>();

    [SerializeField] List<int> skillIDList = new();     // 각 스킬 ID 담는 리스트
    [SerializeField] List<SkillUIInfo> skillUIInfoList = new();     // 각 스킬의 UI 정보를 담는 리스트
    private Dictionary<int, SkillUIInfo> skillUIMap = new ();   // 각 스킬 ID와 UI정보를 매핑한 딕셔너리

    /// <summary>
    /// 개발자: 이예린
    /// 
    /// 주어진 스킬 ID에 해당하는 스킬 UI 정보를 반환하는 메서드
    /// SkillUIInfo에는 스킬 아이콘 이미지와 쿨타임 텍스트가 포함되어 있음
    /// </summary>
    /// <param name="id">조회할 스킬의 고유 ID</param>
    /// <returns>해당 스킬의 SkillUIInfo 객체</returns>
    public SkillUIInfo GetSkillIcon(int id) => skillUIMap[id];

    bool isInit = false; // 중복 초기화 방지

    #endregion

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            GameModeManager.SkillCastingManager = instance;
        }

        else
        {
            Destroy(this.gameObject);
        }

        Init(); // 게임시작 후 초기화 
    }

    /// <summary>
    /// 플레이어가 입력한 캐스팅을 => 문자열 => 해시 코드 순서로 변환한다.
    /// 캐스팅 타입 리스트 (ex: 불, 물, 불..)를 문자열화 => HashCode로 변환
    /// 동일 조합, 동일 순서라면, 같은 문자열이 나오므로 같은 해시가 나온다.
    /// </summary>
    public static int GetCastTypeHashTable(List<E_CastingType> playerCast)
    {
        if (playerCast.Count <= 0)                      // 입력한 원소 조합이 비어있으면 0을 반환해 에러 표시
        {
            return 0;
        }

        string tempString = "";                         // 입력으로 들어온 원소 데이터를 문자로 바꿔 저장할 변수

        foreach(var item in playerCast)
        {
            tempString += item.ToString() + "|";        // 불|물|...| 의 형태의 문자열로 변환시킨다
        }

        return tempString.GetHashCode();                // 변환한 문자열에 대한 고유한 번호를 만들어 해당 번호를 반환한다.
    }

    /// <summary>
    /// 플레이가 입력한 캐스팅을 받아서 GetCastTypeHashTable을 통해 해시값으로 변환,
    /// 스킬의 해시 코드가 들어있는 딕셔너리에서 검색해서 등록된 스킬인지 찾는다
    /// 등록되어 있으면 해시 값을 반환
    /// </summary>
    /// <param name="playerCast">플레이어가 입력한 원소 캐스팅 목록</param>
    /// <returns></returns>
    public BaseCombinationMagic GetSkill( List<E_CastingType> playerCast)
    {
        int hashValue = GetCastTypeHashTable(playerCast);       // 플레이어가 입력한 원소 조합을 해당 조합에 대응되는 고유값으로 반환한다.

        if( hashValue == 0)                                     // 원소 조합이 비어있는 경우 오류를 표시한다.
        {
            Debug.LogError("값 이상함 확인 요망");
        }

        if( allSkillTableDict.ContainsKey(hashValue) )            // 플레이어가 입력한 원소 조합에 해당하는 고유번호가 조합 마법 리스트에 존재하는지 판단
        {
            return allSkillTableDict[hashValue].castSkill;      // 마법이 존재한다면, 해당 조합에 할당된 스킬을 실행한다.
        }

        return null;
    }

    /// <summary>
    /// 인스펙터에 등록된 <see cref="SkillHashTable"/> 리스트를
    /// 런타임 조회용 딕셔너리(<c>allSkillTableDict</c>)로 변환해 캐싱한다.
    /// 
    /// 각 스킬 ID와 UI정보를 매핑한다.
    /// - 중복 호출 방지를 위해 <c>isInit</c> 플래그 사용.
    /// </summary>
    public void Init()
    {
        if (isInit)
        {
            return;     // 중복 초기화 방지
        }

        isInit = true;
        allSkillTableDict.Clear();                          // 게임 실행 중에 원소조합을 저장해놓을 변수를 초기화 시킨다

        foreach(var item in allSkilltable)
        {
            allSkillTableDict.Add(item.currentHashID, item);    // 에디터에서 입력해놓은 조합 마법의 고유값과 대응되는 스킬을 게임 실행 중에 사용할 변수로 복사해 온다.
            item.castSkill.SetCanUseSkill(true);
        }

        for (int i = 0; i < skillIDList.Count; i++)
        {
            skillUIMap.Add(skillIDList[i], skillUIInfoList[i]);   // 각 스킬 ID와 UI정보를 매핑
        }
    }
}

/// <summary>
/// 각 스킬의 UI(쿨타임) 정보를 담는 구조체
/// </summary>
[System.Serializable]
public struct SkillUIInfo
{
    [SerializeField] private Image coolTimeUI;
    [SerializeField] private TMP_Text coolTimeText;

    public Image CoolTimeUI => coolTimeUI;
    public TMP_Text CoolTimeText => coolTimeText;
}
