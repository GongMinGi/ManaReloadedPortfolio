using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// * 작성자 공민기
///  - 특정 CSV파일을 파싱한 데이터를 가지고 스크립터블 오브젝트를 작성하는 툴
///  - CSV 파일이 추가될 때마다 해당 데이터 스크립트와 header의 매핑을 수정한 복제본을 만들어주어야 한다.
/// </summary>
public class CsvToSO : EditorWindow
{
    private TextAsset csvFile;                                                            // 변환할 csv 파일 (에디터에서 드래그&드롭으로 할당)
    private string outputFolder = "Assets/Developer Workspace/GongMingi/Scriptable";      // SO를 생성서알 저장 경로

    [MenuItem("Tools/sampleCSV => sample Scriptable")]                                    // 메뉴바에 항목추가 : Tools/ sampleSCV => samplescriptable
    private static void ShowWindow() => GetWindow<CsvToSO>(true);                         // 창 열기????


    private void OnGUI()
    {
        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", csvFile, typeof(TextAsset), false);    // csv 파일 필드 표시
        outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);          // SO출력(생성) 폴더 입력란 

        GUI.enabled = csvFile != null;                                                    // csv 파일이 선택되었을 때만 생성 버튼 활성화
        if (GUILayout.Button("Convert & Save"))
        {
            Import(csvFile, outputFolder);                                                // csv = SO 변환 실행
        }
        GUI.enabled = true;                                                               // GUI 상태 원복???????
    }


    /// <summary>
    /// csv의 각 행과 SO 의 필드 값을 매칭 시켜주는 매서드
    ///  - csv 파일의 헤더가 수정되거나 파일이 추가되엇을대 이 부분을 수정해야 함
    /// </summary>
    /// <param name="data"> SO가 정의된 Scriptable Object 변수 </param>
    /// <param name="row"> csv를 파싱하여 갖고 있는 데이터 </param>
    private static void FillFromRow(ParsingSampleData data, Dictionary<string, object> row)
    {
        // csv값 => 필드 매핑
        data.id = int.Parse(row["id"].ToString());
        data.type = row["Type"].ToString();
        data.name = row["Name"].ToString();
        data.discription = row["Discription"].ToString();
        data.attack = int.Parse(row["Attack"].ToString());
        data.deffense = int.Parse(row["deffense"].ToString());
        data.magic = int.Parse(row["magic"].ToString());

    }


    private void Import(TextAsset csv, string folder)
    {
        List<Dictionary<string, object>> rows = CSVReader.Read(csv.name);   // csv 파싱 ( 확장자 없는 경로 )

        if(rows.Count == 0)                                                 // csv 파싱 데이터가 비어 있으면 경고 출력 후 종료
        {
            Debug.LogWarning("csv에 데이터가 없습니다");
            return;
        }

        //출력 폴더 유효성 검사
        if (!AssetDatabase.IsValidFolder(folder))                           // 경로로 지정된 폴더가 유효하지 않은경우
        {
            AssetDatabase.CreateFolder("Assets", folder.Replace("Assets", ""));  // Assets 경로 하위에 폴더를 새로 만든다, Asset의 하위 경로만 전달해야하므로 "Assets"는 Replace를 통해 지워준다
        }

        // ParsingSampleData 생성 or 업데이트
        List<ParsingSampleData> created = new();                            // 변환된 SO를 리스트이 형태로 갖고 있는 변수.


        foreach(var row in rows)                                            // CSV의 각행(각 데이터) 반복 => SO 생성/업데이트
        {
            int id = (int)row["id"];                                        // 각 행의 id
            string assetPath = $"{folder}/SampleItem_{id}.asset";           //생성한 스크립터블 오브젝트를 저장할 경로

            // 기존 asset이 있으면 불러오고, 없으면 새로 만든다.
            ParsingSampleData data = AssetDatabase.LoadAssetAtPath<ParsingSampleData>(assetPath);
            if(data == null)
            {
                data = ScriptableObject.CreateInstance<ParsingSampleData>();

                FillFromRow(data, row);     // 모든 필드 채우기

                AssetDatabase.CreateAsset(data, assetPath);
            }
            else
            {
                FillFromRow(data, row);     // 모든 필드 채우기
            }




            EditorUtility.SetDirty(data);
            created.Add(data);
        }


        string dbPath = $"{folder}/ItemDatabase.asset";
        ParsingSampleDataBase db = AssetDatabase.LoadAssetAtPath<ParsingSampleDataBase>(dbPath);

        if (db == null)
        {
            db = ScriptableObject.CreateInstance<ParsingSampleDataBase>();
            AssetDatabase.CreateAsset(db, dbPath);
        }

        db.sampleItems = created.ToArray();
        EditorUtility.SetDirty(db);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"csv 변환 완료: {created.Count}개 Itemdata, database 저장");

    }
}
