using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class DiceRoll : MonoBehaviour
{
    [SerializeField] GameObject prefeb;
    GameObject obj;
    Rigidbody rb;

    [Header("각 눈의 Rotation(0 = 1눈의 회전, 1 = 2눈의 회전 ...)")] 
    [SerializeField]Transform[] factUp = new Transform[6];

    [Header("Settle motion")]
    [SerializeField] private float alignTime = 0.2f;   // 목표 회전으로 천천히 맞추는 시간
    [SerializeField] private float snapEpsilon = 1.0f; // 각도 오차 허용(도)

    public bool isSettled {  get; private set; }
    public int resultIndex {  get; private set; }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Rolling();
        }
    }
    IEnumerator DeleteObj(GameObject obj)
    {
        yield return new WaitForSeconds(6);
        Destroy(obj);
    }
    void Rolling()
    {
        obj = Instantiate(prefeb, transform.position, Quaternion.identity);
        StartCoroutine(DeleteObj(obj));
        rb = obj.GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Vector2 dir2 = Random.insideUnitCircle.normalized;
        Vector3 dir = new Vector3(dir2.x, 0, dir2.y);

        float planar = Random.Range(10f, 20f);
        float up = Random.Range(5f, 8f);

        rb.AddForce(dir * planar + Vector3.up * up, ForceMode.VelocityChange);

        rb.AddTorque(Random.insideUnitSphere * Random.Range(25f, 40f), ForceMode.VelocityChange);
        Result();
    }
    void Result()
    {
        resultIndex = Random.Range(0, 6);
    }
}
