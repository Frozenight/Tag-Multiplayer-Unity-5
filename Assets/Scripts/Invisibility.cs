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
        int i = -1;
        if (transform.GetChild(0).gameObject.activeSelf)
        {
            transform.GetChild(0).gameObject.SetActive(false);
            i = 0;
            Debug.Log(i);
        }

        else
        {
            transform.GetChild(1).gameObject.SetActive(false);
            i = 1;
            Debug.Log(i);
        }
        yield return new WaitForSeconds(invisTime);
        skinRenderer.enabled = true;
        yield return new WaitForSeconds(invisCooldown);
        transform.GetChild(i).gameObject.SetActive(true);
        canGoInvis = true;
    }
}
