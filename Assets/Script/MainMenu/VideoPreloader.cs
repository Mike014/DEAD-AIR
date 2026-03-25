using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoPreloader : MonoBehaviour
{
    private VideoPlayer videoPlayer;
    
    void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.Prepare();
    }
    
    void Update()
    {
        // Quando il video è preparato, inizia a riprodurre
        if (videoPlayer.isPrepared && !videoPlayer.isPlaying)
        {
            videoPlayer.Play();
        }
    }
}
