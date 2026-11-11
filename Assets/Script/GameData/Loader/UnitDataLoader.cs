using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public class UnitDataLoader : MonoBehaviour
{
    [SerializeField] private string _unitTsvUrl;

    // 데이터 열의 개수(id, 유닛 이름, HP, EN, 행동력, 기본 무장, 확장 무장, 진영)
    private const int COLUMN_COUNT = 8;
    
    [SerializeField] private List<UnitData> _units = new();
    private Dictionary<int, UnitData> _unitDataById = new();
    
    /// <summary>
    /// Dictionary인 _unitDataById에 등록
    /// </summary>
    private void Awake()
    {
        foreach (UnitData unit in _units)
        {
            if (unit == null)
            {
                continue;
            }

            if (_unitDataById.ContainsKey(unit.ID))
            {
                Debug.Log("유닛 ID가 중복된다");
                continue;
            }
            
            _unitDataById.Add(unit.ID, unit);
        }
    }
    
    public IEnumerator LoadUnitTable()
    {
        if (_unitTsvUrl == null || _unitTsvUrl.Trim() == "") 
        {
            Debug.Log("유닛 URL이 비어있다");
            yield break;
        }
        
        using (UnityWebRequest unityWebRequest = UnityWebRequest.Get(_unitTsvUrl))
        {
            // 요청이 완료될 때까지 대기
            yield return unityWebRequest.SendWebRequest();

            // url 다운로드를 실패했을 때
            if (unityWebRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("다운로드에 실패했다");
                yield break;
            }
            
            // 변환한 문자열 적용
            string convertJson = ConvertTsvToJson(unityWebRequest.downloadHandler.text);
            
            // SO 적용
            ApplyToJson(convertJson);
        }
    }

    private string ConvertTsvToJson(string tsvContext)
    {
        // 줄 단위 분리
        string[] lines = tsvContext.Split
        (
            new [] { '\r', '\n' },
            StringSplitOptions.RemoveEmptyEntries
        );
        
        // 데이터가 없으면 빈 JSON 배열 반환
        if (lines.Length < 2)
        {
            Debug.Log("데이터가 없다");
            return "[]";
        }

        JArray jsonArray = new JArray();
        
        // 0은 헤더(무엇인지), 1은 자료형을 나타내므로 2부터 시작
        for (int i = 2; i < lines.Length; i++)
        {
            JObject jsonObject = ParseLine(lines[i], i + 1);
            
            if (jsonObject != null)
            {
                jsonArray.Add(jsonObject);
            }
        }
        
        return jsonArray.ToString();
    }

    private JObject ParseLine(string line, int index)
    {
        // 데이터 분리
        string[] values = line.Split('\t');
        
        // 해당 자료의 열 개수 확인
        if (values.Length != COLUMN_COUNT)
        {
            Debug.Log("열 개수가 다르다");
            return null;
        }

        // 공백 제거
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = values[i].Trim();
        }
        
        // 숫자 변환
        if(!int.TryParse(values[0], out int id) ||
           !int.TryParse(values[2], out int hp) ||
           !int.TryParse(values[3], out int en) ||
           !int.TryParse(values[4], out int movePoints))
        {
            Debug.Log("숫자 변환에 실패했다");
            return null;
        }

        // 이름을 붙여 JObject 생성
        return new JObject
        {
            ["id"] = id,
            ["unitName"] = values[1],
            ["hp"] = hp,
            ["en"] = en,
            ["movePoints"] = movePoints,
            ["basicWeapon"] = values[5],
            ["expandedWeapon"] = values[6],
            ["teamSide"] = values[7]
        };
    }

    // JSON 배열을 읽어 ID가 일치하는 UnitData의 SO에 적용
    private void ApplyToJson(string value)
    {
        JArray jsonArray =JArray.Parse(value);

        foreach (JToken jsonObject in jsonArray)
        {
            int id = jsonObject.Value<int>("id");

            if (_unitDataById.TryGetValue(id, out UnitData unit))
            {
                Debug.Log("UnitData 적용 성공");
                unit.SetData
                (
                    id,
                    jsonObject.Value<string>("unitName"),
                    jsonObject.Value<int>("hp"),
                    jsonObject.Value<int>("en"),
                    jsonObject.Value<int>("movePoints"),
                    jsonObject.Value<string>("basicWeapon"),
                    jsonObject.Value<string>("expandedWeapon"),
                    jsonObject.Value<string>("teamSide")
                );
            }
            else
            {
                Debug.Log("UnitData 값을 가져오지 못했다");
            }
        }
    }
}
