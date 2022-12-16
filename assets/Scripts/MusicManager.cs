using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    // instance
    public static MusicManager Instance;

    public string currentTrack { get; private set; } = "start";
    public bool currentlyPlaying { get; private set; } = false;

    private AudioSource source;
    private SceneMusicManager sceneMusic;

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        source = GetComponent<AudioSource>();
    }

    private void Start() {
        sceneMusic = GameObject.Find("SceneMusicManager").GetComponent<SceneMusicManager>();
        source.clip = sceneMusic.GetCurrentMusic();
        source.Play();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        source = GetComponent<AudioSource>();
        sceneMusic = GameObject.Find("SceneMusicManager").GetComponent<SceneMusicManager>();
        AudioClip curr = sceneMusic.GetCurrentMusic();
        if (curr.name != currentTrack) {
            source.Stop();
            source.clip = curr;
            currentTrack = curr.name;
            source.Play();
        }
    }

    private void Update()
    {
        AudioClip curr = sceneMusic.GetCurrentMusic();
        if (curr.name != currentTrack) {
            source.Stop();
            source.clip = curr;
            currentTrack = curr.name;
            source.Play();
        }
    }
}
