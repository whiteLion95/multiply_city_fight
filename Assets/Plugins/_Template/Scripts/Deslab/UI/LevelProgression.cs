using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Deslab.Level;
using DG.Tweening;

/// <summary>
/// Display level progression by distance.
/// </summary>
public class LevelProgression : MonoBehaviour
{
    [SerializeField] protected Transform startPoint;
    [SerializeField] protected Transform endPoint;
    [SerializeField] protected Transform playerTransform;
    [SerializeField] protected Slider progressionSlider;
    [SerializeField] protected TMP_Text currentLevelLabel;
    [SerializeField] protected float minDistanceToStopUpdating;
    [SerializeField] protected float sliderSmoothness;

    private float _levelDistance;
    private bool _updateDistance;

    private void OnEnable()
    {
        LevelManager.OnLevelStarted += OnLevelStartedHandler;
    }

    private void OnDisable()
    {
        LevelManager.OnLevelStarted -= OnLevelStartedHandler;
    }

    protected virtual void OnLevelStartedHandler()
    {
        _updateDistance = true;
    }

    public void Init(Transform startPoint, Transform endPoint, Transform playerTransform)
    {
        this.startPoint = startPoint;
        this.endPoint = endPoint;
        this.playerTransform = playerTransform;
        SetDistance();
    }

    /// <summary>
    /// Setting distance from start point to end and setting distance value to progressionSlider
    /// </summary>
    protected virtual void SetDistance()
    {
        _levelDistance = FastDistance(startPoint.position, endPoint.position);
        progressionSlider.maxValue = _levelDistance;
        progressionSlider.value = progressionSlider.maxValue;
        currentLevelLabel.text = StaticManager.levelID.ToString();
    }

    void Update()
    {
        UpdateSlider();
    }

    private void UpdateSlider()
    {
        if (_updateDistance && playerTransform != null)
        {
            _levelDistance = FastDistance(playerTransform.position, endPoint.position);

            if (_levelDistance < minDistanceToStopUpdating)
            {
                _levelDistance = 0;
                _updateDistance = false;
            }

            progressionSlider.DOValue(_levelDistance, sliderSmoothness);
        }
    }

    protected virtual float FastDistance(Vector3 start, Vector3 end)
    {
        float cacheDistance = Vector3.Distance(start, end);
        return Mathf.Abs(cacheDistance);
    }
}
