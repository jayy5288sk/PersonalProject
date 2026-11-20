using UnityEngine;

[CreateAssetMenu(fileName = "NewStageData", menuName = "Scriptable Object/Stage/Stage Data")]
public class StageData : ScriptableObject
{
    [SerializeField] private string _stageName;
    [SerializeField] private string _mapTsvUrl;
    [SerializeField] private string _unitSpawnTsvUrl;

    public string StageName => _stageName;
    public string MapTsvUrl => _mapTsvUrl;
    public string UnitSpawnTsvUrl => _unitSpawnTsvUrl;
}
