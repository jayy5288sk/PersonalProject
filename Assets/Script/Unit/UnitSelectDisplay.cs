using UnityEngine;

public class UnitSelectDisplay : MonoBehaviour
{
    [SerializeField] private GameObject _groundQuadPrefab;
    [SerializeField] private GameObject _headMarkerPrefab;
    [SerializeField] private MapGenerator _mapGenerator;
    [SerializeField] private float _groundQuadHeight = 0.04f;
    [SerializeField] private float _headMarkerHeight = 1.5f;
    
    private GameObject _groundQuad;
    private GameObject _headMarker;

    private void Awake() => Init();

    /// <summary>
    /// 현재 선택된 유닛임을 알려주는 인디케이터 시각화
    /// </summary>
    public void ShowUnitIndicators(MapUnit unit)
    {
        Vector3 groundPosition = _mapGenerator.ReturnTerrainPosition(unit.SectionUnitPos);
        Vector3 unitPosition = unit.transform.position;
        
        _groundQuad.transform.position = groundPosition + (Vector3.up * _groundQuadHeight);
        _headMarker.transform.position = unitPosition + (Vector3.up * _headMarkerHeight);
        
        _groundQuad.SetActive(true);
        _headMarker.SetActive(true);
    }

    /// <summary>
    /// 인디케이터 해제
    /// </summary>
    public void HideUnitIndicators()
    {
        _groundQuad.SetActive(false);
        _headMarker.SetActive(false);
    }

    private void Init()
    {
        _groundQuad = Instantiate(_groundQuadPrefab,
                                  _groundQuadPrefab.transform.position,
                                  _groundQuadPrefab.transform.rotation);
        _headMarker = Instantiate(_headMarkerPrefab,
                                  _headMarkerPrefab.transform.position,
                                  _headMarkerPrefab.transform.rotation);
        
        _groundQuad.SetActive(false);
        _headMarker.SetActive(false);
    }
}
