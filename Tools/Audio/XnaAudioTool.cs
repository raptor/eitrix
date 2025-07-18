using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using System.Diagnostics;
using System.IO;

namespace Eitrix
{
    /// ---------------------------------------------------------
    /// <summary>
    /// Identify our sound effects
    /// </summary>
    /// ---------------------------------------------------------
    public enum SoundEffectType
    {
        Attack00,
        Attack01,
        Attack02,
        Attack03,
        Attack04,
        Attack05,
        Attack06,
        Attack07,
        Attack08,
        Attack09,
        Attack10_Yell,
        Attack11_Trombone,
        Attack12_haho,
        Attack13_CrazyLaugh,
        Bump,
        Cheer,
        Clear01,
        Clear1Line,
        Clear2Lines,
        Clear3Lines,
        Clear4Lines,
        CrowdAww,
        CymbalLong,
        CymbalShort,
        Dot,
        Dot01,
        Dot02,
        Dot03,
        GameStart,
        Slowdown,
        Smack00,
        Smack01,
        Smack02,
        Smack03a,
        Smack03b,
        Smack03c,
        Smack04a,
        Smack04b,
        Smack04c,
        Smack05,
        Smack06_Cymbal,
        Smack07_Slam,
        Speedup,
        Strum,
        Trans00,
        Trans01,
        Trans02_GuitarSlide,
        Trans03_Chimes,
        Wooahh00,
        Wooahh01,
        Wooahh02_Chior_oohs,



        // This goes at the end
        NumberOfSoundEffects,
    }

    /// ---------------------------------------------------------
    /// <summary>
    /// Xna implementation of IAudioTool
    /// </summary>
    /// ---------------------------------------------------------
    public class XnaAudioTool : IAudioTool
    {
        List<QueuedSoundEffect> effectsToPlay = new List<QueuedSoundEffect>();
        List<SoundEffect> soundEffects = new List<SoundEffect>();
        List<SoundEffect> activeSoundEffects = new List<SoundEffect>(); // matches sound effects to the sound instances
        List<SoundEffectInstance> soundInstances = new List<SoundEffectInstance>();
        List<SongData> backgroundMusicList = new List<SongData>();
        int currentSong = 0;

        public string CurrentSongName
        {
            get { return Path.GetFileName( backgroundMusicList[currentSong].Content.Name); }
        }

        /// ---------------------------------------------------------
        /// <summary>
        /// Constructor
        /// </summary>
        /// ---------------------------------------------------------
        public XnaAudioTool()
        {
            MediaPlayer.MediaStateChanged += MediaPlayer_MediaStateChanged1;
                //+= new EventHandler(MediaPlayer_MediaStateChanged);
        }

        /// ---------------------------------------------------------
        /// <summary>
        /// Callback to record media state
        /// </summary>
        /// ---------------------------------------------------------        
        private void MediaPlayer_MediaStateChanged1(object sender, EventArgs e)
        {
            Debug.WriteLine("Media Player State Changed to " + MediaPlayer.State);
            currentPlayerState = MediaPlayer.State;
        }



        /// ---------------------------------------------------------
        /// <summary>
        /// Helper class for delayed loading of songs
        /// </summary>
        /// ---------------------------------------------------------
        class SongData 
        {
            public static ContentManager ContentManager;
            public string Path;
            public float Volume = 1; 
            public Song content = null;
            public Song Content
            {
                get
                {
                    if (content == null)
                    {
                        content = ContentManager.Load<Song>(Path);
                    }
                    return content;
                }

            }
        }

        /// ---------------------------------------------------------
        /// <summary>
        /// Load Sound effects and songs
        /// </summary>
        /// ---------------------------------------------------------
        public void LoadContent(ContentManager Content) 
        {
            SongData.ContentManager = Content;
            backgroundMusicList = new List<SongData>();
            List<string> musicToLoad = new List<string>( new string[]
            {
                "Sounds/Music/Solarwind_4-traumas", 
                "Sounds/Music/team_2", 
                "Sounds/Music/up_stream_03_farly and big bird", 
                "Sounds/Music/Cl-chip1", 
                "Sounds/Music/Beast-pl", 
                "Sounds/Music/Cutdry", 



                //"Sounds/Music/Class_9a", 
                //"Sounds/Music/Greenmol", 
                //"Sounds/Music/Litehead", 
            });

            while (musicToLoad.Count > 0)
            {
                int songId = Globals.rand.Next(musicToLoad.Count);
                SongData newSong = new SongData() { Path = musicToLoad[songId], Volume = 1f };
                backgroundMusicList.Add(newSong);
                Debug.WriteLine("Added song: " + musicToLoad[songId]);
                musicToLoad.RemoveAt(songId);
                
            }

            soundEffects = new List<SoundEffect>();
            for (int i = 0; i < (int)SoundEffectType.NumberOfSoundEffects; i++)
            {
                soundEffects.Add(
                    Content.Load<SoundEffect>("Sounds/" + ((SoundEffectType)i).ToString()));
            }

        }

