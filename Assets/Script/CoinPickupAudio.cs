using UnityEngine;

public static class CoinPickupAudio
{
    private const string ClipResourceName = "coin_pickup";
    private const float Volume = 0.85f;

    private static AudioClip pickupClip;

    // Plays coin pickup feedback from a temporary object so destroyed coins do not cut it off.
    public static void Play()
    {
        if (pickupClip == null)
        {
            pickupClip = Resources.Load<AudioClip>(ClipResourceName);
        }

        if (pickupClip == null)
        {
            return;
        }

        GameObject soundObject = new GameObject("CoinPickupSound");
        AudioSource audioSource = soundObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.PlayOneShot(pickupClip, Volume);

        Object.Destroy(soundObject, pickupClip.length + 0.1f);
    }
}
