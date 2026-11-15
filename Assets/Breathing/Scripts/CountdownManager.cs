using UnityEngine;
using TMPro;

public class CountdownManager : MonoBehaviour
{
    public TextMeshProUGUI countdownText;
    public GameObject gameUI;

    private float countdown = 3f;
    private bool isCountingDown = true;

    void Start()
    {
        gameUI.SetActive(false);
    }

    void Update()
    {
        if (!isCountingDown) return;

        countdown -= Time.deltaTime;

        if (countdown > 0)
            countdownText.text = Mathf.CeilToInt(countdown).ToString();
        else
        {
            countdownText.text = "GO!";
            if (countdown < -0.5f)
            {
                isCountingDown = false;
                countdownText.gameObject.SetActive(false);
                gameUI.SetActive(true);
            }
        }
    }
}