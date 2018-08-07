using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrenciesUI : MonoBehaviour {


    public Text cashText;
    public Text partsText;

    public void SetCash(int value)
    {
        cashText.text = "Cash: " + value;
    }

    public void SetParts(int value)
    {
        partsText.text = "Parts: " + value;
    }
}
