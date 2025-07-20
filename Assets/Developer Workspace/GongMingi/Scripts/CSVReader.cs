using NUnit.Framework;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;


/// <summary>
/// * 작성자 : 공민기
/// 
/// 간단한 CSV 파일 로더
///   - Resources 폴더에 텍스트 에셋(.csv)를 두고, 파일 이름(확장자 제외)을 인자로 넘기면
///     각 행(row)을 Dictionary<string, object> 형태로 파싱하여 List로 반환한다.
///   - 숫자 형태의 셀은 int -> float 순으로 자동 형변환을 시도한다.
/// </summary>
public class CSVReader
{
    // 열(,) 분리용 정규식 => 큰 따옴표(")로 감싼 필드 안의 쉼표는 무시
    static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";

    // 행(\r\n, \n\r, \n, \r) 분리용 정규식 => 여러 운영체제 개행 패턴 지원
    static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";

    // 셀 값 앞뒤에서 제거할 문자 집합 (현재는 큰 따옴표만)
    static char[] TRIM_CHARS = { '\"' };



    /// <summary>
    /// CSV 파일을 읽어 Dictionary 리스트로 반환 
    /// </summary>
    /// <param name="file"> Resources 폴더 기준 경로(확장자 제외) </param>
    /// <returns>
    ///  - List<Dictionary<string, object>>
    ///  - 각 Dictionary 가 한 행(row), key는 헤더 이름, value는 셀 값
    /// </returns>
    public static List<Dictionary<string, object>> Read(string file)
    {

        var list = new List<Dictionary<string, object>>();

        TextAsset data = Resources.Load(file) as TextAsset;        // Resources 에서 TextAsset 로드
        if (data == null)                                          // 데이터가 없을 시 오류 출력
        {
            Debug.LogWarning($"CSVReader: '{file}' 파일을 찾을 수 없습니다 (Resources).");
            return list;
        }


        var lines = Regex.Split(data.text, LINE_SPLIT_RE);          // 전체 텍스트를 행 단위로 분리

        if (lines.Length <= 1) return list;                         // 내용이 없으면 빈 리스트를 반환

        var header = Regex.Split(lines[0], SPLIT_RE);               // 첫줄은 헤더. => 열 이름 배열

        for(var i = 1 ; i< lines.Length ; i++)                      // 실제 데이터는 1행 (두번째 행) 부터 실행
        {
            var values = Regex.Split(lines[i], SPLIT_RE);
            if (values.Length == 0 || values[0] == "") continue;    // 빈 줄인 경우 스킵

            var entry = new Dictionary<string, object>();           // 하나의 행(row)를 담을 Dictionary

            for(var j = 0; j < header.Length && j < values.Length; j++)                     // 헤더 길이와 값 길이 중 최소값까지 순회
            {
                string value = values[j];                           // 각 열의 값을 하나씩 저장

                value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");  // 앞 뒤 따옴표 제거 + 백슬레시(\) 제거
                                                                    // replace: 문자열 내부에서 \를 지우겠다는 뜻                                                    

                object finalvalue = value;                          // 최종 값에 값을 저장한다
                int n;  
                float f;
                if(int.TryParse(value, out n))                      // 해당 값을 정수로 바꿀 수 있다면 
                {
                    finalvalue = n;                                 // 정수로 바꾼 값을 저장한다.
                }
                else if (float.TryParse(value, out f))              // 해당 값을 실수로 바꿀 수 있다면
                {
                    finalvalue = f;                                 // 실수로 바꾼 값을 저장한다.
                }
                entry[header[j]] = finalvalue;                      // 결정된 값을 행에 저장한다
            }
            list.Add(entry);                                        // 만들어진 하나의 행에 대한 DICTIONARY데이터를 딕셔너리 리스트에 옮겨 담는다.
        }

        return list;                                                // 파싱을 완료한 DICTIONARY List를 반환한다.

    }

}
