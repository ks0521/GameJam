using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public void LoadScene(int scene)
    {
        switch (scene)
        {
            case 0:
                SceneManager.LoadScene(0);
                break;
            case 1:
                SceneManager.LoadScene(1);
                break;
            default:
                Debug.LogWarning("해당 씬은 없습니다");
                break;
        }
    }
}
