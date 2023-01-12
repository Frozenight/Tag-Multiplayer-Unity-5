using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbUpWallTop : MonoBehaviour
{
    public IEnumerator ClimbUpTheWall()
    {
        yield return new WaitForSeconds(1);
        GetComponent<NetworkMovement>().controller.height = GetComponent<NetworkMovement>().controllerDefaultHeight;
        GetComponent<NetworkMovement>().controller.center = GetComponent<NetworkMovement>().controllerDefaultCenter;
        GetComponent<NetworkMovement>().climbUpTheWall = false;
    }
}
