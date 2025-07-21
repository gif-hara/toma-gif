using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using LitMotion;
using LitMotion.Extensions;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace tomagif
{
    public class UIViewInGame
    {
        private readonly HKUIDocument document;

        private readonly List<HKUIDocument> evidenceMessages = new();

        public UIViewInGame(HKUIDocument document)
        {
            this.document = document;
        }

        public void Activate(
            Observable<float> timeLimit,
            float gameTime,
            Observable<int> score,
            Observable<int> combo,
            string comboFormat,
            CancellationToken cancellationToken
            )
        {
            var timeLimitStream = timeLimit.Subscribe((this, gameTime), static (x, t) =>
            {
                var (@this, gameTime) = t;
                @this.document.Q<HKUIDocument>("TimeLimit").Q<Slider>("Slider").value = x / gameTime;
            });
            timeLimitStream.RegisterTo(document.destroyCancellationToken);
            timeLimitStream.RegisterTo(cancellationToken);

            var scoreStream = score.Subscribe(this, static (x, @this) =>
            {
                @this.document.Q<HKUIDocument>("Score").Q<TMP_Text>("Text").text = x.ToString();
            });
            scoreStream.RegisterTo(document.destroyCancellationToken);
            scoreStream.RegisterTo(cancellationToken);

            var comboStream = combo.Subscribe((this, comboFormat), static (x, t) =>
            {
                var (@this, comboFormat) = t;
                var comboText = @this.document.Q<HKUIDocument>("Score").Q<TMP_Text>("Combo");
                comboText.text = string.Format(comboFormat, x);
                comboText.gameObject.SetActive(x > 1);
            });

            document.gameObject.SetActive(true);
            EffectCorrect.gameObject.SetActive(false);
            EffectIncorrect.gameObject.SetActive(false);
            EffectGameOver.gameObject.SetActive(false);
            LieMessage.gameObject.SetActive(false);
            Result.gameObject.SetActive(false);
            Title.gameObject.SetActive(false);
            CountDown.gameObject.SetActive(false);
            document.Q<HKUIDocument>("EvidenceList").gameObject.SetActive(true);
            document.Q<HKUIDocument>("UIElement.Button.True").gameObject.SetActive(true);
            document.Q<HKUIDocument>("UIElement.Button.False").gameObject.SetActive(true);
        }

        public async UniTask<bool> OnClickJudgementButtonAsync(CancellationToken cancellationToken)
        {
            var result = await UniTask.WhenAny(
                document.Q<HKUIDocument>("UIElement.Button.True").Q<Button>("Button").OnClickAsync(cancellationToken),
                document.Q<HKUIDocument>("UIElement.Button.False").Q<Button>("Button").OnClickAsync(cancellationToken)
            );

            return result == 0;
        }

        public void SetupEvidences(List<string> evidences, string talkMessage)
        {
            var evidenceList = document.Q<HKUIDocument>("EvidenceList");
            var evidenceParent = evidenceList.Q<Transform>("Messages");
            var evidencePrefab = evidenceList.Q<HKUIDocument>("Prefab.Message");
            foreach (var i in evidenceMessages)
            {
                UnityEngine.Object.Destroy(i.gameObject);
            }
            evidenceMessages.Clear();

            foreach (var evidence in evidences)
            {
                var message = UnityEngine.Object.Instantiate(evidencePrefab, evidenceParent);
                message.Q<TMP_Text>("Message").text = evidence;
                evidenceMessages.Add(message);
            }

            document.Q<HKUIDocument>("TalkMessage").Q<TMP_Text>("Message").text = talkMessage;
        }

        public async UniTask ShowEffectCorrectAsync(CancellationToken cancellationToken)
        {
            EffectCorrect.gameObject.SetActive(true);
            EffectIncorrect.gameObject.SetActive(false);
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: cancellationToken);
            EffectCorrect.gameObject.SetActive(false);
        }

        public async UniTask ShowEffectIncorrectAsync(CancellationToken cancellationToken)
        {
            EffectIncorrect.gameObject.SetActive(true);
            EffectCorrect.gameObject.SetActive(false);
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: cancellationToken);
            EffectIncorrect.gameObject.SetActive(false);
        }

        public async UniTask ProcessGameOverAsync(int score, string scoreFormat, CancellationToken cancellationToken)
        {
            document.Q<HKUIDocument>("EvidenceList").gameObject.SetActive(false);
            document.Q<HKUIDocument>("UIElement.Button.True").gameObject.SetActive(false);
            document.Q<HKUIDocument>("UIElement.Button.False").gameObject.SetActive(false);
            EffectGameOver.gameObject.SetActive(true);
            await UniTask.Delay(TimeSpan.FromSeconds(3.0f), cancellationToken: cancellationToken);
            Result.Q<TMP_Text>("Score").text = string.Format(scoreFormat, score);
            Result.gameObject.SetActive(true);
            await Result.Q<HKUIDocument>("UIElement.Button.Retry").Q<Button>("Button")
                .OnClickAsync(cancellationToken);
            await BeginFade(Color.clear, Color.black, 0.5f, cancellationToken);
        }

        public void SetActiveLieMessage(bool isActive)
        {
            LieMessage.gameObject.SetActive(isActive);
        }

        public UniTask BeginFade(Color from, Color to, float duration, CancellationToken cancellationToken)
        {
            var fade = document.Q<HKUIDocument>("Fade");
            fade.gameObject.SetActive(true);
            return LMotion.Create(from, to, duration)
                .BindToColor(fade.Q<Image>("Image"))
                .ToUniTask(cancellationToken: cancellationToken);
        }

        public void ClearEvidenceMessages()
        {
            foreach (var message in evidenceMessages)
            {
                UnityEngine.Object.Destroy(message.gameObject);
            }
            evidenceMessages.Clear();
            document.Q<HKUIDocument>("TalkMessage").Q<TMP_Text>("Message").text = "";
        }

        public async UniTask ProcessTitleAsync(AudioManager audioManager, AudioMixer audioMixer, CancellationToken cancellationToken)
        {
            var scope = CancellationTokenSource.CreateLinkedTokenSource(document.destroyCancellationToken, cancellationToken);
            Title.gameObject.SetActive(true);
            CountDown.gameObject.SetActive(false);
            Tutorial.gameObject.SetActive(false);
            LieMessage.gameObject.SetActive(false);
            audioManager.PlayBgm("Title");
            var bgmSlider = Title.Q<HKUIDocument>("UIElement.Slider.Bgm").Q<Slider>("Slider");
            bgmSlider
                .OnValueChangedAsObservable()
                .Subscribe(x => audioMixer.SetFloat("BgmVolume", x > 0 ? Mathf.Log10(x) * 20 : -80))
                .RegisterTo(scope.Token);
            var sfxSlider = Title.Q<HKUIDocument>("UIElement.Slider.Sfx").Q<Slider>("Slider");
            sfxSlider
                .OnValueChangedAsObservable()
                .Subscribe(x => audioMixer.SetFloat("SfxVolume", x > 0 ? Mathf.Log10(x) * 20 : -80))
                .RegisterTo(scope.Token);
            bgmSlider.value = 0.5f;
            sfxSlider.value = 0.5f;
            await BeginFade(Color.black, Color.clear, 0.5f, cancellationToken);
            await Title.Q<HKUIDocument>("UIElement.Button.PlayGame").Q<Button>("Button")
                .OnClickAsync(scope.Token);
            audioManager.PlaySfx("Decision");
            audioManager.FadeOutBgmAsync(0.5f, scope.Token).Forget();
            await BeginFade(Color.clear, Color.black, 0.5f, cancellationToken);
            Title.gameObject.SetActive(false);
            scope.Cancel();
            scope.Dispose();
        }

        public async UniTask ShowCountDownAsync(int count, string startMessage, AudioManager audioManager, CancellationToken cancellationToken)
        {
            CountDown.gameObject.SetActive(true);
            var animator = CountDown.Q<Animator>("Animator");
            var countText = CountDown.Q<TMP_Text>("Text");

            for (int i = count; i >= 1; i--)
            {
                audioManager.PlaySfx("CountDown");
                countText.text = i.ToString();
                animator.Play("In", 0, 0.0f);
                await UniTask.Delay(TimeSpan.FromSeconds(1.0f), cancellationToken: cancellationToken);
            }

            audioManager.PlaySfx("GameStart");
            countText.text = startMessage;
            animator.Play("In", 0, 0.0f);
            await UniTask.Delay(TimeSpan.FromSeconds(1.0f), cancellationToken: cancellationToken);
            CountDown.gameObject.SetActive(false);
        }

        public async UniTask ProcessTutorialAsync(AudioManager audioManager, CancellationToken cancellationToken)
        {
            await BeginFade(Color.black, Color.clear, 0.5f, cancellationToken);
            var animator = Tutorial.Q<Animator>("Animator");
            Tutorial.gameObject.SetActive(true);
            animator.Play("In", 0, 0.0f);
            audioManager.PlaySfx("Tutorial.1");
            await UniTask.Delay(TimeSpan.FromSeconds(40.0f / 60.0f), cancellationToken: cancellationToken);
            await Tutorial.Q<HKUIDocument>("UIElement.Button.AnyClick").Q<Button>("Button")
                .OnClickAsync(cancellationToken);
            animator.Play("Out", 0, 0.0f);
            audioManager.PlaySfx("Tutorial.2");
            await UniTask.Delay(TimeSpan.FromSeconds(1.0f), cancellationToken: cancellationToken);
            Tutorial.gameObject.SetActive(false);
        }

        private HKUIDocument EffectCorrect => document.Q<HKUIDocument>("Effect.Correct");

        private HKUIDocument EffectIncorrect => document.Q<HKUIDocument>("Effect.Incorrect");

        private HKUIDocument EffectGameOver => document.Q<HKUIDocument>("Effect.GameOver");

        private HKUIDocument LieMessage => document.Q<HKUIDocument>("LieMessage");

        private HKUIDocument Result => document.Q<HKUIDocument>("Result");

        private HKUIDocument Title => document.Q<HKUIDocument>("Title");

        private HKUIDocument CountDown => document.Q<HKUIDocument>("CountDown");

        private HKUIDocument Tutorial => document.Q<HKUIDocument>("Tutorial");
    }
}
