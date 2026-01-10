using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class DiceRoll : MonoBehaviour
{
    Rigidbody rb;
    public event Action<int> OnResult;
    [Header("각 눈의 Rotation(0 = 1눈의 회전, 1 = 2눈의 회전 ...)")]
    [SerializeField] DiceSide[] Sides = new DiceSide[6];

    public bool isSettled { get; private set; }
    public int resultVelue { get; private set; }
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    IEnumerator DeleteObj()
    {
        yield return new WaitForSeconds(3);
        Destroy(this);
    }
    public void Rolling()
    {
        StopAllCoroutines();
        isSettled = false;
        //StartCoroutine(DeleteObj(obj));
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Vector2 dir2 = UnityEngine.Random.insideUnitCircle.normalized;
        Vector3 dir = new Vector3(dir2.x, 0, dir2.y);

        float planar = UnityEngine.Random.Range(10f, 20f);
        float up = UnityEngine.Random.Range(5f, 8f);

        rb.AddForce(dir * planar + Vector3.up * up, ForceMode.VelocityChange);
        rb.AddTorque(UnityEngine.Random.insideUnitSphere * UnityEngine.Random.Range(25f, 40f), ForceMode.VelocityChange);

        Sides = GetComponentsInChildren<DiceSide>(true);
        StartCoroutine(SettleDice(rb, Sides));
    }
    IEnumerator SettleDice(Rigidbody rb, DiceSide[] sides)
    {
        while (!rb.IsSleeping()) yield return null;
        yield return null;

        resultVelue = GetTopFaceValue(sides);

        isSettled = true;
        Debug.Log($"주사위 결과 : {resultVelue}");

        OnResult?.Invoke(resultVelue);
    }
    int GetTopFaceValue(DiceSide[] sides)
    {
        int bestValue = 0;
        float BestDot = -999f;

        for (int i = 0; i < sides.Length; i++)
        {
            float dot = Vector3.Dot(sides[i].transform.up, Vector3.up);

            if (dot > BestDot)
            {
                BestDot = dot;
                bestValue = sides[i].value;
            }
        }
        return bestValue;
    }
}
