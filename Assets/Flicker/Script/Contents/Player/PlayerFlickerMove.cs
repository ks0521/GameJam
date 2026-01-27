using System;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerFlickerMove : MonoBehaviour
{
    public float hp;
    public float Hp
    {
        get => hp;
        set
        {
            hp = value;
            onPlayerHpChange?.Invoke(hp, maxhp);
        }
    }
    public int maxhp = 100;
    public int mp;
    public int Mp
    {
        get => mp;
        set
        {
            mp = value;
            onPlayerMpChange?.Invoke(mp, maxMp);
        }
    }
    public int maxMp = 10;
    #region 세팅
    [Header("Flick")]
    public float power = 12f; //충격량
    public float maxDragDistance = 2.5f; //최대 당기는 힘(거리)
    public float stopThreshold = 0.15f; //

    [Header("Aim Line(Optional)")]
    public LineRenderer line; //조준선
    public float lineLengthMultiplier = 1.2f;

    [Header("Player BaseDamage Setting")]
    public float baseDamage = 10;
    public float baseKnockBack = 7.5f;
    public float Growth = 1.18f;
    public float wallDamage = 15;

    float damage;
    float knockBack;
    float stopTimer=0f;
    const float STOP_TIME = 0.3f;
    const float STOP_SPEED = 0.05f;
    Rigidbody2D rb;
    Camera cam;
    bool dragging;
    bool canDragging;
    public bool IsDead { get; private set; }
    Vector2 origin; //캐릭터 시작점 위치
    Vector2 current; //포인터 위치

    IHittable target;
    Vector2 knockbackDir;
    bool canAttack; //한번 공격시 비활성화, 플레이어 턴 상태가 올 시 활성화
    bool isPlayerMoving;
    bool hadHit; //플레이어가 몬스터를 타격했는지 확인

    public event Action onPlayerShooted; //플레이어 발사
    public event Action<bool> onPlayerStopped; //플레이어 멈춤(몬스터 맞췄으면 true, 아니면 false)
    public event Action<float,float> onPlayerHpChange;
    public event Action<int,int> onPlayerMpChange;

    #endregion
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
    }
    private void Start()
    {
        Hp = maxhp;
        Mp = 5;

        canAttack = true;
        canDragging = true;
        if(line != null)
        {
            line.positionCount = 2;
            line.enabled = false;
        }
    }
    
    private void Update()
    {
        CheckSimulationEnd();
        if (!canDragging) return;
        HandleInput();
    }
    void CheckSimulationEnd()
    {
        if (!isPlayerMoving) return;   // 플레이어가 움직이지 않으면 감시할 필요 없음

        if (rb.velocity.sqrMagnitude <= 0.0005f &&
        Mathf.Abs(rb.angularVelocity) <= 0.05f)
        {
            stopTimer += Time.deltaTime;
            if (stopTimer >= STOP_TIME)
            {
                isPlayerMoving = false;
                Debug.Log($"플레이어 움직임 멈춤, 몬스터 공격: {hadHit}");
                onPlayerStopped?.Invoke(hadHit);
            }
        }
        else
        {
            stopTimer = 0f;
        }
    }
    void HandleInput()
    {
        //이동중 조작금지
        if (rb.velocity.sqrMagnitude > stopThreshold * stopThreshold) return;

        if (PointerDown(out Vector2 downPos))
        {
            if (isPointerOnUI()) return;

            var hit = Physics2D.OverlapPoint(downPos);
            if (hit != null && hit.attachedRigidbody == rb)
            {
                BeginDrag(downPos);
            }
        }
        if (dragging && PointerHeld(out Vector2 holdPos))
        {
            current = holdPos;
            UpdateLine();
        }
        if (dragging && PointerUp(out Vector2 upPos))
        {
            current = upPos;
            Release();
        }
    }
    /// <summary>
    /// 플레이어가 마우스 / 터치했는지 확인
    /// </summary>
    /// <param name="pos">터치한 위치</param>
    /// <returns>터치여부</returns>
    bool PointerDown(out Vector2 pos)
    {
        //모바일 터치
        if (Input.touchCount > 0)
        {
            var t = Input.GetTouch(0);
            if(t.phase == TouchPhase.Began)
            {
                pos = cam.ScreenToWorldPoint(t.position);
                return true;
            }
        }
        //PC 클릭
        if (Input.GetMouseButtonDown(0))
        {
            pos = cam.ScreenToWorldPoint(Input.mousePosition);
            return true;
        }

        pos = default;
        return false;
    }
    /// <summary>
    /// 플레이어가 마우스 / 터치 유지중인지 확인
    /// </summary>
    /// <param name="pos">현재 터치중인 위치</param>
    /// <returns>터치여부</returns>
    bool PointerHeld(out Vector2 pos)
    {
        //모바일
        if (Input.touchCount > 0)
        {
            var t = Input.GetTouch(0);
            if(t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary)
            {
                pos = cam.ScreenToWorldPoint(t.position);
                return true;
            }
        }

        if (Input.GetMouseButton(0))
        {
            pos = cam.ScreenToWorldPoint(Input.mousePosition);
            return true;
        }

        pos = default;
        return false;
    }
    /// <summary>
    /// 플레이어가 마우스 / 포인터 뗐을 때의 위치
    /// </summary>
    /// <param name="pos">마우스 / 포인터 뗀 위치</param>
    /// <returns></returns>
    bool PointerUp(out Vector2 pos)
    {
        if (Input.touchCount > 0)
        {
            var t = Input.GetTouch(0);
            if(t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
            {
                pos = cam.ScreenToWorldPoint(t.position);
                return true;
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            pos = cam.ScreenToWorldPoint(Input.mousePosition);
            return true;
        }
        pos = default;
        return false;
    }
    void Release()
    {
        dragging = false;

        Vector2 delta = current - origin;
        float dist = Mathf.Min(delta.magnitude, maxDragDistance);
        //당기는힘 너무작으면 취소
        if (dist < 0.2f)
        {
            if (line != null) line.enabled = false;
            return;
        }

        Vector2 dir = -delta.normalized;
        float strength = dist / maxDragDistance;
        Vector2 impulse = dir * (power * strength);

        rb.AddForce(impulse, ForceMode2D.Impulse);
        onPlayerShooted?.Invoke(); //플레이어 발사 상태 발행

        Shooted();
        if (line != null) line.enabled = false;
    }
    bool isPointerOnUI()
    {
        if (EventSystem.current == null) return false;

        //모바일
        if (Input.touchCount > 0)
        {
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        }

        //PC
        return EventSystem.current.IsPointerOverGameObject();
    }
    void BeginDrag(Vector2 startPos)
    {
        dragging = true;
        origin = rb.position;
        current = startPos;

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        if(line != null)
        {
            line.enabled = true;
            UpdateLine();
        }
    }
    void UpdateLine()
    {
        if (line == null) return;

        Vector2 delta = current - origin;
        float dist = Mathf.Min(delta.magnitude, maxDragDistance);

        Vector2 clamped = origin + delta.normalized * Mathf.Max(0.001f, dist);
        Vector2 shootDir = (origin - clamped).normalized;
        Vector2 end = origin + shootDir * (dist * lineLengthMultiplier);

        line.SetPosition(0, origin);
        line.SetPosition(1, end);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!canAttack) return;
        Debug.Log("충돌");
        if (collision.collider.TryGetComponent<IHittable>(out target))
        {
            hadHit = true;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0;
            knockbackDir = ((Vector2)collision.collider.transform.position-(Vector2)transform.position).normalized;
        }
    }
    public void ApplyDiceResult(int Value)
    {
        damage = baseDamage * Mathf.Pow(Growth, Value - 1);
        knockBack = baseKnockBack * Mathf.Pow(Growth, Value - 1);
        if(target == null)
        {
            Debug.LogWarning("타겟이 없음");
            canAttack = false;
            return;
        }
        target.Hit(damage, knockbackDir, knockBack);
        canAttack = false;
    }
    public void Shooted()
    {
        isPlayerMoving = true;
        hadHit = false;
        stopTimer = 0f;
        target = null;
    }
    public void SetAttackEnabled(bool enabled)
    {
        canAttack = enabled;
    }
    public void SetInputEnabled(bool enabled)
    {
        canDragging = enabled;
    }
    public void ForceStop()
    {
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        line.enabled = false;
        dragging = false;
    }
    public void BeginTurn()
    {
        isPlayerMoving = true;
        canDragging = true;
        canAttack = true;
    }
    public void Damaged(float damage)
    {
        Hp-=damage;
        if (hp < 0)
        {
            IsDead = true;
        }
    }
}
