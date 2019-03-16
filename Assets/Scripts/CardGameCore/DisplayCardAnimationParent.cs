using System.Collections;
using System.Collections.Generic;
using FRG.Taco;
using UnityEngine;

public class DisplayCardAnimationParent : MonoBehaviour
{

    [SerializeField] private AnimationClip _bustAnimationClip;
    [SerializeField] private AnimationClipPlayer _clipPlayer;

    public void PlayBust()
    {
        _clipPlayer.PlayClip(_bustAnimationClip, 1);
    }
}
