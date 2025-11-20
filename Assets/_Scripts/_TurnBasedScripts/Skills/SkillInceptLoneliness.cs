using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillInceptLoneliness : Skill
{
    public Camera mainCamera;
    public int fearPerNearby = 2;
    public int lonelyRadius = 20;


    protected override void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main; // Use main camera if none specified
        }
    }

    public override void ActivateSkill()
    {
        foreach (var player in FindObjectsOfType<PlayerVision>())
        {
            if (player != null)
            {
                player.EnableInceptLoneliness(true, fearPerNearby, lonelyRadius);
            }
        }
    }





}
