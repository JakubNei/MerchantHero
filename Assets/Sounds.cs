using UnityEngine;

public enum SemanticMaterial
{
    Wood,
    Metal,
    Flesh,
    Stone,
    Leather,
    Glass,
}
public static class Sounds
{
    public static class ID
    {
        public const string SwooshThrowingObject = "Sounds/SwooshThrowingObject";
        public const string HitFlesh = "Sounds/Hit/Flesh";
        public const string CollectCoin = "Sounds/CollectCoin";
    }
    public static AudioSource PlayAudio(string resourcePath, float randomPitchRange = 0f)
    {
        return PlayAudio(resourcePath, 0f, randomPitchRange);
    }
    public static AudioSource PlayAudio(Vector3 location, string resourcePath, float randomPitchRange = 0f)
    {
        var audioSource = PlayAudio(resourcePath, 1f, randomPitchRange);
        audioSource.transform.position = location;
        return audioSource;
    }

    public static AudioSource PlayAudio(Transform location, string resourcePath, float randomPitchRange = 0f)
    {
        var audioSource = PlayAudio(resourcePath, 0f, randomPitchRange);
        audioSource.transform.parent = location;
        audioSource.transform.position = location.position;
        return audioSource;
    }

    static AudioSource PlayAudio(string resourcePath, float spatialBlend, float randomPitchRange)
    {
        AudioClip clip = Resources.Load<AudioClip>(resourcePath);
        if (clip == null)
        {
            Debug.LogWarning($"Audio clip not found at path: {resourcePath}");
            return null;
        }
        GameObject go = new GameObject("Audio_" + clip.name);
        AudioSource audioSource = (AudioSource)go.AddComponent(typeof(AudioSource));
        audioSource.clip = clip;
        audioSource.spatialBlend = 0f;
        audioSource.volume = 1;
        audioSource.loop = false;
        if (randomPitchRange > 0f)
            audioSource.pitch = 1f + Random.Range(-randomPitchRange, randomPitchRange);
        audioSource.Play();
        Object.Destroy(go, clip.length * ((Time.timeScale < 0.01f) ? 0.01f : Time.timeScale));
        return audioSource;
    }
}