        /// ---------------------------------------------------------
        /// <summary>
        /// Play a sound
        /// </summary>
        /// ---------------------------------------------------------
        public void PlaySound(SoundEffectType type)
        {
            PlaySound(type, 1, 0, 0);
        }

        /// ---------------------------------------------------------
        /// <summary>
        /// Play a sound
        /// </summary>
        /// ---------------------------------------------------------
        public void PlaySound(SoundEffectType type, float volume, float pitch, float delaySeconds)
        {
            SoundEffect sound = soundEffects[(int)type];
            effectsToPlay.Add(new QueuedSoundEffect { 
                Effect = sound, 
                StartTime = DateTime.Now.AddSeconds(delaySeconds), 
                Volume = volume * Globals.Options.SoundEffectVolume/ 10f, 
                Pitch = pitch });
        }

        /// ---------------------------------------------------------
        /// <summary>
        /// Stop a sound
        /// </summary>
        /// ---------------------------------------------------------
        public void StopSound(SoundEffectType type)
        {
            for (int i = 0; i < soundInstances.Count; i++)
            {
                if (activeSoundEffects[i].Equals(soundEffects[(int)type]))
                {
                    activeSoundEffects.RemoveAt(i);
                    soundInstances[i].Stop();
                    soundInstances.RemoveAt(i);
                }
            }
        }

        bool stopped = false;
        /// ---------------------------------------------------------
        /// <summary>
        /// STop music
        /// </summary>
        /// ---------------------------------------------------------
        public void StopMusic()
        {
            MediaPlayer.Pause();
            stopped = true;
        }

        /// ---------------------------------------------------------
        /// <summary>
        /// Skip to the next background song
        /// </summary>
        /// ---------------------------------------------------------
        public void SkipToNextBackgroundMusic() 
        {
            stopped = false;
            if (backgroundMusicList == null || backgroundMusicList.Count == 0) return;
            currentSong++;
            currentSong = currentSong % backgroundMusicList.Count;

            MediaPlayer.Volume = backgroundMusicList[currentSong].Volume * Globals.Options.MusicVolume / 10f;
            MediaPlayer.Play(backgroundMusicList[currentSong].Content); // this should be a new item
            MediaPlayer.IsRepeating = false;
            Debug.WriteLine("PLAYING SONG #" + currentSong);
        }

        /// ---------------------------------------------------------
        /// <summary>
        /// Set the volume for the current music
        /// </summary>
        /// ---------------------------------------------------------
        public void SetMusicVolume(float volume)
        {
            backgroundMusicList[currentSong].Volume = volume;
            MediaPlayer.Volume = backgroundMusicList[currentSong].Volume * Globals.Options.MusicVolume / 10f;    
        }

        int musicStartTimeout = 0;


        /// ---------------------------------------------------------
        /// <summary>
        /// Play a sound
        /// </summary>
        /// ---------------------------------------------------------
        public void ResolveState()
        {
            for (int i = 0; i < effectsToPlay.Count; )
            {
                // we can't have more than 16 sounds
                // if we have 16, don't play them
                int soundPlays = 0;
                int maxSoundPlays = 16;
                if (effectsToPlay[i].StartTime < DateTime.Now && soundPlays < maxSoundPlays)
                {
                    try
                    {
                        SoundEffectInstance instance = effectsToPlay[i].Effect.CreateInstance();
                        instance.Volume = effectsToPlay[i].Volume;
                        instance.Pitch = effectsToPlay[i].Pitch;
                        instance.Pan = 0;
                        instance.Play();
                        soundPlays++;

                        soundInstances.Add(instance);
                        activeSoundEffects.Add(effectsToPlay[i].Effect);
                        effectsToPlay.RemoveAt(i);
                    }
                    catch (InstancePlayLimitException)
                    {
                        // there have been too many sounds playing
                        // ignore the exception for now
                    }
                }
                else
                {
                    i++;
                }
            }

            for (int i = 0; i < soundInstances.Count; i++)
            {
                if (soundInstances[i].State == SoundState.Stopped)
                {
                    activeSoundEffects.RemoveAt(i);
                    soundInstances.RemoveAt(i);
                }
            }
            musicStartTimeout++;

            if (musicStartTimeout > 60 && !stopped)
            {
                musicStartTimeout = 0;
                if (currentPlayerState == MediaState.Stopped)
                    SkipToNextBackgroundMusic();
            }


        }

        MediaState currentPlayerState = MediaState.Stopped;

        /// ---------------------------------------------------------
        /// <summary>
        /// Class for tracking sound effects
        /// </summary>
        /// ---------------------------------------------------------
        class QueuedSoundEffect
        {
            public DateTime StartTime { get; set; }
            public SoundEffect Effect { get; set; }
            public float Volume { get; set; }
            public float Pitch { get; set; }
        }
    }
}
