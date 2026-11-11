using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public class UnitSpawnDataLoader : MonoBehaviour
{
    [SerializeField] private string _unitSpawnTsvUrl;

    private const int INVALID_UNIT_ID = -1;
    private const int EMPTY_UNIT = 0;
    
    public JArray UnitSpawnArray { get; private set; }

    public IEnumerator LoadUnitSpawnTable()
    {
        if (_unitSpawnTsvUrl == null || _unitSpawnTsvUrl.Trim() == "") 
        {
            Debug.Log("스폰 디자인 URL이 비어있다");
            yield break;
        }
        
        using (UnityWebRequest unityWebRequest = UnityWebRequest.Get(_unitSpawnTsvUrl))
        {
            // 요청이 완료될 때까지 대기
            yield return unityWebRequest.SendWebRequest();

            // url 다운로드를 실패했을 때
            if (unityWebRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("다운로드에 실패했다");
                yield break;
            }
            
            // 변환 결과 변수 적용
            JArray convertJArray = ConvertTsvToJArray(unityWebRequest.downloadHandler.text);
            
            // 최종적으로 보여줄 스폰 포인트에 적용
            UnitSpawnArray = convertJArray;
            Debug.Log($"({UnitSpawnArray.Count}x{((JArray)UnitSpawnArray[0]).Count}) 크기의 스폰 디자인 데이터 적용 성공");
        }
    }
    
    private JArray ConvertTsvToJArray(string tsvContext)
    {
        JArray spawnArray = new();
        
        // 행 단위 분리
        string[] lines = tsvContext.Split
        (
            new [] { '\r', '\n' },
            StringSplitOptions.RemoveEmptyEntries
        );
        
        // 스폰 데이터가 없으면 빈 JSON 배열 반환
        if (lines.Length < 1)
        {
            Debug.LogError("스폰 디자인 데이터가 없다");
            return spawnArray;
        }

        // 줄 당 칸 수 기준
        int standardColumnCount = ParseRow(lines[0], 1).Count;
        
        for (int i = 0; i < lines.Length; i++)
        {
            // 구글 스프레드시트는 1번부터 시작하므로 + 1 해줘야 한다
            JArray rowArray = ParseRow(lines[i], i + 1);
            
            // 현재 줄에 있는 칸 수
            int columnCount = rowArray.Count;
            
            if (columnCount != standardColumnCount)
            {
                Debug.LogError($"{i + 1}번째 행 체크 필요");
            }
            
            spawnArray.Add(rowArray);
        }
        
        return spawnArray;
    }
    
    private JArray ParseRow(string line, int rowIndex)
    {
        JArray rowArray = new();
        
        // 데이터 분리
        string[] values = line.Split('\t');
        
        // 한 행의 데이터 변환
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = values[i].Trim();

            // 빈 칸일 경우(유닛 없음)
            if (values[i] == "")
            {
                rowArray.Add(EMPTY_UNIT);
            }
            
            // 해당 ID로 변환
            else if (int.TryParse(values[i], out int id))
            {
                rowArray.Add(id);
            }
            
            else 
            {
                Debug.LogError($"{rowIndex}행 {i + 1}열 '{values[i]}'값 문제 발생");
                rowArray.Add(INVALID_UNIT_ID);
            }
        }

        // 만든 rowArray 반환
        return rowArray;
    }
}
