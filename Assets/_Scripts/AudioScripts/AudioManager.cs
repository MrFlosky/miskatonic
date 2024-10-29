using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.VFX;

public enum SFXType{CharacterSelection, ButtonSelection, SkillAOE, SkillBarrier, SkillRoomswap, FearIncrease, FearDecrease, heartSound, MouseButtonHit};
public class AudioManager : MonoBehaviour
{
    private static AudioManager S;
    

    void Awake()
    {
        S = this;
        S.enabled = false;
    }
    public AudioSource characterSelect;
    public AudioSource buttonSelect;
    public AudioSource aoe;
    public AudioSource barrier;
    public AudioSource roomSwap;
    public AudioSource fearIncrease;
    public AudioSource fearDecrease;
    public AudioSource heartSound;
    public AudioSource mouseButtonHit;


    public AudioSource bgmMusic;

    float timeElapsed = 0f;
    private float transitionSpeed = 1.8f; // bigger the val the faster
    float start = 0f;
    float end = 0f;
    void Update()
    {
        timeElapsed += Time.deltaTime;
        bgmMusic.volume = Mathf.Lerp(start, end, timeElapsed * transitionSpeed);

        if(bgmMusic.volume == end)
        {
            enabled = false;
        }
    }

    static public void SmoothVolumeTransition(float startVol, float endVol)
    {
        S.timeElapsed = 0f;
        S.start = startVol;
        S.end = endVol;
        S.enabled = true;
        
    }
    static public void PlaySFX(SFXType clip)
    {
        switch(clip)
        {
            case SFXType.CharacterSelection:
            S.characterSelect.Play();
            break;

            case SFXType.ButtonSelection:
            S.buttonSelect.Play();
            break;

            case SFXType.SkillAOE:
            S.aoe.Play();
            break;

            case SFXType.SkillBarrier:
            S.barrier.Play();
            break;

            case SFXType.SkillRoomswap:
            S.roomSwap.Play();
            break;

            case SFXType.FearIncrease:
            S.fearIncrease.Play();
            break;

            case SFXType.FearDecrease:
            S.fearDecrease.Play();
            break;
            
            case SFXType.MouseButtonHit:
            S.mouseButtonHit.Play();
            break;


        }
    }


    // note these not derived from mono
    public void UpdateMusicVolume(Slider slider)
    {
        if(S.heartSound.isPlaying)
        {
            bgmMusic.volume = slider.value / 2.5f;
        }
        else
        {
            bgmMusic.volume = slider.value;
        }
        Heartbeats.mainThemeCurr = slider.value;
    }
    
    public void UpdateSfxVolume(Slider slider)
    {
        characterSelect.volume = slider.value;
        buttonSelect.volume = slider.value;
        aoe.volume = slider.value;
        barrier.volume = slider.value;
        roomSwap.volume = slider.value;
        fearIncrease.volume = slider.value;
        fearDecrease.volume = slider.value;
        mouseButtonHit.volume = slider.value;
    }

    public void UpdateHBVolume(Slider slider)
    {
        heartSound.volume = slider.value;
    }
}
