using System.Collections.Generic;
using UnityEngine;

public class PersistentMusicPlayer : MonoBehaviour
{
    public List<AudioClip> musicTracks; // Assign your audio clips in the inspector
    private List<int> playedTracks; // To keep track of played songs
    private AudioSource audioSource;

    private static PersistentMusicPlayer instance = null; // Singleton instance to persist across scenes

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Don't destroy this object when loading new scenes
        }
        else if (instance != this)
        {
            Destroy(gameObject); // Ensure only one instance of the music player exists
        }
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (musicTracks.Count == 0)
        {
            Debug.LogWarning("No music tracks assigned!");
            return;
        }

        playedTracks = new List<int>();

        if (!audioSource.isPlaying)
        {
            PlayRandomTrack(); // Start playing a random track if not already playing
        }
    }

    void Update()
    {
        // If the song finished playing, play another random song
        if (!audioSource.isPlaying)
        {
            PlayRandomTrack();
        }
    }

    void PlayRandomTrack()
    {
        if (playedTracks.Count == musicTracks.Count)
        {
            playedTracks.Clear(); // All songs played, so reset for a new loop
        }

        int randomIndex;

        do
        {
            randomIndex = Random.Range(0, musicTracks.Count);
        }
        while (playedTracks.Contains(randomIndex)); // Ensure the song hasn't been played in this loop

        playedTracks.Add(randomIndex); // Mark the song as played
        audioSource.clip = musicTracks[randomIndex];
        audioSource.Play(); // Play the selected track
    }
}
