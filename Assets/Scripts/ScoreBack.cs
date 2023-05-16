using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreBack : MonoBehaviour
{
    [SerializeField] BoxCollider _boxCollider;

    private void Start()
    {
        if (_boxCollider != null)
        {
            _boxCollider.isTrigger = true;
        }
        else
        {
            Debug.LogWarning("No BoxCollider component found on this GameObject. Please add a BoxCollider component for this script to work.");
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (_boxCollider.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                OnObjectClicked();
            }
        }
    }

    private void OnObjectClicked()
    {
        SceneManager.LoadScene("Menu");
        // Add your custom method functionality here
    }
}
