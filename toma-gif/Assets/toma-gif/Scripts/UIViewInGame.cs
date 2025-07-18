using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using UnityEngine;
using UnityEngine.UI;

namespace tomagif
{
    public class UIViewInGame
    {
        private readonly HKUIDocument document;

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
        }

        public async UniTask<bool> OnClickJudgementButtonAsync(CancellationToken cancellationToken)
        {
            var result = await UniTask.WhenAny(
                document.Q<HKUIDocument>("UIElement.Button.True").Q<Button>("Button").OnClickAsync(cancellationToken),
                document.Q<HKUIDocument>("UIElement.Button.False").Q<Button>("Button").OnClickAsync(cancellationToken)
            );

            return result == 0;
        }
    }
}
