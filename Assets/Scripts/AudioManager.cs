using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
	public AudioSource SFX;
	public AudioSource death;
	public AudioSource[] music = new AudioSource[2];
	public AudioClip[] clips;
	public AudioClip musicTrack;
	public int currentTrack;
	public float positionInSeconds;
	public bool musicPlaying;


	void Update()
	{
		if (musicPlaying)
		{
			if(music[currentTrack].time >= positionInSeconds)
			{
				//music[currentTrack].volume = 0;
				//StartCoroutine(StopPlaying(music[currentTrack]));
				currentTrack = 1 - currentTrack;
				music[currentTrack].clip = musicTrack;
				music[currentTrack].Play();
				musicPlaying = true;
			}
		}
	}

	IEnumerator StopPlaying(AudioSource source)
	{
		yield return null;

		source.Stop();
		source.volume = 1;
	}

	public void PlayAudio(AudioClip clip)
	{
		SFX.clip = clip;
		SFX.Play();
	}

	public void PlayAudio(AudioClip[] clips)
	{
		if(clips.Length > 0)
		{
			SFX.clip = clips[Random.Range(0, clips.Length)];
			SFX.Play();
		}
	}

	public void PlayAudioDeath(AudioClip[] clips)
	{
		if (clips.Length > 0)
		{
			death.clip = clips[Random.Range(0, clips.Length)];
			death.Play();
		}
	}

	public void PlayMusic()
	{
		music[currentTrack].clip = musicTrack;
		music[currentTrack].Play();
		musicPlaying = true;
	}
}