using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collide : MonoBehaviour
{
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        GameObject obj = GameObject.Find("GameManager");
        if (hit.collider.tag == "Player" && obj.GetComponent<GameManager>().gameHasStarted)
        {
            obj.GetComponent<Rounds>().playerWasCaughtServerRpc();
        }
    }
}
