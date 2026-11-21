using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 행동을 마친 유닛을 어둡게 표시하고, 다시 원래 색으로 되돌린다.
/// 메크처럼 Renderer가 여러 자식에 나뉜 유닛도 전체에 적용된다.
/// MaterialPropertyBlock을 사용하므로 머티리얼 사본을 만들지 않는다.
/// </summary>
public class UnitActionTint : MonoBehaviour
{
    private static readonly int BASE_COLOR_ID = Shader.PropertyToID("_BaseColor");

    [Tooltip("행동 완료 시 원래 색에 곱할 밝기 (0 = 검정, 1 = 원래 색)")]
    [SerializeField, Range(0f, 1f)] private float _actionDoneBrightness = 0.4f;

    private Renderer[] _bodyRenderers;
    private MaterialPropertyBlock _propertyBlock;

    private void Awake()
    {
        CacheRenderers();
    }

    // 몸체 Renderer만 한 번 모아 둔다. VFX(파티클, 잔상)는 어둡게 하지 않도록 제외한다.
    private void CacheRenderers()
    {
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>(true);
        List<Renderer> bodyRenderers = new(allRenderers.Length);

        foreach (Renderer childRenderer in allRenderers)
        {
            if (childRenderer is ParticleSystemRenderer || childRenderer is TrailRenderer)
            {
                continue;
            }

            bodyRenderers.Add(childRenderer);
        }

        _bodyRenderers = bodyRenderers.ToArray();
        _propertyBlock = new MaterialPropertyBlock();
    }

    // true면 어둡게, false면 원래 색으로 되돌린다.
    public void SetActionDone(bool isActionDone)
    {
        foreach (Renderer bodyRenderer in _bodyRenderers)
        {
            Material[] materials = bodyRenderer.sharedMaterials;

            for (int i = 0; i < materials.Length; i++)
            {
                _propertyBlock.Clear();

                if (isActionDone)
                {
                    // 머티리얼마다 원래 색이 다르므로 각자의 색에 밝기를 곱한다.
                    Color originalColor = materials[i].GetColor(BASE_COLOR_ID);
                    Color darkColor = originalColor * _actionDoneBrightness;
                    darkColor.a = originalColor.a;
                    _propertyBlock.SetColor(BASE_COLOR_ID, darkColor);
                }

                // 비어 있는 블록을 넣으면 덮어쓴 값이 지워져 원래 색으로 돌아간다.
                bodyRenderer.SetPropertyBlock(_propertyBlock, i);
            }
        }
    }
}
