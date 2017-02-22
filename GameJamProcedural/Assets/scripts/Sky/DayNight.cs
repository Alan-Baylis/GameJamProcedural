using UnityEngine;
using System.Collections;

public class DayNight : MonoBehaviour
{
    public Light sun;
    void Start()
    {
        // Creating everything needed to demonstrate this from a single cube
        player = this.transform;
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.transform.position = player.position + 0.5f * Vector3.down;
    }

    public Transform player;

    //public GameObject sun;
    public GameObject moon;

    public float radius = 6;

    public Color dayTimeSkyColor = new Color(0.31f, 0.88f, 1f);
    public Color midDaySkyColor = new Color(0.58f, 0.88f, 1f);
    public Color nightTimeSkyColor = new Color(0.04f, 0.19f, 0.27f);

    public Color dayTimeCloudColor = new Color(0.31f, 0.88f, 1f);
    public Color midDayTimeCloudColor = new Color(0.58f, 0.88f, 1f);
    public Color nightTimeCloudColor = new Color(0.04f, 0.19f, 0.27f);

    // implementing minecraft PC defaults
    public const float daytimeRLSeconds = 10.0f * 60;
    public const float duskRLSeconds = 1.5f * 60;
    public const float nighttimeRLSeconds = 7.0f * 60;
    public const float sunsetRLSeconds = 1.5f * 60;
    public const float gameDayRLSeconds = daytimeRLSeconds + duskRLSeconds + nighttimeRLSeconds + sunsetRLSeconds;

    public const float startOfDaytime = 0;
    public const float startOfDusk = daytimeRLSeconds / gameDayRLSeconds;
    public const float startOfNighttime = startOfDusk + duskRLSeconds / gameDayRLSeconds;
    public const float startOfSunset = startOfNighttime + nighttimeRLSeconds / gameDayRLSeconds;


    private float timeRT = 0;
    public float TimeOfDay // game time 0 .. 1
    {
        get { return timeRT / gameDayRLSeconds; }
        set { timeRT = value * gameDayRLSeconds; }
    }

    void Update()
    {
        timeRT = (timeRT + Time.deltaTime) % gameDayRLSeconds;
        Shader.SetGlobalColor("_SkyColor", CalculateColor(dayTimeSkyColor,midDaySkyColor,nightTimeSkyColor));
        Shader.SetGlobalColor("_CloudColor", CalculateColor(dayTimeCloudColor,midDayTimeCloudColor,nightTimeCloudColor));
        Shader.SetGlobalFloat("_TimeOfDay", timeRT);
        float sunangle = TimeOfDay * 360;
        Vector3 midpoint = player.position; midpoint.y -= 0.5f; //midpoint = playerposition at floor height
        sun.transform.position = midpoint + Quaternion.Euler(0, 0, sunangle) * (radius * Vector3.right);
        sun.transform.LookAt(midpoint);
    }

    Color CalculateColor(Color dayTime, Color midDay, Color nightTime)
    {
        float time = TimeOfDay;
        if (time <= 0.25f)
            return Color.Lerp(dayTime, midDay, time / 0.25f);
        if (time <= 0.5f)
            return Color.Lerp(midDay, dayTime, (time - 0.25f) / 0.25f);
        if (time <= startOfNighttime)
            return Color.Lerp(dayTime, nightTime, (time - startOfDusk) / (startOfNighttime - startOfDusk));
        if (time <= startOfSunset) return nightTime;
        return Color.Lerp(nightTime, dayTime, (time - startOfSunset) / (1.0f - startOfSunset));
    }


    void OnGUI()
    {
        Rect rect = new Rect(10, 10, 120, 20);
        GUI.Label(rect, "time: " + TimeOfDay); rect.y += 20;
        GUI.Label(rect, "timeRT: " + timeRT);
        rect = new Rect(120, 10, 200, 10);
        TimeOfDay = GUI.HorizontalSlider(rect, TimeOfDay, 0, 1);
    }
}