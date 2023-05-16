using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Mobile : MonoBehaviour
{
    private bool sprinting = false;
    [SerializeField] GameObject[] PCobjects;
    [SerializeField] GameObject[] MobileObjects;
    [SerializeField] Toggle lowspecToggle;
    public UnityEvent onValueChanged;

    private void Start()
    {
        LoadToggleValue();

        lowspecToggle.onValueChanged.AddListener(UpdateGraphics);
        lowspecToggle.onValueChanged.AddListener(SaveToggleValue);
        UpdateGraphics(lowspecToggle.isOn);
    }

    private void OnDestroy()
    {
        if (lowspecToggle != null)
        {
            lowspecToggle.onValueChanged.RemoveListener(UpdateGraphics);
            lowspecToggle.onValueChanged.RemoveListener(SaveToggleValue);
        }
    }

    private void LoadToggleValue()
    {
        if (PlayerPrefs.HasKey("lowspecToggle"))
        {
            lowspecToggle.isOn = PlayerPrefs.GetInt("lowspecToggle") == 1;
        }
    }

    private void SaveToggleValue(bool value)
    {
        PlayerPrefs.SetInt("lowspecToggle", value ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void UpdateGraphics(bool value)
    {
        foreach (var obj in MobileObjects)
        {
            obj.SetActive(value);
        }
        foreach (var obj in PCobjects)
        {
            obj.SetActive(!value);
        }
    }
    private void changeValue()
    {
        GameObject[] tPlayer = GameObject.FindGameObjectsWithTag("Player");
        GameObject myPlayer = null;
        foreach (var player in tPlayer)
        {
            if (player.GetComponent<Owner>() != null)
                myPlayer = player;
        }
        myPlayer.GetComponent<NetworkMovement>().CheckForWallPart2Mobile();
    }

    public void Jump()
    {
        GameObject[] tPlayer = GameObject.FindGameObjectsWithTag("Player");
        GameObject myPlayer = null;
        foreach (var player in tPlayer)
        {
            if (player.GetComponent<Owner>() != null)
                myPlayer = player;
        }
        myPlayer.GetComponent<NetworkMovement>().Jump();
    }

    public void DashLeft()
    {
        GameObject[] tPlayer = GameObject.FindGameObjectsWithTag("Player");
        GameObject myPlayer = null;
        foreach (var player in tPlayer)
        {
            if (player.GetComponent<Owner>() != null)
                myPlayer = player;
        }
        myPlayer.GetComponent<NetworkMovement>().Dodge(true);
    }

    public void DashRight()
    {
        GameObject[] tPlayer = GameObject.FindGameObjectsWithTag("Player");
        GameObject myPlayer = null;
        foreach (var player in tPlayer)
        {
            if (player.GetComponent<Owner>() != null)
                myPlayer = player;
        }
        myPlayer.GetComponent<NetworkMovement>().Dodge(false);
    }

    public void Sprint()
    {
        if (!sprinting)
        {
            Enable_sprint();
            sprinting = true;
        }
        else
        {
            Disable_sprint();
            sprinting = false;
        }
    }

    public void Disable_sprint()
    {
        GameObject[] tPlayer = GameObject.FindGameObjectsWithTag("Player");
        GameObject myPlayer = null;
        foreach (var player in tPlayer)
        {
            if (player.GetComponent<Owner>() != null)
                myPlayer = player;
        }
        myPlayer.GetComponent<NetworkMovement>().disable_sprint();
    }
    public void Enable_sprint()
    {
        GameObject[] tPlayer = GameObject.FindGameObjectsWithTag("Player");
        GameObject myPlayer = null;
        foreach (var player in tPlayer)
        {
            if (player.GetComponent<Owner>() != null)
                myPlayer = player;
        }
        myPlayer.GetComponent<NetworkMovement>().enable_sprint();
    }
}
