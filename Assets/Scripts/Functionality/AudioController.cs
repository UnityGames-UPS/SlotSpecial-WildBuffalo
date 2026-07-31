using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AudioController : MonoBehaviour
{
    [SerializeField] private AudioSource bg_adudio;
    [SerializeField] internal AudioSource audioPlayer_wl;
    [SerializeField] internal AudioSource audioPlayer_button;
    [SerializeField] internal AudioSource audioSpin_button;
    [SerializeField] private AudioClip[] clips;
    [SerializeField] private AudioClip[] Bonusclips;
    [SerializeField] private AudioSource bg_audioBonus;
    [SerializeField] private AudioSource audioPlayer_Boost;
    [SerializeField] private AudioSource audioPlayer_Bonus;
    bool ISMusic;

    private readonly Dictionary<AudioSource, bool> preFocusMuteState = new Dictionary<AudioSource, bool>();
    private bool isForceMuted = false;

    private void Start()
    {
        if (bg_adudio) bg_adudio.Play();
        audioPlayer_button.clip = clips[clips.Length - 2];
        audioSpin_button.clip = clips[clips.Length - 3];
        audioPlayer_Boost.clip = clips[clips.Length - 1];
    }

    // Focus-driven mute-all — called from BOTH UIManager.OnFocusChanged (JS bridge)
    // and SlotBehaviour.OnApplicationFocus (native path). Reentrancy-guarded so a
    // duplicate call for the same direction can't clobber the captured restore state.
    internal void SetMuteAll(bool forceMute)
    {
        if (forceMute == isForceMuted) return;
        isForceMuted = forceMute;

        AudioSource[] sources = { bg_adudio, audioPlayer_wl, audioPlayer_button, audioSpin_button, bg_audioBonus, audioPlayer_Boost, audioPlayer_Bonus };
        foreach (var source in sources)
        {
            if (source == null) continue;
            if (forceMute)
            {
                preFocusMuteState[source] = source.mute;
                source.mute = true;
            }
            else
            {
                source.mute = preFocusMuteState.TryGetValue(source, out bool prevMuted) ? prevMuted : source.mute;
            }
        }
    }

    private void SetSourceMute(AudioSource source, bool mute)
    {
        if (source == null) return;
        source.mute = mute;
        if (isForceMuted) preFocusMuteState[source] = mute;
    }

    internal void SwitchBGSound(bool isbonus)
    {
        if (isbonus)
        {
            if (bg_audioBonus) bg_audioBonus.enabled = true;
            if (bg_adudio) bg_adudio.enabled = false;
        }
        else
        {
            if (bg_audioBonus) bg_audioBonus.enabled = false;
            if (bg_adudio) bg_adudio.enabled = true;
        }
    }

    internal void PlayWLAudio(string type)
    {
        audioPlayer_wl.loop = false;
        int index = 0;
        switch (type)
        {
            case "spin":
                index = 0;
                audioPlayer_wl.loop = true;
                break;
            case "win":
                index = 1;
                break;
            case "lose":
                index = 2;
                break;
            case "spinStop":
                index = 3;
                break;
            case "megaWin":
                index = 4;
                break;
        }
        StopWLAaudio();
        audioPlayer_wl.clip = clips[index];
        audioPlayer_wl.Play();

    }

    internal void PlayBonusAudio(string type)
    {
        audioPlayer_wl.loop = false;
        int index = 0;
        switch (type)
        {
            case "win":
                index = 0;
                break;
            case "lose":
                index = 1;
                break;
            case "cycleSpin":
                index = 2;
                break;
        }
        StopBonusAaudio();
        audioPlayer_Bonus.clip = Bonusclips[index];
        audioPlayer_Bonus.Play();

    }

    internal void PlayButtonAudio()
    {
        audioPlayer_button.Play();
    }

    internal void PlayBoostSpinAudio()
    {

        audioPlayer_Boost.Play();

    }

    internal void PlaySpinButtonAudio()
    {
        audioSpin_button.Play();
    }

    internal void StopWLAaudio()
    {
        audioPlayer_wl.Stop();
        audioPlayer_wl.loop = false;
    }

    internal void StopBonusAaudio()
    {
        audioPlayer_Bonus.Stop();
        audioPlayer_Bonus.loop = false;
    }

    internal void StopBgAudio()
    {
        bg_adudio.Stop();
    }

    internal void ToggleMute(bool toggle, string type = "all")
    {
        switch (type)
        {
            case "bg":
                SetSourceMute(bg_adudio, toggle);
                SetSourceMute(bg_audioBonus, toggle);
                break;
            case "button":
                SetSourceMute(audioPlayer_button, toggle);
                SetSourceMute(audioSpin_button, toggle);
                break;
            case "wl":
                SetSourceMute(audioPlayer_wl, toggle);
                SetSourceMute(audioPlayer_Bonus, toggle);
                SetSourceMute(audioPlayer_Boost, toggle);
                break;
            case "all":
                SetSourceMute(audioPlayer_wl, toggle);
                SetSourceMute(bg_adudio, toggle);
                SetSourceMute(audioPlayer_button, toggle);
                SetSourceMute(audioSpin_button, toggle);
                SetSourceMute(audioPlayer_Boost, toggle);
                // ISMusic = toggle;
                break;
        }
    }

}
