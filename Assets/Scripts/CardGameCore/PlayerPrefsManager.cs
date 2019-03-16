namespace FRG.Taco
{
    using UnityEngine;
    using System.Collections;

    public class PlayerPrefsManager : MonoBehaviour
    {

        /// <summary>
        /// Centralized access to all the keys in player prefs
        /// </summary>
        const string BEST_FINAL_SCORE_KEY = "final_score";
        const string AUDIO_ON = "audio";          // 1 for true, 0 for false
        const string SFX_ON = "sfx";
        const string SFX_VOLUME = "sfx_volume";
        const string MUSIC_VOLUME = "music_volume";
        const string BUTTONS_SWAPPED = "buttons_swapped";

        public static float previousSFXVolume = GetSFXVolume();
        public static float previousMusicVolume = GetMusicVolume();


        /// <summary>
        /// Sets the highest final score the player has achieved on this machine.
        /// </summary>
        /// <param name="pScore">New best score to be set</param>
        public static void SetBestFinalScore(int pScore)
        {
            PlayerPrefs.SetInt(BEST_FINAL_SCORE_KEY, pScore);
        }

        /// <summary>
        /// Retrieves the best final score the player has achieved on this machine.
        /// </summary>
        /// <returns>Returns the best final score.</returns>
        public static int GetBestFinalScore()
        {
            return PlayerPrefs.GetInt(BEST_FINAL_SCORE_KEY, 0);
        }

        public static void SetIsButtonsSwapped(bool pSwapped)
        {
            PlayerPrefs.SetInt(BUTTONS_SWAPPED, pSwapped ? 1 : 0);
        }

        public static bool GetIsButtonsSwapped()
        {
            return PlayerPrefs.GetInt(BUTTONS_SWAPPED, 0) == 1 ? true : false ;
        }

        /// <summary>
        /// Sets whether the game will play music or not
        /// </summary>
        /// <param name="pAudioOn"></param>
        public static void SetIsMusicPlaying(bool pAudioOn)
        {
            PlayerPrefs.SetInt(AUDIO_ON, pAudioOn ? 1 : 0);
        }

        /// <summary>
        /// Retrieves the setting for game music
        /// </summary>
        /// <returns></returns>
        public static bool GetIsMusicPlaying()
        {
            return PlayerPrefs.GetInt(AUDIO_ON, 1) == 1 ? true : false;
        }


        /// <summary>
        /// Set the value of whether to play SFX or not
        /// </summary>
        /// <param name="pSfxOn"></param>
        public static void SetIsSFXPlaying(bool pSfxOn)
        {
            PlayerPrefs.SetInt(SFX_ON, pSfxOn ? 1: 0);
        }


        /// <summary>
        /// Retrieve the value of the sfx bool
        /// </summary>
        /// <returns></returns>
        public static bool GetIsSFXPlaying()
        {
            return PlayerPrefs.GetInt(SFX_ON, 1) == 1 ? true : false;
        }

        /// <summary>
        /// Sets the volume at which the music will be played
        /// </summary>
        /// <param name="pVolume"></param>
        public static void SetMusicVolume(float pVolume)
        {
            if (pVolume <= 0f) { SetIsMusicPlaying(false); } else { SetIsMusicPlaying(true); }
            previousMusicVolume = GetMusicVolume();
            PlayerPrefs.SetFloat(MUSIC_VOLUME, pVolume);
        }

        /// <summary>
        /// Retrieves the music volume. If it hasn't been set yet, it returns 1, max volume.
        /// <returns></returns>
        public static float GetMusicVolume()
        {
            return PlayerPrefs.GetFloat(MUSIC_VOLUME, 1f);
        }

        /// <summary>
        /// Sets the volume for sound effects to be played at
        /// </summary>
        /// <param name="pVolume"></param>
        public static void SetSFXVolume(float pVolume)
        {
            if (pVolume <= 0f) { SetIsSFXPlaying(false); } else { SetIsSFXPlaying(true); }
            previousSFXVolume = GetSFXVolume();
            PlayerPrefs.SetFloat(SFX_VOLUME, pVolume);
        }

        /// <summary>
        /// Returns the volume of sound effects. If it hasn't been set yet, it returns 1, max volume.
        /// </summary>
        /// <returns></returns>
        public static float GetSFXVolume()
        {
            return PlayerPrefs.GetFloat(SFX_VOLUME, 1f);
        }
    }
}