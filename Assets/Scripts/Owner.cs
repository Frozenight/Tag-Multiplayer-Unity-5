using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Owner : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            StartCoroutine(wait());
    }

    IEnumerator wait()
    {
        yield return new WaitForSecondsRealtime(2);
        Destroy(this);
    }
}
