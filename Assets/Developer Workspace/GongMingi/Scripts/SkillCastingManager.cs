using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;


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

    public static SkillCastingManager Instance => instance; // get 프로퍼티

    // 인스펙터에서 ScriptableObject 목록을 직접 넣어두는 공간
    public List<SkillHashTable> allSkilltable = null;

    // 런타임에 빠른 조회를 위해 변환해 두는 딕셔너리 ( 해시값 => 해시 테이블)
    protected Dictionary<int, SkillHashTable> allSkillTableDict = new Dictionary<int, SkillHashTable>();

    bool isInit = false; // 중복 초기화 방지

    #endregion


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
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
        if (playerCast.Count <= 0)                      // 캐스팅이 비어있으면 0을 반환해 에러 표시
            return 0;

        string tempString = "";

        foreach(var item in playerCast)
        {
            tempString += item.ToString() + "|";        // 불|물|...| 의 형태로 변환시킨다
        }

        return tempString.GetHashCode();
    }



    /// <summary>
    /// 플레이가 입력한 캐스팅을 받아서 GetCastTypeHashTable을 통해 해시값으로 변환,
    /// 스킬의 해시 코드가 들어있는 딕셔너리에서 검색해서 등록된 스킬인지 찾는다
    /// 등록되어 있으면 해시 값을 반환
    /// </summary>
    /// <param name="playerCast">플레이어가 입력한 원소 캐스팅 목록</param>
    /// <returns></returns>
    public BaseSkill GetSkill( List<E_CastingType> playerCast)
    {
        int hashValue = GetCastTypeHashTable(playerCast); 

        if( hashValue == 0)
        {
            Debug.LogError("값 이상함 확인 요망");
        }


        if(allSkillTableDict.ContainsKey(hashValue))
        {
            return allSkillTableDict[hashValue].castSkill;
        }


        return null;
    }



    /// <summary>
    /// 인스펙터에 등록된 <see cref="SkillHashTable"/> 리스트를
    /// 런타임 조회용 딕셔너리(<c>allSkillTableDict</c>)로 변환해 캐싱한다.
    /// - 중복 호출 방지를 위해 <c>isInit</c> 플래그 사용.
    /// </summary>
    public void Init()
    {
        if (isInit) return;     // 중복 초기화 방지


        isInit = true;

        allSkillTableDict.Clear();
        foreach(var item in allSkilltable)
        {
            allSkillTableDict.Add(item.currentHashID, item);
        }
    }
}
