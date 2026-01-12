using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour, IHittable
{
    const float STOP_SPEED = 0.08f;
    const float STOP_TIME = 0.2f;

    public bool isDead;
    public float hp;
    public float maxHp;

    public float Hp
    {
        get
        {
            return hp;
        }
        protected set
        {
            hp = value;
            onMonstarHpChange?.Invoke(hp, maxHp);
            if (value < 0)
            {
                hp = 0;
                isDead = true;
                Die();
            }
        }
    }
    public float DefaultHp => 100;
    public float DefaultMaxHp => 100;
    public bool isLocked;
    public bool isHit;
    float hitStartTime;

    Rigidbody2D rb;
    public event Action onHitEnded;
    public event Action<float, float> onMonstarHpChange;
    private void Start()
    {
        Hp = 100;
        if(rb != null) TurnManager.Instance.RegisterMonsterRb(rb);
    }
    public void Hit(float damage, Vector2 knockbackDir, float knockbackPower)
    {
        Debug.Log($"[HIT] frame={Time.frameCount} obj={name}\n{Environment.StackTrace}");
        if (rb == null)
        {
            Debug.LogError($"[Monster.Hit] rb가 null! obj={name}", this);
            return;
        }
        isLocked = false;
        isHit = true;
        rb.isKinematic = false;
        hitStartTime = Time.time;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0;
        rb.AddForce(knockbackDir * knockbackPower, ForceMode2D.Impulse);
        rb.WakeUp();
        Hp -= damage;
        Debug.Log($"[HIT] frame={Time.frameCount} sleeping={rb.IsSleeping()} vel={rb.velocity}");
    }
    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Wall"))
        {
            Hp -= 15;
        }
    }
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            Debug.LogError($"[Monster] Rigidbody2D 없음! obj={name}", this);

        isLocked = true;
        maxHp = DefaultMaxHp;
        Hp = maxHp;
    }

    void FixedUpdate()
    {
        if (!isHit) return;

        if (Time.time - hitStartTime < 1f) return;

        if (rb.IsSleeping())
        {
            EndHit();
        }
    }
    void EndHit()
    {
        Debug.Log("EndHit");
        isHit = false;
        rb.isKinematic = true;
        onHitEnded?.Invoke();
    }
    void Die()
    {
        Debug.Log("몬스터 사망");
        TurnManager.Instance.UnregisterMonsterRb(rb);
        Destroy(gameObject);
    }

}
