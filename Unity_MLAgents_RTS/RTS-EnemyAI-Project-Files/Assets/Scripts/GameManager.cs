using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Player[] players;

    public static GameManager instance;

    void Awake ()
    {
        instance = this;
    }

    // returns a random enemy player
    public Player GetRandomEnemyPlayer (Player me)
    {
        Player ranPlayer = players[Random.Range(0, players.Length)];

        while(ranPlayer == me)
        {
            ranPlayer = players[Random.Range(0, players.Length)];
        }

        return ranPlayer;
    }

    // called when a unit dies, check to see if there's one remaining player
    public void UnitDeathCheck ()
    {
        //////=================================REMOVE RETURN AFTER TRAINING
        return;
        //////=============================================================


        int remainingPlayers = 0;
        Player winner = null;

        for(int x = 0; x < players.Length; x++)
        {
            if(players[x].units.Count > 0)
            {
                remainingPlayers++;
                winner = players[x];
            }
        }

        // if there is more than 1 remaining player, return
        if(remainingPlayers != 1)
            return;

        EndScreenUI.instance.SetEndScreen(winner.isMe);
    }

    public static void ResetResourceSource(ResourceSource resourceSource)
    {
        var instance = GameObject.FindObjectOfType<GameManager>();
        if (!instance) return;
        instance.StartCoroutine(instance.ResetResourceSourceEnum(resourceSource));
    }

    private IEnumerator ResetResourceSourceEnum(ResourceSource resourceSource)
    {
        resourceSource.gameObject.SetActive(false);
        yield return new WaitForSeconds(600f);
        resourceSource.quantity = 100;
        resourceSource.gameObject.SetActive(true);
    }
}