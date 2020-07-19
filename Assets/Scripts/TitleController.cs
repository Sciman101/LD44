using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleController : MonoBehaviour
{
    public Image fade;

    private void Start()
    {
        fade.gameObject.SetActive(true);
        fade.CrossFadeAlpha(0, .1f, false);
    }

    public void Play()
    {
        fade.CrossFadeAlpha(1, 1, false);
        Invoke("LoadMainScene", 1);
    }

    void LoadMainScene()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void Quit()
    {
        Application.Quit();
    }
}
