using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/*class used to updating the values of the number of units created and the amount of resources gathered*/

public class GameUI : MonoBehaviour
{
    public TextMeshProUGUI unitCountText;
    public TextMeshProUGUI foodText;
    public TextMeshProUGUI soldierCountText;
    public TextMeshProUGUI commanderCountText;

    public static GameUI instance;

    void Awake ()
    {
        instance = this;
    }

    public void UpdateUnitCountText (int value)
    {
        unitCountText.text = value.ToString();
    }

    public void UpdateFoodText (int value)
    {
        foodText.text = value.ToString();
    }

    public void UpdateSoldierCountText (int value)
    {
        soldierCountText.text = value.ToString();
    }

    public void UpdateCommanderCountText (int value)
    {
        commanderCountText.text = value.ToString();
    }
}