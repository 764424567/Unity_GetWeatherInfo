using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#region 返回的城市名字等数据类
public class CityData
{
    public string address;
    public Content content;
    public int status;

}
public class Content
{
    public string address;
    public Address_Detail address_detail;
    public Point point;
}
public class Address_Detail
{
    public string city;
    public int city_code;
    public string district;
    public string province;
    public string street;
    public string street_number;
}

public class Point
{
    public string x;
    public string y;
}
#endregion

#region 城市的city_code编号代码
public class CityCode
{
    public int id;
    public int pid;
    public string city_code;
    public string city_name;
    public string post_code;
    public string area_code;
    public string ctime;
}
#endregion

#region 天气数据类
public class WeatherData
{
    public string message;
    public int status;
    public string date;
    public string time;
    public CityInfo cityInfo;
    public WeathData data;
}
public class CityInfo
{
    public string city;
    public string cityId;
    public string parent;
    public string updateTime;
}
public class WeathData
{
    public string shidu;
    public double pm25;
    public double pm10;
    public string quality;
    public string wendu;
    public string ganmao;
    public WeathDetailData[] forecast;
    public WeathDetailData yesterday;
}
public class WeathDetailData
{
    public string date;
    public string sunrise;
    public string high;
    public string low;
    public string sunset;
    public double aqi;
    public string ymd;
    public string week;
    public string fx;
    public string fl;
    public string type;
    public string notice;
}
#endregion

public class WeatherTools : MonoBehaviour
{
    public static Dictionary<string, string> PosToId = new Dictionary<string, string>();
    public static bool initDic = false;

    //UI显示
    public Text m_TextCityName;
    public Text m_TextQuality;
    public Text m_TextNotice;
    public Image[] m_ImageType;
    //今天
    public Text m_TextTodayDate;
    
    public Text m_TextTodayType;
    public Text m_TextTodayTemperature;
    public Text m_TextTodayfx;
    //明天
    public Text m_TextTomorrowDate;
    public Text m_TextTomorrowType;
    public Text m_TextTomorrowTemperature;
    public Text m_TextTomorrowfx;
    //后天
    public Text m_TextAcquiredDate;
    public Text m_TextAcquiredType;
    public Text m_TextAcquiredTemperature;
    public Text m_TextAcquiredfx;

    /// <summary>
    /// 获取位置信息
    /// </summary>
    string Posurl = "http://api.map.baidu.com/location/ip?ak=bretF4dm6W5gqjQAXuvP0NXW6FeesRXb&coor=bd09ll";
    /// <summary>
    /// 获取天气信息
    /// </summary>
    string Weatherurl = "http://t.weather.sojson.com/api/weather/city/";

    void Start()
    {
        //获取位置
        StartCoroutine(RequestCityName());
    }

    IEnumerator RequestCityName()
    {
        WWW www = new WWW(Posurl);
        yield return www;
        if (string.IsNullOrEmpty(www.error))
        {
            CityData cityData = LitJson.JsonMapper.ToObject<CityData>(www.text);
            Debug.Log(cityData.content.address_detail.city);
            //获取city_code
            Debug.Log(GetWeatherId(cityData.content.address_detail.city));
            //获取天气信息
            string city_code = GetWeatherId(cityData.content.address_detail.city);
            StartCoroutine(RequestWeatherData(city_code));
        }
    }

    public static string GetWeatherId(string name)
    {
        string city_code = "";
        if (!initDic)
        {
            initDic = true;
            TextAsset city = Resources.Load<TextAsset>("city");
            List<CityCode> cityCode = LitJson.JsonMapper.ToObject<List<CityCode>>(city.text);
            foreach (CityCode t in cityCode)
            {
                PosToId[t.city_name] = t.city_code;
            }
        }
        for (int i = 1; i < name.Length; i++)
        {
            string tn = name.Substring(0, i);
            if (PosToId.ContainsKey(tn))
            {
                city_code = PosToId[tn];
            }
        }
        return city_code;
    }

    IEnumerator RequestWeatherData(string cicy_code)
    {
        WWW www = new WWW(Weatherurl + cicy_code);
        Debug.Log(Weatherurl + cicy_code);
        yield return www;
        if (string.IsNullOrEmpty(www.error))
        {
            Debug.Log(www.text);
            WeatherData t = LitJson.JsonMapper.ToObject<WeatherData>(www.text);
            //UI显示数据
            WeatherData_UIShow(t);
        }
    }

    public void WeatherData_UIShow(WeatherData _weatherData)
    {
        m_TextCityName.text = _weatherData.cityInfo.city;
        m_TextQuality.text = _weatherData.data.quality;
        m_TextNotice.text = _weatherData.data.forecast[0].notice;
        string[] data = WeatherData_Parse(_weatherData);
        //今天
        m_TextTodayDate.text = data[0];
        m_TextTodayType.text = data[1];
        m_TextTodayTemperature.text = data[2];
        m_TextTodayfx.text = data[3];
        //明天
        m_TextTomorrowDate.text = data[4];
        m_TextTomorrowType.text = data[5];
        m_TextTomorrowTemperature.text = data[6];
        m_TextTomorrowfx.text = data[7];
        //后天
        m_TextAcquiredDate.text = data[8];
        m_TextAcquiredType.text = data[9];
        m_TextAcquiredTemperature.text = data[10];
        m_TextAcquiredfx.text = data[11];
    }

    public string[] WeatherData_Parse(WeatherData _weatherData)
    {
        string[] data = new string[12];
        for (int i = 0; i < 3; i++)
        {
            //图片显示
            string path = "weather/" + _weatherData.data.forecast[i].type;
            m_ImageType[i].sprite = Resources.Load(path, typeof(Sprite)) as Sprite;
            //数据计算
            string temperatureLow = _weatherData.data.forecast[i].low;
            string temperatureHigh = _weatherData.data.forecast[i].high;
            temperatureLow = temperatureLow.Substring(3, temperatureLow.Length - 3);
            temperatureHigh = temperatureHigh.Substring(3, temperatureHigh.Length- 3);
            //String数组存一下拼接好的字符串
            data[i * 4 + 0] = _weatherData.data.forecast[i].date + "日   " + _weatherData.data.forecast[i].week;
            data[i * 4 + 1] = _weatherData.data.forecast[i].type;
            data[i * 4 + 2] = temperatureLow + " ~ " + temperatureHigh;
            data[i * 4 + 3] = _weatherData.data.forecast[i].fx;
        }
        return data;
    }
}
