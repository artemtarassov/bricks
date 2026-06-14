using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;
using DG.Tweening;

public class SoundsController : MonoBehaviour
{
    void Awake()
    {
        for (var i = 0; i < this.transform.childCount; i++)
        {
            var c = this.transform.GetChild(i);
            c.gameObject.SetActive(false);
        }
    }
    void Start()
    {
        SoundModel.Instance.OnPlaySound += OnPlaySound;
        SoundModel.Instance.OnStopSound += OnStopSound;
        new SoundCmd(SoundModel.Instance.MUSIC1).Run();
    }

    private AudioSource GetAudioSourceByName(string name)
    {
        for (var i = 0; i < this.transform.childCount; i++)
        {
            var c = this.transform.GetChild(i);
            if (c.name == name)
            {
                var audioSource = c.GetComponent<AudioSource>();
                Assert.IsNotNull(audioSource, "AudioSource component is missing on sound " + name);
                return audioSource;
            }
        }
        return null;
    }

    private void OnStopSound(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return;
        }
        Debug.Log("SoundsController: OnStopSound: " + name);
        var audioSource = GetAudioSourceByName(name);
        if (audioSource != null && audioSource.isPlaying)
        {
            var isMusic = name.Contains("music");
            if (isMusic)
            {
                //fade out music
                audioSource.DOKill();
                audioSource.DOFade(0, Durations.MusicFade).OnComplete(() =>
                {
                    audioSource.Stop();
                    audioSource.gameObject.SetActive(false);
                });
                return;
            }
            audioSource.Stop();
            audioSource.gameObject.SetActive(false);
        }
    }

    private void OnPlaySound(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return;
        }
        var audioSource = GetAudioSourceByName(name);
        if (audioSource != null)
        {
            audioSource.gameObject.SetActive(true);

            var isMusic = name.Contains("music");
            if (isMusic)
            {
                if (audioSource.isPlaying)
                {
                    return;
                }
                else
                {
                    //fade in music
                    audioSource.DOKill();
                    audioSource.volume = 0;
                    audioSource.Play();
                    audioSource.DOFade(1, Durations.MusicFade);
                }
                return;
            }
            audioSource.Play();
        }
    }

    public bool IsPlaying(string name)
    {
        var audioSource = GetAudioSourceByName(name);
        return audioSource != null && audioSource.isPlaying;
    }
    void OnDestroy()
    {
        SoundModel.Instance.OnPlaySound -= OnPlaySound;
        SoundModel.Instance.OnStopSound -= OnStopSound;
    }

}