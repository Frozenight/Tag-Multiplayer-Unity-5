using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class FollowCamera : MonoBehaviour
{
    [SerializeField]
    private CinemachineFreeLook vcam;
    [SerializeField]
    private GameObject loading_screen;

    public bool playerInitiated
    {
        get { return playerInitiatedPrivate; }
        set
        {
                if (value != playerInitiatedPrivate)
                {
                    playerInitiatedPrivate = value;
                }
            }
    }
    private bool playerInitiatedPrivate = false;

    public bool gameStarted = false;


    IEnumerator wait()
    {
        yield return new WaitForSecondsRealtime(2);
        Assign();
    }

    public void Assign()
    {
        Cursor.lockState = CursorLockMode.Locked;

        GameObject[] tPlayer = GameObject.FindGameObjectsWithTag("Player");
        if (tPlayer.Length == 0)
        {
            StartCoroutine(wait());
            
        }
        GameObject myPlayer = null;
        foreach (var player in tPlayer)
        {
            if (player.GetComponent<Owner>() != null)
            {
                myPlayer = player;
            }
        }
        for (int i = 0; i < 5; i++)
        {
            if (myPlayer == null)
            {
                StartCoroutine(HeartBeatLobbyCoroutine(2));
            }
            else
                break;
        }
        playerInitiated = true;
        loading_screen.SetActive(false);
        Transform head = myPlayer.transform.GetChild(1).transform.GetChild(2).transform.GetChild(0).transform.GetChild(0).transform.GetChild(1).transform.GetChild(0);
        vcam.LookAt = head;
        vcam.Follow = myPlayer.transform;
        gameStarted = true;
    }

    IEnumerator HeartBeatLobbyCoroutine(float waitTimeSeconds)
    {
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);
        yield return delay;
    }

}
