using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FW
{
    public enum SoundType
    {
        ENEMY,PLAYER,ITEM
    }
    public interface ISoundListener
    {
        public void OnHearSound(ISoundSender source);
    }

    public interface ISoundSender
    {
        public GameObject SoundGameObject { get; }
        public SoundType SoundSourceType { get; }

        public float SoundRange { get; }
         public void SendSound();
    }
}


