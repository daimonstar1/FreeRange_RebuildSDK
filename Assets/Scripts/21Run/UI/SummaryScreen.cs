using System;
using System.Collections;
using DarkTonic.MasterAudio;
using FRG.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FRG.Taco.Run21
{
    public class SummaryScreen : PoolObject
    {
        [SerializeField] TextMeshProUGUI columnsClearedValue;
        [SerializeField] TextMeshProUGUI timeBonusValue;
        [SerializeField] TextMeshProUGUI finalScoreValue;
        [SerializeField] TextMeshProUGUI unusedCardsValue;
        [SerializeField] TextMeshProUGUI topStreakValue;
        [SerializeField] TextMeshProUGUI playerScoreValue;
        [SerializeField] TextMeshProUGUI noBustsBonusValue;
        [SerializeField] TextMeshProUGUI emptyLanesBonusValue;
        [SerializeField] TextMeshProUGUI perfectGameBonusText;
        [SerializeField] TextMeshProUGUI perfectGameBonusValue;
        [SerializeField] TextMeshProUGUI bestFinalScoreValue;
        [SerializeField] Button button_submit;
        [SerializeField] RawImage timeBar;

        public delegate void OnSoundAction(AudioManager.Sound sound);

        private Run21 run21;
        int columnsCleared;

        private void OnEnable()
        {

            Initialize();
            if (run21.Score.TimeScore != 0)
            {
                StartCoroutine(TimeBonusAnimation());
            }

            columnsClearedValue.text = columnsCleared.ToString();
            button_submit.onClick.AddListener(Gameplay.instance.ConfirmSubmitScore);
        }

        void Update()
        {
            float currentNormalizedTimeBarWidth = Gameplay.instance.GameOverDuration / Gameplay.instance.durations.summaryLingerForSeconds;
            float xValue = -(currentNormalizedTimeBarWidth / 2f) + 0.5f;
            timeBar.uvRect = new Rect(xValue, 0f, 0.5f, 1f);
        }

        private void Initialize()
        {
            run21 = Gameplay.instance.run21;
            if (run21.Score.PerfectGameScore != 0) {
                perfectGameBonusText.gameObject.SetActive(true);
                perfectGameBonusValue.gameObject.SetActive(true);
            }
            columnsCleared = run21.ColumnsCleared;
            finalScoreValue.text = (run21.Score.FinalScore - run21.Score.TimeScore).ToString();
            playerScoreValue.text = run21.Score.GameScore.ToString();
            timeBonusValue.text = run21.Score.TimeScore.ToString();
            noBustsBonusValue.text = run21.Score.BustScore.ToString();
            emptyLanesBonusValue.text = run21.Score.LaneScore.ToString();
            perfectGameBonusValue.text = run21.Score.PerfectGameScore.ToString();
            unusedCardsValue.text = run21.UnusedCardCount.ToString();
            topStreakValue.text = run21.BestStreak.ToString();
            columnsClearedValue.text = run21.ColumnsCleared.ToString();
            bestFinalScoreValue.text = PlayerPrefsManager.GetBestFinalScore().ToString();

        }

        private void OnDisable()
        {
            // cleanup
            perfectGameBonusText.gameObject.SetActive(false);
            perfectGameBonusValue.gameObject.SetActive(false);
        }

        IEnumerator TimeBonusAnimation()
        {
            for (float f = 0; f <= run21.Score.TimeScore; f += Gameplay.instance.durations.addPoints)
            {
                timeBonusValue.text = f.ToString();
                AudioManager.instance.PlaySound(AudioManager.Sound.GFBonusTime);
                yield return new WaitForSeconds(Gameplay.instance.durations.summaryTimeBonusWaitBetweenAdding);
            }
            timeBonusValue.text = run21.Score.TimeScore.ToString();
            StartCoroutine(TimeBonusToFinalscore());
        }

        IEnumerator TimeBonusToFinalscore()
        {
            for (float f = run21.Score.FinalScore - run21.Score.TimeScore; f <= run21.Score.FinalScore; f += Gameplay.instance.durations.addPoints)
            {
                finalScoreValue.text = f.ToString();
                AudioManager.instance.PlaySound(AudioManager.Sound.GFBonusTime);
                yield return new WaitForSeconds(Gameplay.instance.durations.summaryTimeBonusWaitBetweenAdding);
            }
            finalScoreValue.text = run21.Score.FinalScore.ToString();


            if (run21.Score.FinalScore >= PlayerPrefsManager.GetBestFinalScore())
            {
                StartCoroutine(TimeBonusToBestFinalScore());
            }
            else
            {
                yield return null;
            }
        }

        IEnumerator TimeBonusToBestFinalScore()
        {
            for (float f = PlayerPrefsManager.GetBestFinalScore(); f <= run21.Score.FinalScore; f += Gameplay.instance.durations.addPoints)
            {
                bestFinalScoreValue.text = f.ToString();
                AudioManager.instance.PlaySound(AudioManager.Sound.GFBonusTime);
                yield return new WaitForSeconds(Gameplay.instance.durations.summaryTimeBonusWaitBetweenAdding);
            }
            bestFinalScoreValue.text = run21.Score.FinalScore.ToString();
            PlayerPrefsManager.SetBestFinalScore(run21.Score.FinalScore);
            yield return null;
        }
    }
}
