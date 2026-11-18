using UnityEngine;

public class AttackRangeHighlight : MonoBehaviour
{
    public void ShowAttackRange(Vector3 position)
    {
        gameObject.SetActive(true);
        transform.position = position;
    }

    public void HideAttackRange()
    {
        gameObject.SetActive(false);
    }
}
