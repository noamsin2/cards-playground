using System;
using UnityEngine;

public class OptionsPanel : MonoBehaviour
{
    [SerializeField] GameObject surrenderButton;
    public event Action OnSurrender;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ToggleOptionsPanel()
    {
        // Toggle the visibility of the Settings Panel
            bool isActive = gameObject.activeSelf;
            gameObject.SetActive(!isActive);

        if(MatchManager.Instance.CurrentMatch != null)
        {
            surrenderButton.SetActive(true);
        }
        else
        {
            surrenderButton.SetActive(false);
        }
    }
    public void Surrender()
    {
        OnSurrender?.Invoke();
    }
}
