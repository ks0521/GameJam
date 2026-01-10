using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour, IHittable
{
    public bool isDead;
    public int hp;
    public int maxHp;
    public int Hp
    {
        get
        {
            return hp;
        }
        protected set
        {
            hp = value;
            if (value < 0)
            {
                hp = 0;
                isDead = true;
                Die();
            }
        }
    }
    public int DefaultHp => 100;
    public int DefaultMaxHp => 100;
    public void Hit(int damage)
    {
        Debug.Log(damage);
        Hp -= damage;
    }

    // Start is called before the first frame update
    void Start()
    {
        maxHp = DefaultMaxHp;
        Hp = maxHp;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void Die()
    {
        Debug.Log("몬스터 사망");
        Destroy(gameObject);
    }
}
