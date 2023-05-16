using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Score : MonoBehaviour
{
    [SerializeField] GameObject simpleHelveticaWins;
    [SerializeField] GameObject simpleHelveticaLosses;

    private void Start()
    {
        UpdateScore();
    }

    public void UpdateScore()
    {
        SimpleHelvetica simpleHelveticaScriptWins = simpleHelveticaWins.GetComponent<SimpleHelvetica>();
        simpleHelveticaScriptWins.ChangeText(PlayerPrefs.GetInt("Total Wins", 0).ToString()); // Change the text to "New Text"

        SimpleHelvetica simpleHelveticaScriptLosses = simpleHelveticaLosses.GetComponent<SimpleHelvetica>();
        simpleHelveticaScriptLosses.ChangeText(PlayerPrefs.GetInt("Total Losses", 0).ToString()); // Change the text to "New Text"
    }

}
