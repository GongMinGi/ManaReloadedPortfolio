using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CsvToSO : EditorWindow
{
    private TextAsset csvFile;
    private string outputFolder = "Assets/Developer Workspace/GongMingi/Scriptable";

    [MenuItem("Tools/sampleCSV => sample Scriptable")]
    private static void ShowWindow() => GetWindow<CsvToSO>(true);


    private void OnGUI()
    {
        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", csvFile, typeof(TextAsset), false);
        outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);

        GUI.enabled = csvFile != null;
        if (GUILayout.Button("Convert & Save"))
        {
            Import(csvFile, outputFolder);
        }
        GUI.enabled = true;
    }


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

        if(rows.Count == 0)
        {
            Debug.LogWarning("csv에 데이터가 없습니다");
            return;
        }

        //출력 폴더 대비??
        if (!AssetDatabase.IsValidFolder(folder))                           // 경로로 지정된 폴더가 유효하지 않은경우
        {
            AssetDatabase.CreateFolder("Assets", folder.Replace("Assets", ""));  // Assets 경로 하위에 폴더를 새로 만든다
        }

        // ParsingSampleData 생성 or 업데이트
        List<ParsingSampleData> created = new();        // Database 생성용


        foreach(var row in rows)
        {
            int id = (int)row["id"];
            string assetPath = $"{folder}/SampleItem_{id}.asset";       //생성한 스크립터블 오브젝트를 저장할 경로

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
