using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public float moveSpeed;
    public Button nextButton;
    public Button previousButton;
    public Button goButton;
    public RectTransform tutorialPage;
    private float targetPos = 0;

    // Start is called before the first frame update
    void Start()
    {
        previousButton.transform.gameObject.SetActive(true);
        previousButton.transform.gameObject.SetActive(true);
        goButton.transform.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(tutorialPage.position.x);
        if (Mathf.Approximately(tutorialPage.position.x, (960f/1920f)*Screen.width))
        {
            previousButton.transform.gameObject.SetActive(false);
            nextButton.transform.gameObject.SetActive(true);
        }
        else if(Mathf.Approximately(tutorialPage.position.x, (-3040/1920f)*Screen.width))
        {
            nextButton.transform.gameObject.SetActive(false);
            previousButton.transform.gameObject.SetActive(true);
            goButton.transform.gameObject.SetActive(true);
        }
        else
        {
            nextButton.transform.gameObject.SetActive(true);
            previousButton.transform.gameObject.SetActive(true);
            goButton.transform.gameObject.SetActive(false);
        }

    }

    public void MoveNext()
    {
        targetPos -= 2000;
        StartCoroutine(MoveCanvas(targetPos));
    }

    public void MovePrevious()
    {
        targetPos += 2000;
        StartCoroutine(MoveCanvas(targetPos));
    }

    private IEnumerator MoveCanvas(float targetPos)
    {
        while (Mathf.Abs(tutorialPage.anchoredPosition.x - targetPos) > 0.1f)
        {
            nextButton.transform.gameObject.SetActive(false);
            previousButton.transform.gameObject.SetActive(false);

            float newX = Mathf.MoveTowards(tutorialPage.anchoredPosition.x, targetPos, moveSpeed * Time.deltaTime);
            tutorialPage.anchoredPosition = new Vector2(newX, tutorialPage.anchoredPosition.y);

            yield return null;
        }
    }

}
