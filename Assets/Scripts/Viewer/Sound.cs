using UnityEngine;

public class Sound : MonoBehaviour
{
    public static AudioClip RandomAudioFromArray(AudioClip[] array) => array[Random.Range(0, array.Length)];
    public static void PlaySound(AudioSource source, AudioClip[] array)
    {
        if (source != null && array.Length > 0)
        {
            source.Stop();
            source.clip = RandomAudioFromArray(array);
            source.Play();
        }
    }
}
