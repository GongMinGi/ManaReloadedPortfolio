using UnityEngine;


/// <summary>
/// * 작성자 공민기
///   - csv로 파싱한 데이터를 스크립터블 오브젝트로 옮기기 위한 샘플 오브젝트
///   - 추후 기획에서 아이템 csv가 완성되면 그에 맞게 대체 / 수정 예정
/// </summary>
public class ParsingSampleData : ScriptableObject
{
    public int id;
    public string type;
    public string name;
    public string discription;
    public int attack;
    public int deffense;
    public int magic;
}
