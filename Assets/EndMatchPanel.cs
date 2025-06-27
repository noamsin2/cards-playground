using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndMatchPanel : MonoBehaviour
{
    [SerializeField] private GameObject victoryText;
    [SerializeField] private GameObject DefeatText;
    [SerializeField] private MatchPanel matchPanel;
    public event Action OnPlayerContinued;
    public void OnMatchEnded(bool isVictory)
    {
        Debug.Log("match ended");
        if (isVictory)
        {
            victoryText.SetActive(true);
            DefeatText.SetActive(false);
        }
        else
        {
            victoryText.SetActive(false);
            DefeatText.SetActive(true);
        }
        gameObject.SetActive(true);
        StartCoroutine(WaitForUserInput()); // wait for user to continue
    }
    private IEnumerator WaitForUserInput()
    {
        yield return new WaitUntil(() => Input.anyKeyDown || Input.GetMouseButtonDown(0));
        ContinueAfterMatch();
    }
    private void ContinueAfterMatch()
    {
        gameObject.SetActive(false);
        OnPlayerContinued?.Invoke();
    }
}
