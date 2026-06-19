using UnityEngine;
using System.Collections.Generic;

public class FiringSystem : MonoBehaviour
{
    [SerializeField] private float currentTemperature;
    [SerializeField] private float temperatureRisePerSecond = 50f;
    [SerializeField] private float windValue = 0f;
    [SerializeField] private float fuelBoost = 200f;
    [SerializeField] private bool isFiring = true;
    [SerializeField] private FireConfigSO fireConfig;
    [SerializeField] private float lowWindThreshold = 0.3f;
    [SerializeField] private float lowWindDurationThreshold = 3f;
    [SerializeField] private float temperatureDropPerSecond = 30f;

    private bool isWindowOpen;
    private float lowWindTimer;
    private bool isTemperatureDropping;

    public enum FireZone { Underfired, Normal, Overfired }

    public float CurrentTemperature { get { return currentTemperature; } }
    public bool IsFiring { get { return isFiring; } }
    public bool IsWindowOpen { get { return isWindowOpen; } }
    public bool IsTemperatureDropping { get { return isTemperatureDropping; } }
    public float WindValue { get { return windValue; } }

    public void SetWindValue(float value) { windValue = Mathf.Clamp01(value); }
    public void AddFuel() { currentTemperature += fuelBoost; }
    public void ToggleWindow() { isWindowOpen = !isWindowOpen; }

    public void StopFiring()
    {
        isFiring = false;
        isWindowOpen = true;
    }

    // 温度条视频倒放到 0 帧时调用，强制走欠烧次品分支
    public void ForceUnderfiredOpen()
    {
        currentTemperature = 0f;
        isTemperatureDropping = false;
        lowWindTimer = 0f;
        StopFiring();
    }

    public FireZone GetCurrentZone()
    {
        if (currentTemperature < 1000f) return FireZone.Underfired;
        if (currentTemperature > 1300f) return FireZone.Overfired;
        return FireZone.Normal;
    }

    public float GetFireScore()
    {
        float score;
        if (currentTemperature < 1000f)
        {
            score = Mathf.InverseLerp(0f, 1000f, currentTemperature) * 100f;
        }
        else if (currentTemperature > 1300f)
        {
            score = Mathf.InverseLerp(1500f, 1300f, currentTemperature) * 100f;
        }
        else
        {
            score = 100f;
        }
        return Mathf.Clamp(score, 0f, 100f);
    }

    public FireScoreResult CalculateScore()
    {
        FireInput input = new FireInput
        {
            temperatureReadings = new float[] { currentTemperature },
            timeStamps = new float[] { Time.time },
            stopFireTemp = currentTemperature,
            stopFireTime = Time.time,
            kilnOpenTemp = isWindowOpen ? currentTemperature : 0f,
            reductionWasSwitched = false,
            reductionSwitchTemp = 0f,
            stageDurations = new float[8],
            flameJudgments = new bool[4],
            triggeredDefectIds = new List<string>() // 由烧窑过程逻辑填充，当前为空
        };

        Debug.Log($"[FiringSystem.CalculateScore] Temp={currentTemperature:F1}°C | WindowOpen={isWindowOpen} | Firing={isFiring}");
        return FireCalculator.Calculate(input, fireConfig);
    }

    public void ResetFiring()
    {
        currentTemperature = 0f;
        isFiring = false;
        isWindowOpen = false;
        windValue = 0f;
        lowWindTimer = 0f;
        isTemperatureDropping = false;
    }

    public void StartFiring()
    {
        currentTemperature = 0f;
        isFiring = true;
        isWindowOpen = false;
        windValue = 0f;
        lowWindTimer = 0f;
        isTemperatureDropping = false;
    }

    public void BeginFiringByWind()
    {
        if (isFiring) return;
        if (windValue <= 0f) return;
        isFiring = true;
        isWindowOpen = false;
        lowWindTimer = 0f;
        isTemperatureDropping = false;
    }

    private void Update()
    {
        if (!isFiring) return;

        if (windValue < lowWindThreshold)
        {
            lowWindTimer += Time.deltaTime;
            if (lowWindTimer >= lowWindDurationThreshold)
            {
                isTemperatureDropping = true;
                currentTemperature -= temperatureDropPerSecond * Time.deltaTime;
                currentTemperature = Mathf.Max(0f, currentTemperature);
                return;
            }
        }
        else
        {
            lowWindTimer = 0f;
            isTemperatureDropping = false;
        }

        currentTemperature += temperatureRisePerSecond * windValue * Time.deltaTime;
    }
}
