using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAIController : MonoBehaviour
{
    // 접근 이동용 거리 지도는 이동력 제한 없이 계산
    private const int UNLIMITED_MOVEMENT_POINTS = int.MaxValue;

    [SerializeField] private MapGenerator _mapGenerator;
    [SerializeField] private UnitGenerator _unitGenerator;
    [SerializeField] private float _actionDelay = 0.5f; // 적 한 기 행동 전 대기 시간(초)
    [SerializeField] private int _defeatScoreBonus = 100; // 조합 점수: 대상 격파 시 추가 점수

    private readonly MovementRangeCalculator _movementRangeCalculator = new();
    private readonly AttackRangeCalculator _attackRangeCalculator = new();

    // 결과 컬렉션 재사용
    private readonly List<MapUnit> _enemyUnits = new();
    private readonly List<MapUnit> _allyUnits = new();
    private readonly List<Vector2Int> _standSections = new();
    private readonly HashSet<Vector2Int> _attackableSections = new();
    private readonly List<Vector2Int> _hitSections = new();
    private readonly List<MapUnit> _targets = new();

    private WaitForSeconds _actionWait;

    public event Action OnEnemyTurnCompleted;

    private void Awake() => Init();

    public void StartEnemyTurn()
    {
        StartCoroutine(EnemyTurnRoutine());
    }

    public void StopEnemyTurn()
    {
        StopAllCoroutines();
    }

    private IEnumerator EnemyTurnRoutine()
    {
        // 이동하면 칸 목록이 바뀌므로 목록을 먼저 복사해 두고 순회
        _unitGenerator.GetUnitsByTeam(TeamSide.ENEMY, _enemyUnits);

        foreach (MapUnit enemy in _enemyUnits)
        {
            yield return _actionWait;
            ActEnemy(enemy);
            enemy.CompleteUnitAction();
        }

        // 적이 0기여도 대기 뒤에 완료를 알린다.
        // StartCoroutine은 첫 yield까지 바로 실행되므로, 대기 없이 알리면 ChangePhase 도중 다시 전환된다
        yield return _actionWait;

        foreach (MapUnit enemy in _enemyUnits)
        {
            enemy.ResetUnitAction();
        }

        OnEnemyTurnCompleted?.Invoke();
    }

    // 공격 조합이 있으면 이동 후 공격. 없으면 돌격형만 가까운 아군 쪽으로 이동, 거점형은 제자리 대기
    private void ActEnemy(MapUnit enemy)
    {
        Dictionary<Vector2Int, int> reachableSections = _movementRangeCalculator.GetReachableSections
        (
            enemy.SectionUnitPos,
            enemy.SelectUnitData.MovementPoints,
            enemy.SelectUnitData.TeamSide,
            _mapGenerator.TerrainBySection,
            _unitGenerator.UnitsBySection
        );

        if (TryFindBestAttack(enemy, reachableSections,
                              out Vector2Int bestStand, out WeaponData bestWeapon, out Vector2Int bestClicked))
        {
            AttackFrom(enemy, bestStand, bestWeapon, bestClicked);
            return;
        }

        if (enemy.BehaviorType == EnemyBehaviorType.Assault)
        {
            MoveTowardNearestAlly(enemy, reachableSections);
        }
    }

    // (서는 칸 × 무장 × 클릭 칸) 조합을 모두 점수화해 가장 높은 조합을 찾음. 점수 0 이하는 조합 없음
    private bool TryFindBestAttack(MapUnit enemy, Dictionary<Vector2Int, int> reachableSections,
                                   out Vector2Int bestStand, out WeaponData bestWeapon, out Vector2Int bestClicked)
    {
        bestStand = enemy.SectionUnitPos;
        bestWeapon = null;
        bestClicked = default;
        int bestScore = 0;

        // 제자리를 먼저 넣어, 같은 점수면 이동하지 않는 쪽을 고름
        _standSections.Clear();
        _standSections.Add(enemy.SectionUnitPos);
        _standSections.AddRange(reachableSections.Keys);

        foreach (Vector2Int stand in _standSections)
        {
            foreach (WeaponData weapon in enemy.WeaponDataList)
            {
                if (!enemy.CanUseWeapon(weapon))
                {
                    continue;
                }

                _attackRangeCalculator.GetAttackableSections(stand, weapon, _mapGenerator.IsInsideMap,
                                                             _attackableSections);

                foreach (Vector2Int clicked in _attackableSections)
                {
                    // 상대 진영 유닛이 있는 칸만 클릭 후보
                    if (!_unitGenerator.TryGetUnitAtSection(clicked, out MapUnit unit) ||
                        unit.SelectUnitData.TeamSide == enemy.SelectUnitData.TeamSide)
                    {
                        continue;
                    }

                    _attackRangeCalculator.ApplyAttackSection(stand, weapon, clicked,
                                                              _attackableSections, _hitSections);
                    AttackExecutor.CollectTargets(enemy, _hitSections, _unitGenerator, _targets);

                    int score = CalculateAttackScore(enemy, weapon, _targets);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestStand = stand;
                        bestWeapon = weapon;
                        bestClicked = clicked;
                    }
                }
            }
        }

        return bestWeapon != null;
    }

    // 점수 = Σ 실제로 깎을 수 있는 HP + 격파 수 × 격파 보너스
    private int CalculateAttackScore(MapUnit enemy, WeaponData weapon, List<MapUnit> targets)
    {
        int score = 0;
        foreach (MapUnit target in targets)
        {
            int damage = BattleCalculator.CalculateDamage(enemy, weapon, target);
            score += Mathf.Min(damage, target.CurrentHp);

            if (damage >= target.CurrentHp)
            {
                score += _defeatScoreBonus;
            }
        }

        return score;
    }

    private void AttackFrom(MapUnit enemy, Vector2Int stand, WeaponData weapon, Vector2Int clicked)
    {
        if (stand != enemy.SectionUnitPos)
        {
            _unitGenerator.MoveUnitToSection(enemy, stand);
        }

        // 탐색 중 결과 컬렉션을 덮어썼으므로, 고른 조합으로 다시 계산
        _attackRangeCalculator.GetAttackableSections(stand, weapon, _mapGenerator.IsInsideMap,
                                                     _attackableSections);
        _attackRangeCalculator.ApplyAttackSection(stand, weapon, clicked, _attackableSections, _hitSections);
        AttackExecutor.CollectTargets(enemy, _hitSections, _unitGenerator, _targets);
        AttackExecutor.Execute(enemy, weapon, _targets, _unitGenerator);
    }

    // 아군마다 "그 아군까지의 거리 지도"를 만들고, 이동 가능한 칸 중 가장 가까운 칸으로 이동
    private void MoveTowardNearestAlly(MapUnit enemy, Dictionary<Vector2Int, int> reachableSections)
    {
        _unitGenerator.GetUnitsByTeam(TeamSide.ALLY, _allyUnits);

        bool hasDestination = false;
        Vector2Int destination = default;
        int bestDistance = int.MaxValue;

        foreach (MapUnit ally in _allyUnits)
        {
            // 아군 칸에서 출발해 적 진영 규칙으로 퍼뜨린 비용 = 그 칸에서 아군까지의 거리
            Dictionary<Vector2Int, int> distanceToAlly = _movementRangeCalculator.GetReachableSections
            (
                ally.SectionUnitPos,
                UNLIMITED_MOVEMENT_POINTS,
                enemy.SelectUnitData.TeamSide,
                _mapGenerator.TerrainBySection,
                _unitGenerator.UnitsBySection
            );

            foreach (Vector2Int section in reachableSections.Keys)
            {
                if (distanceToAlly.TryGetValue(section, out int distance) && distance < bestDistance)
                {
                    bestDistance = distance;
                    destination = section;
                    hasDestination = true;
                }
            }
        }

        // 길이 막혀 어느 아군에도 닿지 않으면 제자리
        if (hasDestination)
        {
            _unitGenerator.MoveUnitToSection(enemy, destination);
        }
    }

    private void Init()
    {
        _actionWait = new WaitForSeconds(_actionDelay);
    }
}
