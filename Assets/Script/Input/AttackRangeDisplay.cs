using UnityEngine;
using System.Collections.Generic;

public class AttackRangeDisplay : MonoBehaviour
{
    [SerializeField] private AttackRangeHighlight _attackRangePrefab;
    [SerializeField] private float _prefabHeight = 0.02f;
    private List<AttackRangeHighlight> _attackRangeHighlights = new();

    /// <summary>
    /// 필요없는 표시 하이라이트 숨기기
    /// </summary>
    public void ClearHighlights()
    {
        foreach (AttackRangeHighlight moveRangeHighlight in _attackRangeHighlights)
        {
            moveRangeHighlight.HideAttackRange();
        }
    }

    private void DuplicateHighlight()
    { 
        AttackRangeHighlight cloneHighlight = Instantiate(_attackRangePrefab, transform);
        cloneHighlight.HideAttackRange();
        _attackRangeHighlights.Add(cloneHighlight);
    }

    /// <summary>
    /// 튜플 기법을 통해 필요한 표시 하이라이트만큼 표시
    /// </summary>
    public void ShowAttackRangeHighlight(List<Vector3> position)
    {
        ClearHighlights();

        while (_attackRangeHighlights.Count < position.Count)
        {
            DuplicateHighlight();
        }

        for (int i = 0; i < position.Count; i++)
        {
            _attackRangeHighlights[i].ShowAttackRange(position[i] + 
                                                      (Vector3.up * _prefabHeight));
        }
    }
}
