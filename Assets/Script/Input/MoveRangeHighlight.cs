using TMPro;
using UnityEngine;

public class MoveRangeHighlight : MonoBehaviour
{
    [SerializeField] private TextMeshPro _moveRangeText;

    public void ShowMoveRange(Vector3 position, int cost)
    {
        gameObject.SetActive(true);
        transform.position = position;
        _moveRangeText.text = cost.ToString();
    }

    public void HideMoveRange()
    {
        gameObject.SetActive(false);
    }
}
