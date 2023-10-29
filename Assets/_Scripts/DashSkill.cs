using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashSkill : MonoBehaviour
{
    private const float DOUBLE_CLICK_TIME = .2f;

    [SerializeField] float dashSpeed = 10f;
    [SerializeField] float dashDuration = 1f;

    private bool isDashing;

    private Rigidbody2D rb;


    private int lastTapDirection = 0; // -1 for left, 1 for right


    private float lastClickTime;

    private void Start ()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update ()
    {
        if (!isDashing)
        {
            int currentTapDirection = 0;

            if (Input.GetButtonDown("Horizontal"))
            {
                currentTapDirection = (int)Mathf.Sign(Input.GetAxisRaw("Horizontal"));
            }

            if (currentTapDirection != 0)
            {
                float timeSinceLastClick = Time.time - lastClickTime;

                if (timeSinceLastClick <= DOUBLE_CLICK_TIME && currentTapDirection == lastTapDirection)
                {
                    Dash(currentTapDirection);
                    lastClickTime = 0;
                }
                else
                {
                    lastClickTime = Time.time;
                    lastTapDirection = currentTapDirection;
                }
            }
        }
    }


    private void Dash ( int direction )
    {
        isDashing = true; // Dash started
        Vector2 startDashPosition = rb.position;
        Vector2 targetDashPosition = new Vector2(rb.position.x + (dashSpeed * direction), rb.position.y);
        StartCoroutine(DashMovement(startDashPosition, targetDashPosition, dashDuration));
    }


    private IEnumerator DashMovement ( Vector2 start, Vector2 end, float duration )
    {
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = Mathf.Clamp(elapsed / duration, 0, 1); 
            rb.position = Vector2.Lerp(start, end, normalizedTime); // Interpolate position based on elapsed time
            yield return null; // Wait for the next frame
        }

        rb.position = end; 
        isDashing = false;
    }




}
