using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CountdownUI : MonoBehaviour
{
    private const string NUMBER_POPUP = "NumberPopup";

    [SerializeField] private Transform playerTransform;
    [SerializeField] private TextMeshProUGUI countdownText;
    private Vector3 originalScale;

    private Animator animator;

    public float timeRemaining = 3f;
    public bool timerIsRunning = false;
    private float initialTime; // Store the initial countdown time to reset it later

    public delegate void CountdownFinished ();
    public event CountdownFinished OnCountdownFinished;



    private void Awake ()
    {
        animator = GetComponent<Animator>();

    }

    private void Start ()
    {
        originalScale = transform.localScale;
        initialTime = timeRemaining;
        Hide();
    }

    private void Update ()
    {
        if (timerIsRunning)
            RunTimer();

        if (playerTransform != null)
            FlipCanvasAccordingToPlayer();
    }

    private void FlipCanvasAccordingToPlayer ()
    {
        // Flip the canvas based on the player's orientation
        if (playerTransform.localScale.x < 0)
        {
            transform.localScale = new Vector3(-originalScale.x, originalScale.y, originalScale.z);
        }
        else
        {
            transform.localScale = originalScale;
        }
    }

    private void RunTimer ()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            DisplayTime(timeRemaining);
        }
        else
        {
            StopTimer();
            OnCountdownFinished?.Invoke();
        }
    }

    public void StartTimer ()
    {
        Show();
        timerIsRunning = true;
    }

    public void StopTimer ()
    {
        timerIsRunning = false;
        Hide();
        timeRemaining = initialTime;
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