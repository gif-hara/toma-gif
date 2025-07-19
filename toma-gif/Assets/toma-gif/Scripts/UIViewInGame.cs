using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using TMPro;
using UnityEngine;
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

        public void Initialize()
        {
            document.gameObject.SetActive(false);
        }

        public void Activate()
        {
            document.gameObject.SetActive(true);
            EffectCorrect.gameObject.SetActive(false);
            EffectIncorrect.gameObject.SetActive(false);
            LieMessage.gameObject.SetActive(false);
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

        public void SetActiveLieMessage(bool isActive)
        {
            LieMessage.gameObject.SetActive(isActive);
        }

        private HKUIDocument EffectCorrect => document.Q<HKUIDocument>("Effect.Correct");

        private HKUIDocument EffectIncorrect => document.Q<HKUIDocument>("Effect.Incorrect");

        private HKUIDocument LieMessage => document.Q<HKUIDocument>("LieMessage");
    }
}
