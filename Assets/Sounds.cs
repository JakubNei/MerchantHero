using UnityEngine;

public static class Sounds
{
    public static class ID
    {
        public const string SwooshThrowingObject = "Sounds/SwooshThrowingObject";
        public const string HitFlesh = "Sounds/Hit/Flesh";
        public const string CollectCoin = "Sounds/CollectCoin";
    }
    public static void PlayAudio(string resourcePath, float randomPitchRange = 0f)
    {
        PlayAudio(resourcePath, 0f, randomPitchRange);
    }
    public static void PlayAudio(Vector3 location, string resourcePath, float randomPitchRange = 0f)
    {
        var go = PlayAudio(resourcePath, 1f, randomPitchRange);
        go.transform.position = location;
    }

    public static void PlayAudio(Transform location, string resourcePath, float randomPitchRange = 0f)
    {
        var go = PlayAudio(resourcePath, 0f, randomPitchRange);
        go.transform.parent = location;
        go.transform.position = location.position   ;
    }

    static GameObject PlayAudio(string resourcePath, float spatialBlend, float randomPitchRange)
    {
        AudioClip clip = Resources.Load<AudioClip>(resourcePath);
        if (clip == null)
        {
            Debug.LogWarning($"Audio clip not found at path: {resourcePath}");
            return null;
        }
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
        return audioObject;
    }
}