using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CountdownUI : MonoBehaviour
{
    private const string NUMBER_POPUP = "NumberPopup";


    [SerializeField] private TextMeshProUGUI countdownText;

    private Animator animator;

    public float timeRemaining = 3f;
    public bool timerIsRunning = false;
    private float initialTime; // Store the initial countdown time to reset it later




    private void Awake ()
    {
        animator = GetComponent<Animator>();
    }

    private void Start ()
    {
        initialTime = timeRemaining;
        Hide();
    }

    private void Update ()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);
            }
            else
            {
                timerIsRunning = false;
                Hide();
                timeRemaining = initialTime;
            }
        }
    }

    public void StartTimer ()
    {
        Show();
        timerIsRunning = true;
    }


    void DisplayTime ( float timeToDisplay )
    {
        timeToDisplay += 1;

        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        countdownText.text = string.Format("{0}", seconds);
        animator.SetTrigger(NUMBER_POPUP);
    }


    private void Show ()
    {
        gameObject.SetActive(true);
    }

    private void Hide ()
    {
        gameObject.SetActive(false);
    }

}