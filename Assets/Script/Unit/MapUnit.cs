using UnityEngine;

public class MapUnit : MonoBehaviour
{
    private const float ACTION_DONE_DARKNESS = 0.5f;
    
    [SerializeField] private UnitData _selectUnitData;
    [SerializeField] private Vector2Int _sectionUnitPos;
    
    private Renderer _renderer;
    private Color _originalColor;
    
    public UnitData SelectUnitData => _selectUnitData;
    public Vector2Int SectionUnitPos => _sectionUnitPos;
    public bool IsUnitActionDone { get; private set; }

    private void Awake() => Init();
    
    public void SetUnitData(UnitData unitData, Vector2Int sectionUnitPos)
    {
        _selectUnitData = unitData;
        _sectionUnitPos = sectionUnitPos;
    }

    public void CompleteUnitAction()
    {
        IsUnitActionDone = true;
        _renderer.material.color = new Color(_originalColor.r*ACTION_DONE_DARKNESS,
                                             _originalColor.g*ACTION_DONE_DARKNESS,
                                             _originalColor.b*ACTION_DONE_DARKNESS,
                                             _originalColor.a);
    }

    public void ResetUnitAction()
    {
        IsUnitActionDone = false;
        _renderer.material.color = _originalColor;
    }

    private void Init()
    {
        _renderer = GetComponent<Renderer>();
        _originalColor = _renderer.material.color;
    }
}
