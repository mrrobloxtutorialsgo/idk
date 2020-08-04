using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class World : MonoBehaviour
{
    private static World _instance;
    public static World Instance { get { return _instance; } }

    public Texture2D[] terrainTextures;
    [HideInInspector]
    public Texture2DArray terrainTexArray;

    public int dayStartTime = 240;
    public int dayEndTime = 1360;
    private int dayLength { get { return dayEndTime - dayStartTime; } }
    private float sunDayRotationPerMinute { get { return 180f / dayLength; }  }
    private float sunNightRotationPerMinute { get { return 180f / (1440 - dayLength); } }

    public Transform sun;
    public TextMeshProUGUI clock;

    public int HordeNightFrequency = 7;

    public bool isHordeNight
    {
        get {
            if (Day % HordeNightFrequency == 0)
                return true;
            else
                return false;
        }
    }

    [Range(4f, 0f)]
    public float ClockSpeed = 1f;

    public int Day = 1;

    [SerializeField] private int _timeOfDay;
    public int TimeOfDay
    {
        get { return _timeOfDay; }
        set
        {
            _timeOfDay = value;

            if (_timeOfDay > 1439) {
                _timeOfDay = 0;
                Day++;
            }

            UpdateClock();

            float rotAmount;

            if (_timeOfDay > dayStartTime && _timeOfDay < dayEndTime)
            {
                rotAmount = (_timeOfDay - dayStartTime) * sunDayRotationPerMinute;
            } else if (_timeOfDay >= dayEndTime)
            {
                rotAmount = dayLength * sunDayRotationPerMinute;

                rotAmount += ((_timeOfDay - dayStartTime - dayLength) * sunNightRotationPerMinute);
            } else
            {
                rotAmount = dayLength * sunDayRotationPerMinute;
                rotAmount += (1440 - dayEndTime) * sunNightRotationPerMinute;
                rotAmount += _timeOfDay * sunNightRotationPerMinute;
            }

            sun.eulerAngles = new Vector3(rotAmount, 0f, 0f);
        }
    }

    private void UpdateClock()
    {
        int hours = TimeOfDay / 60;
        int minutes = TimeOfDay - (hours * 60);

        string dayText;
        if (isHordeNight)
            dayText = string.Format("<color=red>{0}</color>", Day.ToString());
        else
            dayText = Day.ToString();

        clock.text = string.Format("DAY: {0} TIME:  {1}:{2}", dayText, hours.ToString("D2"), minutes.ToString("D2"));
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("More than one instance of World Present. Removing additional instance.");
            Destroy(this.gameObject);
        }
        else
            _instance = this;

        PopulateTextureArray();
    }

    private float secondCounter = 0;

    private void Update()
    {
        secondCounter += Time.deltaTime;
        if (secondCounter > ClockSpeed)
        {
            TimeOfDay++;
            secondCounter = 0;
        }
    }

    void PopulateTextureArray()
    {
        terrainTexArray = new Texture2DArray(1024, 1024, terrainTextures.Length, TextureFormat.ARGB32, false);
        Debug.Log(terrainTexArray);
        for (int i = 0; i < terrainTextures.Length; i++)
        {
            terrainTexArray.SetPixels(terrainTextures[i].GetPixels(0), i, 0);
        }
        terrainTexArray.Apply();
    }
}
