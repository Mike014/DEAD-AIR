using System.Collections;
using System;
using UnityEngine;

public class FadeSystem : MonoBehaviour
{
    public IEnumerator FadeOut(AudioSource source, float fadeTime, Action onComplete = null)
    {
        float startVolume = source.volume;

        while (source.volume > 0)
        {
            source.volume -= startVolume * Time.deltaTime / fadeTime;

            yield return null;
        }

        source.Stop();
        source.volume = startVolume;
        onComplete?.Invoke();
    }

    public IEnumerator FadeIn(AudioSource source, float targetVolume, float fadeTime, Action onComplete = null)
    {
        source.volume = 0f;

        while (source.volume < targetVolume)
        {
            source.volume += targetVolume * Time.deltaTime / fadeTime;
            yield return null;
        }
        source.volume = targetVolume;
        onComplete?.Invoke();
    }
}

