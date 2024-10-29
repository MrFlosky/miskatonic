using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public enum SFXType{CharacterSelection, ButtonSelection, SkillAOE, SkillBarrier, SkillRoomswap, FearIncrease, FearDecrease, heartSound, MouseButtonHit};
public class AudioManager : MonoBehaviour
{
    private static AudioManager S;
    

    void Awake()
    {
        S = this;
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

    public void UpdateMusicVolume(Slider slider)
    {
        bgmMusic.volume = slider.value;
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
        heartSound.volume = slider.value;
        mouseButtonHit.volume = slider.value;
    }
}
