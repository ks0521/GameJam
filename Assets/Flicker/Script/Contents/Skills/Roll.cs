using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Roll : MonoBehaviour
{
    /// <summary>
    /// 가중합 기반 랜덤 계산기
    /// </summary>
    /// <param name="weight">각 인덱스별 가중치</param>
    /// <returns>선택된 인덱스 번호, 오류 발생 시 무조건 0번 인덱스 반환</returns>
    public static int CalcWeight(int[] weight)
    {
        if (weight == null || weight.Length == 0)
        {
            Debug.LogWarning("빈 배열 들어옴");
            return 0;
        }
        //계산은 누적합 방식으로 계산
        int total = 0;
        for (int i = 0; i < weight.Length; i++)
        {
            if (weight[i] < 0)
            {
                Debug.LogWarning($"가중치 입력이 잘못되었습니다 weight[{i}] = {weight[i]}");
                return 0;
            }
            total += weight[i];
            //오류 발생 시 최하등급 부여
        }
        if (total == 0)
        {
            Debug.LogWarning("가중치 배열의 합이 0입니다!");
            return 0;
        }
        int random = UnityEngine.Random.Range(0, total);
        for (int i = 0; i < weight.Length; i++)
        {
            if (random >= weight[i])
            {
                random -= weight[i];
            }
            else return i;
        }
        Debug.Log("입력값 오류");
        return 0;
    }
}
