using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class Menu : MonoBehaviour
{
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject local_menu;
    [SerializeField] private GameObject game_selection_menu;
    [SerializeField] private GameObject settings_menu;
    [SerializeField] private GameObject connection_menu;
    [SerializeField] private GameObject paused_menu;
    [SerializeField] private Toggle mobile_toggle;
    [SerializeField] private TMP_Dropdown screenMode_dropdown;
    [SerializeField] private GameObject loading_screen;

    [SerializeField] private GameObject joystick;

    [SerializeField] private Button pause_button;
    [SerializeField] private Button jump_button;
    [SerializeField] private Button sprint_button;

    public TMP_InputField input_ipAdress;
    public TMP_Text host_IPadress_textbox;
    public TMP_Text connectionState_text;

    private TMP_Text score_1;
    private TMP_Text score_2;


    public bool mobile = false;


    private void Start()
    {
        GameObject score1 = GameObject.Find("Score_1");
        score_1 = score1.GetComponent<TextMeshProUGUI>();
        GameObject score2 = GameObject.Find("Score_2");
        score_2 = score2.GetComponent<TextMeshProUGUI>();

        screenMode_dropdown.value = PlayerPrefs.GetInt("ScreenMode");
        //Fetch the Toggle GameObject
        GameObject toggle = GameObject.Find("Mobile_toggle");
        //Add listener for when the state of the Toggle changes, to take action
        mobile_toggle.onValueChanged.AddListener(delegate {
            ToggleValueChanged(mobile_toggle);
        });
    }

    public void playerInitiated()
    {
        if (mobile)
        {
            pause_button.gameObject.SetActive(true);
            jump_button.gameObject.SetActive(true);
            sprint_button.gameObject.SetActive(true);
            joystick.SetActive(true);
        }
        score_1.enabled = true;
        score_2.enabled = true;
    }

    //Output the new state of the Toggle into Text
    void ToggleValueChanged(Toggle change)
    {
        if (change.isOn)
        {
            mobile = true;
            screenMode_dropdown.gameObject.SetActive(false);
        }
        else
        { 
            mobile = false;
            screenMode_dropdown.gameObject.SetActive(true);
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Pause_Game();
    }
    public void Enter_Game_Selection()
    {
        menu.SetActive(false);
        game_selection_menu.SetActive(true);
    }

    public void Start_Game()
    {
        connection_menu.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        loading_screen.SetActive(true);
        Time.timeScale = 1;
    }

    public void Exit_Game()
    {
        Application.Quit();
    }

    public void Open_Settings()
    {
        menu.SetActive(false);
        settings_menu.SetActive(true);
    }

    public void Close_Settings()
    {
        menu.SetActive(true);
        settings_menu.SetActive(false);
    }

    public void Close_GameSelection()
    {
        menu.SetActive(true);
        game_selection_menu.SetActive(false);
    }

    public void Screen_Modes(int value)
    {
        if (value == 0)
        {
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
            PlayerPrefs.SetInt("ScreenMode", 0);
        }
        if (value == 1)
        {
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerPrefs.SetInt("ScreenMode", 1);
        }
        if (value == 2)
        {
            Screen.fullScreenMode = FullScreenMode.MaximizedWindow;
            PlayerPrefs.SetInt("ScreenMode", 2);
        }
        if (value == 3)
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
            PlayerPrefs.SetInt("ScreenMode", 3);
        }
    }
    public IEnumerator StartCooldown()
    {
        yield return new WaitForSeconds(1);
        GameObject obj = GameObject.Find("FreeLookCamera");
        obj.GetComponent<FollowCamera>().Assign();
    }

    public void Start_Host()
    {
        var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
        string IP = "";
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                host_IPadress_textbox.text =  "IP: " + ip.ToString();
                IP = ip.ToString();
            }
        }
        NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address = IP;
        NetworkManager.Singleton.StartHost(); StartCoroutine(StartCooldown());
        local_menu.SetActive(false);
    }

    public void Start_Client()
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address = input_ipAdress.text;       // The IP address is a string
        NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Port = 7777;                    // The port number is an unsigned short
        NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.ServerListenAddress = "7777"; // The server listen address is a string
        NetworkManager.Singleton.StartClient(); StartCoroutine(StartCooldown());
        local_menu.SetActive(false);
    }

    public void Choose_Tag()
    {
        game_selection_menu.SetActive(false);
        connection_menu.SetActive(true);
    }

    public void Select_Local()
    {
        local_menu.SetActive(true);
        connection_menu.SetActive(false);
    }
    public void Select_Online()
    {
        connectionState_text.enabled = true;
    }
    public void Back_LocalDedicated()
    {
        connection_menu.SetActive(false);
        game_selection_menu.SetActive(true);
    }
    public void Back_Local()
    {
        local_menu.SetActive(false);
        connection_menu.SetActive(true);
    }
    public void Pause_Game()
    {
        paused_menu.SetActive(true);
    }

    public void Continue_Game()
    {
        paused_menu.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void Disconnect()
    {
        paused_menu.SetActive(false);
        menu.SetActive(true);
        pause_button.gameObject.SetActive(false);
        jump_button.gameObject.SetActive(false);
        sprint_button.gameObject.SetActive(false);
        joystick.SetActive(false);
        score_1.enabled = false;
        score_2.enabled = false;
    }
}
