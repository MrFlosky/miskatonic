using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// This script changes any UI element that it's attached to's color from black to white
public class ColorChange : MonoBehaviour
{
    [SerializeField] private float fadeSpeed = 1f;

    private Image image;
    private Text text;
    private Graphic graphic;

    private float t = 0f;
    private bool fadeColor = true;

    void Start()
    { //UI component
        graphic = GetComponent<Graphic>();

        if (graphic == null)
        {
            Debug.LogError("Color requires UI graphic element (Image or Text etc.)");
                enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (fadeColor)
        {
            t += Time.deltaTime * fadeSpeed;
            if (t >= 1f)
            {
                t = 1f;
                fadeColor = false;
            }
        }

        else
        {
            t -= Time.deltaTime * fadeSpeed;
            if (t <= 0f)
            {
                t = 0f;
                fadeColor = true;
            }
        }
        graphic.color = Color.Lerp(Color.black, Color.white, t); //fading between black and white
    }
}
