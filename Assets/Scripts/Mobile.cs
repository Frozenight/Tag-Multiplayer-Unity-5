using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mobile : MonoBehaviour
{
    private bool sprinting = false;
    public void changeValue()
    {
        GameObject[] tPlayer = GameObject.FindGameObjectsWithTag("Player");
        GameObject myPlayer = null;
        foreach (var player in tPlayer)
        {
            if (player.GetComponent<Owner>() != null)
                myPlayer = player;
        }
        myPlayer.GetComponent<NetworkMovement>().CheckForWallPart2Mobile();
    }

    public void Jump()
    {
        GameObject[] tPlayer = GameObject.FindGameObjectsWithTag("Player");
        GameObject myPlayer = null;
        foreach (var player in tPlayer)
        {
            if (player.GetComponent<Owner>() != null)
                myPlayer = player;
        }
        myPlayer.GetComponent<NetworkMovement>().Jump();
    }

    public void Sprint()
    {
        if (!sprinting)
        {
            Enable_sprint();
            sprinting = true;
        }
        else
        {
            Disable_sprint();
            sprinting = false;
        }
    }

    public void Disable_sprint()
    {
        GameObject[] tPlayer = GameObject.FindGameObjectsWithTag("Player");
        GameObject myPlayer = null;
        foreach (var player in tPlayer)
        {
            if (player.GetComponent<Owner>() != null)
                myPlayer = player;
        }
        myPlayer.GetComponent<NetworkMovement>().disable_sprint();
    }
    public void Enable_sprint()
    {
        GameObject[] tPlayer = GameObject.FindGameObjectsWithTag("Player");
        GameObject myPlayer = null;
        foreach (var player in tPlayer)
        {
            if (player.GetComponent<Owner>() != null)
                myPlayer = player;
        }
        myPlayer.GetComponent<NetworkMovement>().enable_sprint();
    }
}
