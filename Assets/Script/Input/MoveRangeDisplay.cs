using UnityEngine;
using System.Collections.Generic;

public class MoveRangeDisplay : MonoBehaviour
{
    [SerializeField] private MoveRangeHighlight _mobilityPrefab;
    [SerializeField] private float _prefabHeight = 0.02f;
    private List<MoveRangeHighlight> _moveRangeHighlights = new();

    /// <summary>
    /// 필요없는 표시 하이라이트 숨기기
    /// </summary>
    public void ClearHighlights()
    {
        foreach (MoveRangeHighlight moveRangeHighlight in _moveRangeHighlights)
        {
            moveRangeHighlight.HideMoveRange();
        }
    }

    private void DuplicateHighlight()
    { 
        MoveRangeHighlight cloneHighlight = Instantiate(_mobilityPrefab, transform);
        cloneHighlight.HideMoveRange();
        _moveRangeHighlights.Add(cloneHighlight);
    }

    /// <summary>
    /// 튜플 기법을 통해 필요한 표시 하이라이트만큼 표시
    /// </summary>
    public void ShowMoveRangeHighlight(List<(Vector3 position, int cost)> highlightTargets)
    {
        ClearHighlights();

        while (_moveRangeHighlights.Count < highlightTargets.Count)
        {
            DuplicateHighlight();
        }

        for (int i = 0; i < highlightTargets.Count; i++)
        {
            Vector3 position = highlightTargets[i].position + (Vector3.up * _prefabHeight);
            int cost = highlightTargets[i].cost;
            _moveRangeHighlights[i].ShowMoveRange(position, cost);
        }
    }
}
