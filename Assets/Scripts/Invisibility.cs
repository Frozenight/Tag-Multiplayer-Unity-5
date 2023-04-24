using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Invisibility : MonoBehaviour
{
    bool canGoInvis = true;
    float invisCooldown = 5f;
    float invisTime = 2f;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            if (canGoInvis)
                StartCoroutine(TurnInvisible(other.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>()));
        }
    }

    IEnumerator TurnInvisible(SkinnedMeshRenderer skinRenderer)
    {
        canGoInvis = false;
        skinRenderer.enabled = false;
        GetComponent<MeshRenderer>().enabled = false;
        transform.GetChild(0).gameObject.SetActive(false);
        yield return new WaitForSeconds(invisTime);
        skinRenderer.enabled = true;
        yield return new WaitForSeconds(invisCooldown);
        GetComponent<MeshRenderer>().enabled = true;
        transform.GetChild(0).gameObject.SetActive(true);
        canGoInvis = true;
    }
}
