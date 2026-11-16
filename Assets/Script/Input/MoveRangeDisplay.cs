using UnityEngine;
using System.Collections.Generic;

public class MoveRangeDisplay : MonoBehaviour
{
    [SerializeField] private MoveRangeHighlight _mobilityPrefab;
    [SerializeField] private float _prefabHeight = 0.02f;
    private List<MoveRangeHighlight> _moveRangeHighlights = new();
}
