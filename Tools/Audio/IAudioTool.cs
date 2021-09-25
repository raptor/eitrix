using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;

namespace Eitrix
{
    public interface IAudioTool
    {
        void LoadContent(ContentManager Content);
        void PlaySound(SoundEffectType type);
        void PlaySound(SoundEffectType type, float volume, float pitch, float delaySeconds);
        void StopSound(SoundEffectType type);
        void StopMusic();
        void SkipToNextBackgroundMusic();
        void ResolveState();
        void SetMusicVolume(float volume);
        string CurrentSongName { get; }

    }
}
