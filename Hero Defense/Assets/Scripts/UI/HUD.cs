using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{

    public Image portraitImg;
    #region Singleton
    public static HUD instance;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one intance of GameManager!");
            return;
        }
        instance = this;
    }
    #endregion



    public void RegisterPlayerPortrait(Sprite portrait)
    {
        portraitImg.sprite = portrait;
    }
}
