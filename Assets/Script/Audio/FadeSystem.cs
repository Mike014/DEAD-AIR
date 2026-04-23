using System.Collections;
using UnityEngine;

public class FadeSystem : MonoBehaviour
{
    public IEnumerator FadeOut(AudioSource source, float fadeTime)
    {
        float startVolume = source.volume;

        while (source.volume > 0)
        {
            source.volume -= startVolume * Time.deltaTime / fadeTime;

            yield return null;
        }

        source.Stop();
        source.volume = startVolume;
    }
}

/*
StartCoroutine (AudioFadeOut.FadeOut (sound_open, 0.1f));

//or:

public AudioSource Sound1;

IEnumerator fadeSound1 = AudioFadeOut.FadeOut (Sound1, 0.5f);
StartCoroutine (fadeSound1);
StopCoroutine (fadeSound1);

*/