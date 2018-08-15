using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Wave", menuName = "Wave/Wave")]
public class Wave : ScriptableObject {
    public int noOfMelee = 0;
    public int noOfRanged = 0;
    public int noOfSpecialRanged = 0;
    public int noOfBosses = 0;
}
