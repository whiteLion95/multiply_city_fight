using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class TutorialStep : MonoBehaviour
{
    [SerializeField] private Image hand;
    [SerializeField] private Transform[] wayPoints;
    [SerializeField] private float handSpeed;
    [SerializeField] private float startDelay;
    [SerializeField] private Ease handEaseType;
    [SerializeField] private float delayOnLastPoint;

    private Sequence stepSequence;

    private void Awake()
    {
        TurnoOffWayPoints();
    }

    private void OnDisable()
    {
        stepSequence.Rewind();
    }

    private void TurnoOffWayPoints()
    {
        foreach (var point in wayPoints)
        {
            point.gameObject.SetActive(false);
        }
    }

    public void DoStep()
    {
        gameObject.SetActive(true);
        StartCoroutine(DoStepRoutine());
    }

    private IEnumerator DoStepRoutine()
    {
        yield return new WaitForSeconds(startDelay);

        stepSequence = DOTween.Sequence();
        Vector2 startPos;

        for (int i = 0; i < wayPoints.Length; i++)
        {
            if (i == 0)
                startPos = (hand.transform as RectTransform).anchoredPosition;
            else
                startPos = (wayPoints[i - 1] as RectTransform).anchoredPosition;

            Transform point = wayPoints[i];
            float deltaPos = ((point.transform as RectTransform).anchoredPosition - startPos).magnitude;
            stepSequence.Append(hand.transform.DOMove(point.position, deltaPos / handSpeed).SetEase(handEaseType));

            if (i == wayPoints.Length - 1)
                stepSequence.Append(hand.transform.DOMove(wayPoints[i].position, delayOnLastPoint));
        }

        stepSequence.Play().SetLoops(-1, LoopType.Restart);
    }
}
