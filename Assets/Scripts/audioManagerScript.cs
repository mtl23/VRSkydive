using UnityEngine;
using System.Collections;

public class audioManagerScript : MonoBehaviour {

    public AudioSource BackGroundMusic;
    public AudioSource fxSource;
    public static audioManagerScript instance = null;
    public AudioClip music;

    float lowPitchRange = .95f;
    float highPitchRange = 1.05f;
	// Use this for initialization
	void Start () {
        BackGroundMusic.clip = music;
        BackGroundMusic.Play(0);
    }

    void Awake()
    {

        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
	// Update is called once per frame
	 public void ChangeBackgroundMusic(AudioClip music)
    {
        BackGroundMusic.Stop();
        BackGroundMusic.clip = music;
    }
    public void PauseBackGroundMusic()
    {
        BackGroundMusic.Pause();
    }
    public void PlayBackgroundMusic()
    {
        BackGroundMusic.Play();
    }
	public void adjustMusicVolume(int command)
    {
		if (command == 1)
			BackGroundMusic.volume = fxSource.volume + 0.5f;
		else
			BackGroundMusic.volume = fxSource.volume - 0.5f;
    }
	public void ajustEffectsVolume(int command)
    {
		if (command == 1)
			BackGroundMusic.volume = BackGroundMusic.volume + 0.5f;
		else
			BackGroundMusic.volume = BackGroundMusic.volume - 0.5f; 
    }
    public void playSound(AudioClip soundClip)
    {
        float randomPitch = Random.Range(lowPitchRange, highPitchRange);
        fxSource.pitch = randomPitch;
        fxSource.clip = soundClip;
        fxSource.Play();
    }
    public void playSoundNoChange(AudioClip soundClip)
    {
        float randomPitch = Random.Range(lowPitchRange, highPitchRange);
        fxSource.pitch = randomPitch;
        fxSource.clip = soundClip;
        fxSource.Play();
    }

}
