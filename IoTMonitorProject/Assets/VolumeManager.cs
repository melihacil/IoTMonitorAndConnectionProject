using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumeManager : MonoBehaviour
{
    static int STREAMMUSIC;
    static int FLAGSHOWUI = 1;

    private static AndroidJavaObject audioManager;

    private static AndroidJavaObject deviceAudio
    {
        get
        {
            if (audioManager == null)
            {
                AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject currentActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject context = currentActivity.Call<AndroidJavaObject>("getApplicationContext");
                AndroidJavaClass audioManagerClass = new AndroidJavaClass("android.media.AudioManager");
                AndroidJavaClass contextClass = new AndroidJavaClass("android.content.Context");

                STREAMMUSIC = audioManagerClass.GetStatic<int>("STREAM_MUSIC");
                string Context_AUDIO_SERVICE = contextClass.GetStatic<string>("AUDIO_SERVICE");

                audioManager = context.Call<AndroidJavaObject>("getSystemService", Context_AUDIO_SERVICE);

                if (audioManager != null)
                    Debug.Log("[AndroidNativeVolumeService] Android Audio Manager successfully set up");
                else
                    Debug.Log("[AndroidNativeVolumeService] Could not read Audio Manager");
            }
            return audioManager;
        }

    }

    private static int GetDeviceMaxVolume()
    {
        return deviceAudio.Call<int>("getStreamMaxVolume", STREAMMUSIC);
    }

    public float GetSystemVolume()
    {
        int deviceVolume = deviceAudio.Call<int>("getStreamVolume", STREAMMUSIC);
        float scaledVolume = (float)(deviceVolume / (float)GetDeviceMaxVolume());

        return scaledVolume;
    }

    public void SetSystemVolume(float volumeValue)
    {
        int scaledVolume = (int)(volumeValue * (float)GetDeviceMaxVolume());
        deviceAudio.Call("setStreamVolume", STREAMMUSIC, scaledVolume, FLAGSHOWUI);
    }
}
