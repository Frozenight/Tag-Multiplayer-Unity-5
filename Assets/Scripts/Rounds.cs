using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class Rounds : NetworkBehaviour
{
    private int playerID = -1;

    private Vector3 _chaserStartingPoint = new Vector3(20, 0.53f, -14);
    private Vector3 _runnerStartingPoint = new Vector3(-10, 0.53f, 10);

    public NetworkVariable<int> firstPlyaerPoints = new NetworkVariable<int>(0);
    public NetworkVariable<int> secondPlyaerPoints = new NetworkVariable<int>(0);

    public int firstPlayerPoints
    {
        get { return firstPoints; }
        set
        {
            if (value != firstPoints)
            {
                firstPoints = value;
                score_1.text = firstPlayerPoints.ToString();
                if (secondPoints == 3)
                {
                    GameOver();
                }
                else
                    SwitchSides();
            }
        }
    }

    private int firstPoints;

    public int secondPlayerPoints
    {
        get { return secondPoints; }
        set
        {
            if (value != secondPoints)
            {
                secondPoints = value;
                score_2.text = secondPlayerPoints.ToString();
                if (secondPoints == 3)
                {
                    GameOver();
                }
                else
                    SwitchSides();
            }
        }
    }

    private int secondPoints;

    public NetworkVariable<int> RoundInfo = new NetworkVariable<int>(0);

    public NetworkVariable<bool> canHIt = new NetworkVariable<bool>(true);

    private TMP_Text score_1;
    private TMP_Text score_2;

    private playerRole role1;
    private playerRole role2;

    private TMP_Text _runnerText;
    private TMP_Text _chaserText;

    private float timeLeft = 4f;
    private bool _countDown = false;
    [SerializeField] private TMP_Text _countDownText;

    private float roundTimeLeft = 15f;
    private bool _roundCountDown = false;
    [SerializeField] private TMP_Text _roundcountDownText;

    [SerializeField] private TMP_Text _gameOverText;

    private enum playerRole
    {
        chaser,
        runner
    }

    private void Start()
    {
        GameObject score1 = GameObject.Find("Score_1");
        score_1 = score1.GetComponent<TextMeshProUGUI>();
        GameObject score2 = GameObject.Find("Score_2");
        score_2 = score2.GetComponent<TextMeshProUGUI>();
        score_1.text = 0.ToString();
        score_2.text = 0.ToString();
    }

    public override void OnNetworkSpawn()
    {
        StartCoroutine(loadingTime());
        GameObject[] tPlayer = GameObject.FindGameObjectsWithTag("Player");
        GameObject myPlayer = null;
        foreach (var player in tPlayer)
        {
            if (player.GetComponent<Owner>() != null)
                myPlayer = player;
        }

        GameObject chaser = GameObject.Find("Chaser");
        GameObject runner = GameObject.Find("Runner");
        _chaserText = chaser.GetComponent<TextMeshProUGUI>();
        _runnerText = runner.GetComponent<TextMeshProUGUI>();

        playerID = GetComponent<GameManager>().playerID;
    }

    // change for performance
    private void Update()
    {
        firstPlayerPoints = firstPlyaerPoints.Value;
        secondPlayerPoints = secondPlyaerPoints.Value;
        if (_countDown)
        {
            _countDownText.enabled = true;
            timeLeft -= Time.deltaTime;
            _countDownText.text = ((int)timeLeft).ToString();
            if (timeLeft < 1)
            {
                _countDown = false;
                _countDownText.enabled = false;
                _roundCountDown = true;
            }
        }

        if (_roundCountDown)
        {
            _roundcountDownText.enabled = true;
            roundTimeLeft -= Time.deltaTime;
            _roundcountDownText.text = ((int)roundTimeLeft).ToString();
            if (roundTimeLeft < 0)
            {
                _roundCountDown = false;
                _roundcountDownText.enabled = false;
                timerRanOutServerRpc();
            }
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void playerWasCaughtServerRpc()
    {
        if (canHIt.Value)
        {
            if (role1 == playerRole.chaser)
            {
                firstPlyaerPoints.Value = firstPlyaerPoints.Value + 1;
            }
            else
            {
                secondPlyaerPoints.Value = secondPlyaerPoints.Value + 1;
            }
            _roundcountDownText.enabled = false;
            _roundCountDown = false;
            canHIt.Value = false;
            StartCoroutine(cooldowns());
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void timerRanOutServerRpc()
    {
        if (canHIt.Value)
        {
            if (role1 == playerRole.runner)
            {
                firstPlyaerPoints.Value = firstPlyaerPoints.Value + 1;
            }
            else
            {
                secondPlyaerPoints.Value = secondPlyaerPoints.Value + 1;
            }
            canHIt.Value = false;
            StartCoroutine(cooldowns());
        }
    }

    public void StartTagGame()
    {
        GameObject[] tPlayer = GameObject.FindGameObjectsWithTag("Player");
        GameObject myPlayer = null;
        foreach (var player in tPlayer)
        {
            if (player.GetComponent<Owner>() != null)
                myPlayer = player;
        }
        if (playerID == 0)
        {
            myPlayer.GetComponent<CharacterController>().enabled = false;
            myPlayer.transform.position = _runnerStartingPoint;
            role1 = playerRole.runner;
            _runnerText.enabled = true;
            score_1.fontSize = 72;
            score_2.fontSize = 48;
            StartCoroutine(countdown());
        }
        else
        {
            myPlayer.GetComponent<CharacterController>().enabled = false;
            myPlayer.transform.position = _chaserStartingPoint;
            role2 = playerRole.chaser;
            _chaserText.enabled = true;
            score_1.fontSize = 48;
            score_2.fontSize = 72;
            StartCoroutine(countdown());
        }
        _countDown = true;
        GameObject obj = GameObject.Find("GameManager");
        obj.GetComponent<GameManager>().gameHasStarted = true;
    }

    public void SwitchSides()
    {
        if (role1 == playerRole.chaser)
        {
            role1 = playerRole.runner;
            role2 = playerRole.chaser;
        }
        else
        {
            role1 = playerRole.chaser;
            role2 = playerRole.runner;
        }
        GameObject[] tPlayer = GameObject.FindGameObjectsWithTag("Player");
        GameObject myPlayer = null;
        foreach (var player in tPlayer)
        {
            if (player.GetComponent<Owner>() != null)
                myPlayer = player;
        }

        myPlayer.GetComponent<CharacterController>().enabled = false;

        if (role1 == playerRole.chaser)
        {
            myPlayer.transform.position = _chaserStartingPoint;
            _chaserText.enabled = true;
        }
        else
        {
            myPlayer.transform.position = _runnerStartingPoint;
            _runnerText.enabled = true;
        }
        StartCoroutine(countdown());
    }

    public void GameOver()
    {
        _gameOverText.gameObject.SetActive(true);
        _gameOverText.text = "You've won";
    }


    private IEnumerator countdown()
    {
        GameObject[] tPlayer = GameObject.FindGameObjectsWithTag("Player");
        GameObject myPlayer = null;
        foreach (var player in tPlayer)
        {
            if (player.GetComponent<Owner>() != null)
                myPlayer = player;
        }
        var delay = new WaitForSecondsRealtime(3);
        yield return delay;
        _roundCountDown = true;
        roundTimeLeft = 60f;
        _chaserText.enabled = false;
        _runnerText.enabled = false;
        myPlayer.GetComponent<CharacterController>().enabled = true;
    }

    private IEnumerator cooldowns()
    {
        var delay = new WaitForSecondsRealtime(5);
        yield return delay;
        _roundCountDown = true;
        returnCanHitServerRpc();
    }

    private IEnumerator loadingTime()
    {
        var delay = new WaitForSecondsRealtime(1.5f);
        yield return delay;
    }

    [ServerRpc(RequireOwnership = false)]
    private void returnCanHitServerRpc()
    {
        canHIt.Value = true;
    }
}
