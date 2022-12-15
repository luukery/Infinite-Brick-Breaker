using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// The script for the health blocks. The only thing this really does is putting on a randomized texture on the health places and fade in.
/// The fadeOut is done in <see cref="GameManager.removeLife"/>
/// The destruction is done in another script, because there all the health blocks are stored
/// and you can 
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class PhysicalHealth : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<SpriteRenderer>().sprite = GameManager.instance.healthTextures[Random.Range(0, GameManager.instance.healthTextures.Length)];
        GameManager.instance.lives.Add(this);
        StartCoroutine(FadeIn());
    }


    IEnumerator FadeIn()
    {
        yield return null;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color temp = new Color(1, 1, 1, 1);

        float elapsedTime = 0;
        float duration = 0.2f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float newAlpha = Mathf.Lerp(0, 1, elapsedTime / duration);
            sr.color = new Color(temp.r, temp.g, temp.b, newAlpha);

            yield return null;
        }
    }
}
