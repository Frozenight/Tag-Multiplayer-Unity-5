using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class Rounds : NetworkBehaviour
{
    private int playerID = -1;

    private Vector3 _chaserStartingPoint = new Vector3(-32.17f, 8.44f, -7.0f);
    private Vector3 _runnerStartingPoint = new Vector3(44.011f, 8.44f, -7.0f);

    public NetworkVariable<int> firstPlyaerPoints = new NetworkVariable<int>(0);
    public NetworkVariable<int> secondPlyaerPoints = new NetworkVariable<int>(0);
    public int pointsToWin = 3;

    [SerializeField] private Material _firstPlyaerMaterial;
    [SerializeField] private Material _secondPlyaerMaterial;

    public int firstPlayerPoints
    {
        get { return firstPoints; }
        set
        {
            if (value != firstPoints)
            {
                firstPoints = value;
                score_1.text = firstPlayerPoints.ToString();
                if (firstPoints == pointsToWin)
                {
                    GameOver(0);
                }
                else if (firstPoints == 0)
                    return;
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
                if (secondPoints == pointsToWin)
                {
                    GameOver(1);
                }
                else if (secondPoints == 0)
                    return;
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
    public float roundTime = 10f;
    [SerializeField] private TMP_Text _countDownText;

    private float roundTimeLeft = 10f;
    private bool _roundCountDown = false;
    [SerializeField] private TMP_Text _roundcountDownText;
    [SerializeField] private TMP_Text _catchTimer;
    [SerializeField] private TMP_Text _gameOverText;

    private enum playerRole
    {
        chaser,
        runner
    }

    [SerializeField]private GameManager gameController;

    [SerializeField] private AudioSource backgroundMusic;
    [SerializeField] private TMP_Text _catchTimerText;

    private GameObject myPlayer;
    private GameObject enemyPlayer;
    private float safeDistance = 7f;
    private float catchTimer = 5f;

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

        GameObject chaser = GameObject.Find("Chaser");
        GameObject runner = GameObject.Find("Runner");
        _chaserText = chaser.GetComponent<TextMeshProUGUI>();
        _runnerText = runner.GetComponent<TextMeshProUGUI>();

        playerID = GetComponent<GameManager>().playerID;
    }

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

            if (myPlayer == null || enemyPlayer == null)
            {
                GameObject[] tPlayer = GameObject.FindGameObjectsWithTag("Player");
                foreach (var player in tPlayer)
                {
                    if (player.GetComponent<Owner>() != null)
                        myPlayer = player;
                    else
                        enemyPlayer = player;
                }
            }

            if (Vector3.Distance(myPlayer.transform.position, enemyPlayer.transform.position) < safeDistance)
            {
                if (role1 == playerRole.runner)
                    _catchTimerText.text = "You're almost caught! Keep running!";
                else
                    _catchTimerText.text = "Don't let the runner get away!";
                catchTimer -= Time.deltaTime;
                _catchTimer.text = catchTimer.ToString("0");
                float scale = Mathf.Lerp(0.8f, 1.2f, Mathf.PingPong(catchTimer, 1f));
                _catchTimer.transform.localScale = new Vector3(scale, scale, scale);
            }
            else
            {
                _catchTimer.text = string.Empty;
                _catchTimerText.text = string.Empty;
                catchTimer = 5f;
                _catchTimer.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            }

            if (catchTimer <= 0)
            {
                playerWasCaughtServerRpc();
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
        backgroundMusic.Play();
        GameObject[] tPlayer = GameObject.FindGameObjectsWithTag("Player");
        GameObject myPlayer = null;
        GameObject enemey = null;
        foreach (var player in tPlayer)
        {
            if (player.GetComponent<Owner>() != null)
                myPlayer = player;
            else
                enemey = player;
            player.GetComponent<NetworkMovement>().ResetActions();
        }
        if (playerID == 0)
        {
            myPlayer.GetComponent<CharacterController>().enabled = false;
            myPlayer.GetComponent<NetworkMovement>().flag.SetActive(true);
            enemey.GetComponent<NetworkMovement>().flag.SetActive(false);
            myPlayer.transform.position = _runnerStartingPoint;
            myPlayer.transform.GetChild(0).GetComponent<Renderer>().material = _firstPlyaerMaterial;
            enemey.transform.GetChild(0).GetComponent<Renderer>().material = _secondPlyaerMaterial;
            role1 = playerRole.runner;

            _runnerText.enabled = true;
            score_1.fontSize = 72;
            score_2.fontSize = 48;
            StartCoroutine(countdown());
        }
        else
        {
            myPlayer.GetComponent<CharacterController>().enabled = false;
            myPlayer.GetComponent<NetworkMovement>().flag.SetActive(false);
            enemey.GetComponent<NetworkMovement>().flag.SetActive(true);
            myPlayer.transform.position = _chaserStartingPoint;
            myPlayer.transform.GetChild(0).GetComponent<Renderer>().material = _secondPlyaerMaterial;
            enemey.transform.GetChild(0).GetComponent<Renderer>().material = _firstPlyaerMaterial;
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

    public void Disconnect()
    {
        backgroundMusic.Stop();
        firstPlayerPoints = 0;
        secondPlayerPoints = 0;
        score_1.enabled = false;
        score_2.enabled = false;
        _countDownText.enabled = false;
    }

    public void SwitchSides()
    {
        _catchTimer.text = string.Empty;
        _catchTimerText.text = string.Empty;
        backgroundMusic.Play();
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
        GameObject enemey = null;
        foreach (var player in tPlayer)
        {
            if (player.GetComponent<Owner>() != null)
                myPlayer = player;
            else
                enemey = player;
            player.GetComponent<NetworkMovement>().ResetActions();
        }

        myPlayer.GetComponent<CharacterController>().enabled = false;

        if (role1 == playerRole.chaser)
        {
            myPlayer.transform.position = _chaserStartingPoint;
            _chaserText.enabled = true;
            myPlayer.GetComponent<NetworkMovement>().flag.SetActive(false);
            enemey.GetComponent<NetworkMovement>().flag.SetActive(true);
        }
        else
        {
            myPlayer.transform.position = _runnerStartingPoint;
            _runnerText.enabled = true;
            myPlayer.GetComponent<NetworkMovement>().flag.SetActive(true);
            enemey.GetComponent<NetworkMovement>().flag.SetActive(false);
        }
        timeLeft = 4f;
        _roundcountDownText.enabled = false;
        _roundCountDown = false;
        _countDown = true;
        StartCoroutine(countdown());
    }

    public void GameOver(int winnderId)
    {
        Debug.Log(winnderId + " winnderId");
        Debug.Log(playerID + " playerId");
        _gameOverText.GetComponent<TextMeshProUGUI>().enabled = true;
        if (winnderId == playerID)
        {
            _gameOverText.text = "You've won";
            if (!PlayerPrefs.HasKey("Total Wins"))
                PlayerPrefs.SetInt("Total Wins", 0);
            else
                PlayerPrefs.SetInt("Total Wins", PlayerPrefs.GetInt("Total WIns") + 1);
        }
        else
        {
            _gameOverText.text = "You've lost";
            if (!PlayerPrefs.HasKey("Total Losses"))
                PlayerPrefs.SetInt("Total Losses", 0);
            else
                PlayerPrefs.SetInt("Total Losses", PlayerPrefs.GetInt("Total Losses") + 1);
        }
        score_1.enabled = false;
        score_2.enabled = false;
        _countDownText.enabled = false;
        StartCoroutine(ReturnToMainMenu());
    }

    IEnumerator ReturnToMainMenu()
    {
        yield return new WaitForSecondsRealtime(5f);
        gameController.Disconnect();
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
        roundTimeLeft = roundTime;
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
