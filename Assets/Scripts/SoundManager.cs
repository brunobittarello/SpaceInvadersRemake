using UnityEngine;

class SoundManager : MonoBehaviour
{
    internal static SoundManager instance;

    [Header("Audio Clips")]
    public AudioClip clipCannonShot;
    public AudioClip clipCannonExplosion;
    public AudioClip clipAlienExplosion;
    public AudioClip clipMystery;
    public AudioClip clipMysteryExplosion;
    public AudioClip[] clipsFleet;

    [Header("Audio Source")]
    public AudioSource audioFleet;
    public AudioSource audioCannon;
    public AudioSource audioMystery;

    int currentFleetClip;

    private void Awake()
    {
        instance = this;
    }

    internal void NextFleetSound()
    {
        audioFleet.PlayOneShot(clipsFleet[currentFleetClip]);
        currentFleetClip++;
        if (currentFleetClip == clipsFleet.Length)
            currentFleetClip = 0;
    }

    internal void CannonShot() => audioCannon.PlayOneShot(clipCannonShot);
    internal void CannonExplosion() => audioCannon.PlayOneShot(clipCannonExplosion);
    internal void AlienExplosion() => audioFleet.PlayOneShot(clipAlienExplosion);
    internal void Mystery(bool play)
    {
        if (play == false)
        {
            audioMystery.Stop();
            return;
        }
        audioMystery.clip = clipMystery;
        audioMystery.loop = true;
        audioMystery.Play();
    }
    internal void MysteryExplosion()
    {
        audioMystery.loop = false;
        audioMystery.PlayOneShot(clipMysteryExplosion);
        audioMystery.PlayOneShot(clipMysteryExplosion);
    }

    internal void PauseResumeMystery(bool resume) {
        if (resume) audioMystery.UnPause();
        else audioMystery.Pause();
    }
}
