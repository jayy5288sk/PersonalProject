using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public class WeaponDataLoader : MonoBehaviour
{
    [SerializeField] private string _weaponTsvUrl;

    // 데이터 열의 개수(ID, 무기 이름, 공격력, 최소 사거리, 최대 사거리, 범위 모양, 필요 EN)
    private const int COLUMN_COUNT = 7;
    
    [SerializeField] private List<WeaponData> _weapons = new();
    private Dictionary<int, WeaponData> _weaponDataById = new();

    /// <summary>
    /// Dictionary인 _weaponDataByID에 등록
    /// </summary>
    private void Awake()
    {
        foreach (var weapon in _weapons)
        {
            if (weapon == null) 
            {
                continue;
            }

            if (_weaponDataById.ContainsKey(weapon.ID))
            {
                Debug.Log("무기 ID 중복");
                continue;
            }
            
            _weaponDataById.Add(weapon.ID, weapon);
        }
    }
    
    public IEnumerator LoadWeaponTable()
    {
        if (_weaponTsvUrl == null || _weaponTsvUrl.Trim() == "") 
        {
            Debug.Log("무기 URL이 비어있다");
            yield break;
        }
        
        using (UnityWebRequest unityWebRequest = UnityWebRequest.Get(_weaponTsvUrl))
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
           !int.TryParse(values[2], out int attackDamage) ||
           !int.TryParse(values[3], out int minRange) ||
           !int.TryParse(values[4], out int maxRange)||
           !int.TryParse(values[6], out int rangeShape))
        {
            Debug.Log("숫자 변환에 실패했다");
            return null;
        }

        // 이름을 붙여 JObject 생성
        return new JObject
        {
            ["id"] = id,
            ["weaponName"] = values[1],
            ["attackDamage"] = attackDamage,
            ["minRange"] = minRange,
            ["maxRange"] = maxRange,
            ["rangeShape"] = values[5],
            ["requireEn"] = rangeShape
        };
    }

    // JSON 배열을 읽어 ID가 일치하는 WeaponData의 SO에 적용
    private void ApplyToJson(string value)
    {
        JArray jsonArray =JArray.Parse(value);

        foreach (JToken jsonObject in jsonArray)
        {
            int id = jsonObject.Value<int>("id");

            if (_weaponDataById.TryGetValue(id, out WeaponData weapon))
            {
                Debug.Log("WeaponData 적용 성공");
                weapon.SetData
                (
                    id,
                    jsonObject.Value<string>("weaponName"),
                    jsonObject.Value<int>("attackDamage"),
                    jsonObject.Value<int>("minRange"),
                    jsonObject.Value<int>("maxRange"),
                    jsonObject.Value<string>("rangeShape"),
                    jsonObject.Value<int>("requireEn")
                );
            }
            
            else
            {
                Debug.Log("WeaponData 값을 가져오지 못했다");
            }
        }
    }
}
