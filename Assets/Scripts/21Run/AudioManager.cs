namespace FRG.Taco
{
    using System.Collections.Generic;
    using DarkTonic.MasterAudio;
    using UnityEngine;
    using UnityEngine.Audio;

    public class AudioManager : MonoBehaviour
    {
        #region DEFINITIONS

        /// <summary>
        /// Storing all of the available sounds into an enum.
        /// This way they are more easily accessed from other classes.
        /// </summary>
        public enum Sound
        {
            DrawCard,
            CardPlaced,
            Streak,
            Bust,
            StackComplete,
            LastXSeconds,
            TimerWarned,
            GameFinished,
            GFNoBusts,
            GFPerfectGame,
            GFBonusTime,
            Shuffle,
            ScoreTally,
            LanePopup,
            OutOfTime,
            BlackJack,
            FiveCards,
            Undo,
            Combo
        }

        public enum Streak
        {
            First,
            Second,
            Third,
            Fourth,
            Fifth
        }
        #endregion

        #region STATIC
        /// <summary>
        /// AudioManager is a Singleton so it doesn't depend
        /// on any class it is being called from. Just use this
        /// instance to use all of the AudioManager methods.
        /// </summary>
        public static AudioManager instance { get; private set; }
        #endregion

        #region PROPERTIES

        /// <summary>
        /// Flagging whether the music will be played or not
        /// </summary>
        private bool _musicOn;

        public bool MusicOn { 
            get { return _musicOn; }
            set {
                _musicOn = value;
                if ( ! _musicOn) { audioMixer.SetFloat("volumeOfMusic", -80f); } else { audioMixer.SetFloat("volumeOfMusic", Mathf.Log10(_musicVolume) * 60); }
            }
        }

        /// <summary>
        /// This is a flag that determines if we will play sound effects
        /// </summary>
        private bool _sfxOn;

        public bool SFXOn
        {
            get { return _sfxOn; }
            set { _sfxOn = value;
                if ( ! _sfxOn) { audioMixer.SetFloat("volumeOfFX", -80f); } else { audioMixer.SetFloat("volumeOfFX", Mathf.Log10(_sfxVolume) * 60); }
            }
        }

        /// <summary>
        /// What volume will the music be played at. If the music is flagged as
        /// off, music will not be played regardless of the volume. 
        /// </summary>
        private float _musicVolume;

        public float MusicVolume
        {
            get { return _musicVolume; }
            set { if (value >= 0f && value <= 1f) {
                    _musicVolume = value;
                    audioMixer.SetFloat("volumeOfMusic", Mathf.Log10(_musicVolume) * 60);
                } }
        }

        /// <summary>
        /// At what volume to play the sound effects. If the sound effect flag
        /// is off, sound effects will not be played regardless of volume.
        /// </summary>
        private float _sfxVolume;

        public float SFXVolume
        {
            get { return _sfxVolume; }
            set { if (value >= 0f && value <= 1f) {
                    _sfxVolume = value;
                    audioMixer.SetFloat("volumeOfFX", Mathf.Log10(_sfxVolume) * 60);
                } }
        }
        #endregion

        /// <summary>
        /// Drawing in all the sounds our audio engineers have created
        /// </summary>
        #region SERIALIZED_FIELDS
        [SerializeField] AudioMixer audioMixer;
        [SerializeField, SoundGroup] string drawCardSound;
        [SerializeField, SoundGroup] string cardPlacedSound;
        [SerializeField, SoundGroup] string shuffleDeckSound;
        [SerializeField, SoundGroup] string gameFinishedSound;
        [SerializeField, SoundGroup] string GFNoBustsSound;
        [SerializeField, SoundGroup] string GFPerfectGameSound;
        [SerializeField, SoundGroup] string streakSound;
        [SerializeField, SoundGroup] string bustSound;
        [SerializeField, SoundGroup] string stackCompleteSound;
        [SerializeField, SoundGroup] string scoreTallySound;
        [SerializeField, SoundGroup] string timerLastXSecondsSound;
        [SerializeField, SoundGroup] string timerWarnedSound;
        [SerializeField, SoundGroup] string lanePopup;
        [SerializeField, SoundGroup] string GFBonusTimeSound;
        [SerializeField, SoundGroup] string OutOfTimeSound;
        [SerializeField, SoundGroup] string blackJackSound;
        [SerializeField, SoundGroup] string fiveCardsSound;
        [SerializeField, SoundGroup] string undoSound;
        [SerializeField, SoundGroup] string comboSound;
        #endregion

        #region PUBLIC_FIELDS


        #endregion

        #region PRIVATE_FIELDS

        /// <summary>
        /// This dictionary is used to connect strings that MasterAudio uses
        /// and our Sound enum for easier use. It is more transparent this way.
        /// </summary>
        private Dictionary<Sound, string> soundsCollection = new Dictionary<Sound, string>();

        private Dictionary<Streak, string> streaks = new Dictionary<Streak, string>();

        #endregion

        #region PUBLIC_METHODS

        /// <summary>
        /// Whenever we want to play a sound we call this method on the AudioManager instance
        /// and pass it a sound we want to play irregardless of whether it's a sound effect
        /// or a song.
        /// Before playing it we set the volume from <seealso cref = "PlayerPrefsManager.cs"/>
        /// </summary>
        /// <param name="sound">Sound to be played</param>
        public void PlaySound(Sound sound)
        {
            if ( ! PlayerPrefsManager.GetIsSFXPlaying()) { return; }
            string soundToPlay;
            soundsCollection.TryGetValue(sound, out soundToPlay);
            if (!string.IsNullOrEmpty(soundToPlay))
            {
                MasterAudio.PlaySoundAndForget(soundToPlay, _sfxVolume);
            }
        }

        /// <summary>
        /// Overloaded method that gets passed the streak we're on
        /// so it knows which specific member of the group to play.
        /// </summary>
        /// <param name="sound">Group of streak sounds</param>
        /// <param name="streak">Specific streak sound we want to play</param>
        public void PlaySound(Sound sound, Streak streak)
        {
            if ( ! PlayerPrefsManager.GetIsSFXPlaying()) { return; }
            string group;
            string variationToPlay;
            soundsCollection.TryGetValue(sound, out group);
            streaks.TryGetValue(streak, out variationToPlay);

            if (!string.IsNullOrEmpty(group) && !string.IsNullOrEmpty(variationToPlay))
            {
                MasterAudio.PlaySound(group, _sfxVolume, null, 0f, variationToPlay, null, false, false);
            }
        }

        /// <summary>
        /// When calling this method the sound will be stopped.
        /// </summary>
        /// <param name="sound">Sound to be stopped</param>
        public void StopSound(Sound sound)
        {
            string soundToStop;
            soundsCollection.TryGetValue(sound, out soundToStop);
            if (!string.IsNullOrEmpty(soundToStop))
            {
                MasterAudio.StopAllOfSound(soundToStop);
            }
        }

        /// <summary>
        /// Turns the mute on or off, stopping or allowing all sound and music
        /// </summary>
        /// <param name="isPlaying">True or false, to set the mute to it.</param>
        public void ToggleMute(bool isPlaying)
        {
            switch (isPlaying)
            {
                case true:
                    MasterAudio.UnmuteEverything();
                    break;
                case false:
                    MasterAudio.MuteEverything();
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region PRIVATE_METHODS
        void Awake()
        {
            instance = this;
            _sfxOn = PlayerPrefsManager.GetIsSFXPlaying();
            _musicOn = PlayerPrefsManager.GetIsMusicPlaying();
            _sfxVolume = PlayerPrefsManager.GetSFXVolume();
            _musicVolume = PlayerPrefsManager.GetMusicVolume();
            ConnectSounds();
        }

        void Start()
        {
            if (!_musicOn) { audioMixer.SetFloat("volumeOfMusic", -80f); } else { audioMixer.SetFloat("volumeOfMusic", Mathf.Log10(_musicVolume) * 60); }
            if (!_sfxOn) { audioMixer.SetFloat("volumeOfFX", -80f); } else { audioMixer.SetFloat("volumeOfFX", Mathf.Log10(_sfxVolume) * 60); }

        }

        /// <summary>
        /// This method simply connects the right Sound enum value to the
        /// string of the dragged sound and puts them in a easy-to-use Dictionary
        /// </summary>
        private void ConnectSounds()
        {
            soundsCollection.Add(Sound.DrawCard, drawCardSound);
            soundsCollection.Add(Sound.CardPlaced, cardPlacedSound);
            soundsCollection.Add(Sound.Shuffle, shuffleDeckSound);
            soundsCollection.Add(Sound.GameFinished, gameFinishedSound);
            soundsCollection.Add(Sound.GFNoBusts, GFNoBustsSound);
            soundsCollection.Add(Sound.GFPerfectGame, GFPerfectGameSound);
            soundsCollection.Add(Sound.Streak, streakSound);
            soundsCollection.Add(Sound.Bust, bustSound);
            soundsCollection.Add(Sound.StackComplete, stackCompleteSound);
            soundsCollection.Add(Sound.ScoreTally, scoreTallySound);
            soundsCollection.Add(Sound.LastXSeconds, timerLastXSecondsSound);
            soundsCollection.Add(Sound.TimerWarned, timerWarnedSound);
            soundsCollection.Add(Sound.LanePopup, lanePopup);
            soundsCollection.Add(Sound.OutOfTime, OutOfTimeSound);
            soundsCollection.Add(Sound.GFBonusTime, GFBonusTimeSound);
            soundsCollection.Add(Sound.BlackJack, blackJackSound);
            soundsCollection.Add(Sound.FiveCards, fiveCardsSound);
            soundsCollection.Add(Sound.Undo, undoSound);
            soundsCollection.Add(Sound.Combo, comboSound);

            // streak variations
            streaks.Add(Streak.First, "UI_Run_01");
            streaks.Add(Streak.Second, "UI_Run_02");
            streaks.Add(Streak.Third, "UI_Run_03");
            streaks.Add(Streak.Fourth, "UI_Run_04");
            streaks.Add(Streak.Fifth, "UI_Run_05");
        }
        #endregion





    }
}