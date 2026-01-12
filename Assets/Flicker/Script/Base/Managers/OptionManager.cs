using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionManager : MonoBehaviour
{
    [SerializeField] GameObject option;

    bool isOpened;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isOpened)
            {
                Time.timeScale = 1;
                option.SetActive(false);
                isOpened = false;
            }
            else
            {
                Time.timeScale = 0;
                option.SetActive(true);
                isOpened = true;
            }
        }
    }
    public void OnResume()
    {
        option.SetActive(false);
        Time.timeScale = 1;
    }
    public void OnMain()
    {
        option.SetActive(false);
        Time.timeScale = 1;
        GameManager.Instance.LoadScene(0);
    }
    public void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
