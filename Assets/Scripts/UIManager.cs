using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] public TMP_Text stateText;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.instance.UpdateState += UpdateState;
    }

    private void UpdateState(string newState, ulong id)
    {
        stateText.text = newState;
        if (newState == "Player found!" && id == 1)
        {
            StartCoroutine(startGame());
        }
    }

    IEnumerator startGame()
    {
        var delay = new WaitForSeconds(5);
        yield return delay;
        stateText.text = "";
        GameObject gManager = GameObject.Find("GameManager");
        gManager.GetComponent<GameManager>().gameHasStarted = false;
        GameObject obj = GameObject.Find("GameManager");
        obj.GetComponent<Rounds>().StartTagGame();
    }

    private void OnDestroy()
    {
        Debug.Log("Removed UpdateState");
        GameManager.instance.UpdateState -= UpdateState;
        GameManager._instance = null;
    }
}
