using UnityEngine;
using System.Collections.Generic;

public class FiringSystem : MonoBehaviour
{
    [SerializeField] private float currentTemperature;
    [SerializeField] private float temperatureRisePerSecond = 50f;
    [SerializeField] private float windValue = 1f;
    [SerializeField] private float fuelBoost = 200f;
    [SerializeField] private bool isFiring = true;
    [SerializeField] private FireConfigSO fireConfig;

    private bool isWindowOpen;

    public enum FireZone { Underfired, Normal, Overfired }

    public float CurrentTemperature { get { return currentTemperature; } }
    public bool IsFiring { get { return isFiring; } }
    public bool IsWindowOpen { get { return isWindowOpen; } }

    public void SetWindValue(float value) { windValue = Mathf.Clamp01(value); }
    public void AddFuel() { currentTemperature += fuelBoost; }
    public void ToggleWindow() { isWindowOpen = !isWindowOpen; }

    public void StopFiring()
    {
        isFiring = false;
        isWindowOpen = true;
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
        windValue = 1f;
    }

    public void StartFiring()
    {
        currentTemperature = 0f;
        isFiring = true;
        isWindowOpen = false;
        windValue = 1f;
    }

    private void Update()
    {
        if (!isFiring) return;
        currentTemperature += temperatureRisePerSecond * windValue * Time.deltaTime;
    }
}
