using System;
using System.Collections;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Deslab.UI
{
    public class FakeServerConnection : CanvasGroupWindow
    {
        [SerializeField] private TMP_Text connectionText;
        private Action onConnected;
        public override void ShowWindow(Action onCompleted = null)
        {
            onConnected = onCompleted;
            StartCoroutine(ShowFakeConnection());
            base.ShowWindow(() =>
            {
            });
        }

        private IEnumerator ShowFakeConnection()
        {
            connectionText.text = "Connecting to server...";
            yield return new WaitForSeconds(Random.Range(0.2f, 1.5f));
            connectionText.text = "Finding match...";
            yield return new WaitForSeconds(Random.Range(0.2f, 1.5f));
            connectionText.text = "Connect to match...";
            yield return new WaitForSeconds(Random.Range(0.2f, 1f));
            HideWindow();
            onConnected?.Invoke();
        }

        public override void HideWindow()
        {
            base.HideWindow(() =>
            {
            });
        }
    }
}