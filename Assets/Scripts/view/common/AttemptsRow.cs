using TMPro;
using UnityEngine;

public class AttemptsRow : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private GameObject starsContainer;
    void Start()
    {

    }

    void OnEnable()
    {
        UpdateAttemptsLeft();
    }

    private void UpdateAttemptsLeft()
    {
        var left = 3;

        if (left == 0)
        {
            text.text = "No attempts left";
        }
        else if (left == 1)
        {
            text.text = "1 attempt left";
        }
        else
        {
            text.text = $"{left} attempts left";
        }

        for (var i = 0; i < starsContainer.transform.childCount; i++)
        {
            var c = starsContainer.transform.GetChild(i).gameObject.transform;
            var star = c.GetChild(0).gameObject;
            var emptyStar = c.GetChild(1).gameObject;

            star.SetActive(i < left);
            emptyStar.SetActive(i >= left);
        }
    }
}