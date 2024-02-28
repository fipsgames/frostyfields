using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasFader : MonoBehaviour
{
    public float duration = 0.4f;

    public void Fade(bool active, float delay = 0f){
        var canvasGroup = GetComponent<CanvasGroup>();
        ////Logger.Log("fading canvas group: " + active, this, true);
        if (active) this.gameObject.SetActive(active);
        StartCoroutine(DoFade(canvasGroup, canvasGroup.alpha, active ? 1 : 0, active, delay));
    }

    public IEnumerator DoFade(CanvasGroup canvasGroup, float start, float end, bool active, float delay = 0f){
        float counter = 0f;
        while(counter < delay)
        {
            counter += Time.deltaTime; 
            yield return null;
        }
        counter = 0f;
        while (counter < duration){
            counter += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, end, counter / duration);
            yield return null;
        }
        if (!active) this.gameObject.SetActive(active);
    }
}
