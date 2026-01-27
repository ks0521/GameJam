using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

enum Turn
{
    Player_Aim,
    Physics_Simulating,
    Resolve_PlayerHit,
    Wait_Settling,
    End_Turn,
    Enemy_Turn,
    GameClear,
    GameOver
}
public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;
    [SerializeField] private Rigidbody2D playerRb;
    // 살아있는 몬스터 RB를 관리(스폰/사망 시 갱신)
    private readonly List<Rigidbody2D> aliveMonsterRbs = new();

    [SerializeField] PlayerFlickerMove player;
    [SerializeField] PlayerSkillManager skillManager;
    [SerializeField] DiceManager diceManager;
    [SerializeField] Turn nowTurn;

    [SerializeField] private float maxSettleTime = 5f;

    [SerializeField] GameObject gameClear;
    [SerializeField] GameObject gameOver;
    private float settleTimer;

    bool hadHit;
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        nowTurn = Turn.Player_Aim;
    }
    private void OnEnable()
    {
        if (player != null)
        {
            player.onPlayerShooted += PlayerShot;
            player.onPlayerStopped += SimulationEnd;
        }
        if (diceManager != null)
            diceManager.OnDiceResult += DiceRolled;
    }
    private void OnDisable()
    {
        if (player != null)
        {
            player.onPlayerShooted -= PlayerShot;
            player.onPlayerStopped -= SimulationEnd;
        }
        if (diceManager != null)
            diceManager.OnDiceResult -= DiceRolled;
    }
    private void Update()
    {
        switch (nowTurn)
        {
            case Turn.Wait_Settling:
                UpdateWaitSettling();
                break;
            default:
                break;

        }
    }
    #region Player_Aim 상태
    void EnterPlayerAim()
    {
        nowTurn = Turn.Player_Aim;
        player.SetAttackEnabled(true);
        player.SetInputEnabled(true);
    }
    //플레이어 발사시 FSM처리
    void PlayerShot()
    {
        Debug.Log("플레이어 발사 상태");
        nowTurn = Turn.Physics_Simulating;
        player.SetInputEnabled(false);
    }
    #endregion

    #region Physics_Simulating 상태
    //발사된 플레이어가 정지 시 몬스터를 맞췄는지, 못맞췄는지 확인
    void SimulationEnd(bool hadHit)
    {
        if (hadHit)
        {
            nowTurn = Turn.Resolve_PlayerHit;
            player.SetAttackEnabled(false);
            //장착한 스킬 반영해서 주사위굴리기
            skillManager.UseSkill();
        }
        else
        {
            EnterEnemyTurn();
        }
    }
    //주사위 굴린 결과를 적용
    #endregion

    #region Resolve_PlayerHit 상태
    void DiceRolled(int value)
    {
        nowTurn = Turn.Resolve_PlayerHit;
        player.ApplyDiceResult(value);
        EnterWaitSettling();
    }
    #endregion

    #region Wait_Settle 상태
    void EnterWaitSettling()
    {
        nowTurn = Turn.Wait_Settling;
        settleTimer = 0f;
    }
    void UpdateWaitSettling()
    {
        settleTimer += Time.deltaTime;

        // 전부 sleeping이면 몬스터 턴으로
        if (AreAllBodiesSleeping())
        {
            EnterEnemyTurn();
            return;
        }

        // 타임아웃 → 강제 종료
        if (settleTimer >= maxSettleTime)
        {
            ForceStopAll();
            EnterEnemyTurn();
        }
    }
    bool AreAllBodiesSleeping()
    {
        // 플레이어
        if (playerRb != null && !playerRb.IsSleeping())
            return false;

        // 몬스터
        for (int i = aliveMonsterRbs.Count - 1; i >= 0; i--)
        {
            var rb = aliveMonsterRbs[i];
            if (rb == null)
            {
                aliveMonsterRbs.RemoveAt(i);
                continue;
            }

            if (!rb.IsSleeping())
                return false;
        }

        return true;
    }
    void ForceStopAll()
    {
        ForceStop(playerRb);

        foreach (var rb in aliveMonsterRbs)
            ForceStop(rb);
    }
    void ForceStop(Rigidbody2D rb)
    {
        if (rb == null) return;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0;
        rb.Sleep();
    }
    #endregion

    #region Enemy_Turn 상태
    void EnterEnemyTurn()
    {
        nowTurn = Turn.Enemy_Turn;

        int aliveCount = aliveMonsterRbs.Count;
        int damagePerMonster = 3; // 조절 가능
        int totalDamage = aliveCount * damagePerMonster;
        Debug.Log($"몬스터 {aliveCount} 마리 살아있음. 플레이어 {totalDamage} 피해");
        player.Damaged(totalDamage); // 네 구조에 맞게 연결

        // 게임오버 체크
        if (player.IsDead)
        {
            GameOver();
            return;
        }
        if(aliveCount == 0)
        {
            Debug.Log("게임 클리어!");
            GameClear();
        }
        // 다음 플레이어 턴
        EnterPlayerAim();
        player.Mp += 1; //플레이어 mp 재생
    }
    #endregion
    void GameOver()
    {
        gameOver.SetActive(true);
        StartCoroutine(SceneChange());
    }
    void GameClear()
    {
        gameClear.SetActive(true);
        StartCoroutine(SceneChange());

    }
    IEnumerator SceneChange()
    {
        yield return new WaitForSeconds(3);
        GameManager.Instance.LoadScene(0);
    }
    public void RegisterMonsterRb(Rigidbody2D rb)
    {
        if (rb == null) return;
        if (!aliveMonsterRbs.Contains(rb))
            aliveMonsterRbs.Add(rb);
    }

    public void UnregisterMonsterRb(Rigidbody2D rb)
    {
        if (rb == null) return;
        aliveMonsterRbs.Remove(rb);
    }
}
