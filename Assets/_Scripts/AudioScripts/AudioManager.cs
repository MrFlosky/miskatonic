using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SFXType{CharacterSelection, ButtonSelection, SkillAOE, SkillBarrier, SkillRoomswap, FearIncrease, FearDecrease};
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


        }
    }
}
