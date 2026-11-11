using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public class TerrainDataLoader : MonoBehaviour
{
    [SerializeField] private string _terrainTsvUrl;

    // 데이터 열의 개수(ID, 지형 이름, 이동 가능 여부, 소모 이동값)
    private const int COLUMN_COUNT = 4;
    
    [SerializeField] private List<TerrainData> _terrains = new();
    private Dictionary<int, TerrainData> _terrainDataById = new();

    /// <summary>
    /// Dictionary인 _terrainDataByID에 등록
    /// </summary>
    private void Awake()
    {
        foreach (var terrain in _terrains)
        {
            if (terrain == null) 
            {
                continue;
            }

            if (_terrainDataById.ContainsKey(terrain.ID))
            {
                Debug.Log("지형 ID 중복");
                continue;
            }
            
            _terrainDataById.Add(terrain.ID, terrain);
        }
    }
    
    public IEnumerator LoadTerrainTable()
    {
        if (_terrainTsvUrl == null || _terrainTsvUrl.Trim() == "") 
        {
            Debug.Log("지형 URL이 비어있다");
            yield break;
        }
        
        using (UnityWebRequest unityWebRequest = UnityWebRequest.Get(_terrainTsvUrl))
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
           !int.TryParse(values[3], out int requireMove))
        {
            Debug.Log("숫자 변환에 실패했다");
            return null;
        }

        // 이름을 붙여 JObject 생성
        return new JObject
        {
            ["id"] = id,
            ["tileName"] = values[1],
            ["isMoveAvailable"] = values[2],
            ["requireMove"] = requireMove
        };
    }

    // JSON 배열을 읽어 ID가 일치하는 TerrainData의 SO에 적용
    private void ApplyToJson(string value)
    {
        JArray jsonArray =JArray.Parse(value);

        foreach (JToken jsonObject in jsonArray)
        {
            int id = jsonObject.Value<int>("id");

            if (_terrainDataById.TryGetValue(id, out TerrainData tile))
            {
                Debug.Log("TerrainData 적용 성공");
                tile.SetTerrainData
                (
                    id,
                    jsonObject.Value<string>("tileName"),
                    jsonObject.Value<bool>("isMoveAvailable"),
                    jsonObject.Value<int>("requireMove")
                );
            }
            else
            {
                Debug.Log("TerrainData 값을 가져오지 못했다");
            }
        }
    }

    /// <summary>
    /// 지형 로더에 해당 ID의 SO를 가져오기 위한 함수
    /// </summary>
    public bool TryGetTerrainData(int id, out TerrainData terrainData)
    {
        if (_terrainDataById.TryGetValue(id, out terrainData))
        {
            return true;
        }
        else
        {
            Debug.Log($"{id}번의 지형 데이터를 갖고 오지 못했다");
            return false;
        }
    }
}
