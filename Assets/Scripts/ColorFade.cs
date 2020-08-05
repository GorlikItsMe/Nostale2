using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ColorFade : MonoBehaviour
{
    void Update()
    {
        StartCoroutine("Fade");
    }
    IEnumerator Fade()
    {
        Renderer renderer = GetComponent<Renderer>();

        for (float ft = 1f; ft >= 0; ft -= 0.1f)
        {
            Color c = renderer.material.color;
            c.a = ft;
            renderer.material.color = c;
            yield return null;
        }
    }
}
