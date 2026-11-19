using System;
using UnityEngine;
using UnityEngine.EventSystems;

// 무장 버튼에 붙여 마우스 진입·이탈을 알림. 표시는 WeaponListHUD가 담당
public class WeaponDataHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private int _weaponIndex;   // 선택 유닛의 무장 목록 순번 (버튼 순서와 동일)

    public event Action<int> OnHoverEnter;
    public event Action OnHoverExit;

    public void OnPointerEnter(PointerEventData eventData) => OnHoverEnter?.Invoke(_weaponIndex);

    public void OnPointerExit(PointerEventData eventData) => OnHoverExit?.Invoke();
}
