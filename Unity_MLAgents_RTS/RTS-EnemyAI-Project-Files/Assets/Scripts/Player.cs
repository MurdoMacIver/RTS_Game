using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class Player : MonoBehaviour
{
    public int team = 0;

    public bool isMe;

    [Header("Units")]
    public List<Unit> units = new List<Unit>();
    //public List<Unit> soliders = new List<Unit>();
    //public List<Unit> commanders = new List<Unit>();

    [Header("Resources")]
    public int food;

    [Header("Components")]

    #region unit components
    [Header("Gatherer Unit")]
    public GameObject unitPrefab;
    public Transform unitSpawnPos;

    [Header("Soldier Unit")]
    public GameObject unit2Prefab;
    public Transform unit2SpawnPos;
    public GameObject[] soliderArray;

    [Header("Commander Unit")]
    public GameObject unit3Prefab;
    public Transform unit3SpawnPos;
    public GameObject[] commanderArray;
    #endregion

    // events
    [System.Serializable]
    public class UnitCreatedEvent : UnityEvent<Unit> { }
    public UnitCreatedEvent onUnitCreated;

    #region unit costs
    public readonly int unitCost = 50;

    public readonly int unit2Cost = 25; //only lower cause the system developed takes away money * the number of units created

    public readonly int unit3Cost = 40;
    #endregion

    public static Player me;

    public int maxUnits = 5;


    void Start ()
    {
        if(isMe)
        {
            GameUI.instance.UpdateUnitCountText(units.Count);
            GameUI.instance.UpdateFoodText(food);
            GameUI.instance.UpdateSoldierCountText(units.Count);
            GameUI.instance.UpdateCommanderCountText(units.Count);

            CameraController.instance.FocusOnPosition(unitSpawnPos.position);
        }

        food += unitCost;
        CreateNewUnit();
    }

    // called when a unit gathers a certain resource
    public void GainResource (ResourceType resourceType, int amount)
    {
        switch(resourceType)
        {
            case ResourceType.Food:
            {
                food += amount;

                if(isMe)
                    GameUI.instance.UpdateFoodText(food);

                break;
            }
        }
    }

    // debug to see if a unit spawns or not when a key is pressed
    /*void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
            CreateNewUnit2();
    }*/

    // creates a new unit for the player
    #region create units

    #region Gatherer unit create
    public void CreateNewUnit ()
    {
        if (units.Count >= maxUnits)
            return;

        if(food - unitCost < 0)
            return;

        GameObject unitObj = Instantiate(unitPrefab, unitSpawnPos.position, Quaternion.identity, transform);
        Unit unit = unitObj.GetComponent<Unit>();
        unit.team = team;

        units.Add(unit);
        unit.player = this;
        food -= unitCost;

        if(onUnitCreated != null)
            onUnitCreated.Invoke(unit);

        if(isMe)
        {
            GameUI.instance.UpdateUnitCountText(units.Count);
            GameUI.instance.UpdateFoodText(food);
        }
    }
    #endregion

    #region Solider unit create
    public void CreateNewUnit2()
    {
            if (food - unit2Cost < 0)
                return;

        soliderArray = new GameObject[4]; // creates 4 of the 1 unit
        for (int i = 0; i < soliderArray.Length; i++)
        {
            GameObject unitObj2 = Instantiate(unit2Prefab, unit2SpawnPos.position, Quaternion.identity, transform);
            Unit unit = unitObj2.GetComponent<Unit>();

            units.Add(unit);
            unit.player = this;
            food -= unit2Cost;

            if (onUnitCreated != null)
                onUnitCreated.Invoke(unit);

            if (isMe)
            {
                GameUI.instance.UpdateUnitCountText(units.Count);
                GameUI.instance.UpdateFoodText(food);
            }
            
        }
    }
    
    #endregion

    #region Commander unit create
    public void CreateNewUnit3()
    {
        if (food - unit3Cost < 0)
            return;

        commanderArray = new GameObject[5]; //creates 6 of the 1 unit
        for (int i = 0; i < commanderArray.Length; i++)
        {
            GameObject unitObj3 = Instantiate(unit3Prefab, unit3SpawnPos.position, Quaternion.identity, transform);
            Unit unit = unitObj3.GetComponent<Unit>();
        
            units.Add(unit);
            unit.player = this;
            food -= unit3Cost;

            if (onUnitCreated != null)
                onUnitCreated.Invoke(unit);

            if (isMe)
            {
                GameUI.instance.UpdateUnitCountText(units.Count);
                GameUI.instance.UpdateFoodText(food);
            }
        }
    }
    #endregion

    #endregion

    // is this my unit?
    public bool IsMyUnit (Unit unit)
    {
        return units.Contains(unit);
    }
}