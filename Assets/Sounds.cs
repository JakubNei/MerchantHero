using UnityEngine;

public static class Sounds
{
    public static class ID
    {
        public const string SwooshThrowingObject = "Sounds/SwooshThrowingObject";
    }
    public static void PlayAudio(string resourcePath, float randomPitchRange = 0f)
    {
        AudioClip clip = Resources.Load<AudioClip>(resourcePath);
        if (clip != null)
        {
            GameObject audioObject = new GameObject("Audio_" + clip.name);
            AudioSource audioSource = (AudioSource)audioObject.AddComponent(typeof(AudioSource));
            audioSource.clip = clip;
            audioSource.spatialBlend = 0f;
            audioSource.volume = 1;
            audioSource.loop = false;
            if (randomPitchRange > 0f)
                audioSource.pitch = 1f + Random.Range(-randomPitchRange, randomPitchRange);
            audioSource.Play();
            Object.Destroy(audioObject, clip.length * ((Time.timeScale < 0.01f) ? 0.01f : Time.timeScale));
        }
        else
        {
            Debug.LogWarning($"Audio clip not found at path: {resourcePath}");
        }
    }
    public static void PlayAudioAtLocation(Transform location, string resourcePath, float randomPitchRange = 0f)
    {
        AudioClip clip = Resources.Load<AudioClip>(resourcePath);
        if (clip != null)
        {
            AudioSource audioSource = (AudioSource)location.gameObject.AddComponent(typeof(AudioSource));
            audioSource.clip = clip;
            audioSource.spatialBlend = 1;
            audioSource.volume = 1;
            audioSource.loop = false;
            if (randomPitchRange > 0f)
                audioSource.pitch = 1f + Random.Range(-randomPitchRange, randomPitchRange);
            audioSource.Play();
            Object.Destroy(audioSource, clip.length * ((Time.timeScale < 0.01f) ? 0.01f : Time.timeScale));
        }
        else
        {
            Debug.LogWarning($"Audio clip not found at path: {resourcePath}");
        }
    }
}