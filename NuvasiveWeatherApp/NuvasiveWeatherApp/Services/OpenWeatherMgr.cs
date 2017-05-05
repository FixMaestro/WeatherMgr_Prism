namespace NuvasiveWeatherApp.Services
{
    #region using statements

    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using Models;

    using Prism.Events;

    #endregion

    public class OpenWeatherMgr : IOpenWeatherMgr
    {
        #region Fields

        private const string BaseUrltoWeatherSvc = "http://api.openweathermap.org/data/2.5/";
        private const string TempType = "&units=imperial";
        private const string RequestKeyParm = "&appid=<<<KEY GOES HERE>>>";

        private IEventAggregator _eventAggregator;

        private string _cityId;
        private List<ForecastItem> _forecastWeather;
        private TodaysWeatherOutlook _todaysWeather;
        private string _zipCode;

        #endregion

        #region Constructor

        public OpenWeatherMgr(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        #endregion

        #region Properties

        public List<ForecastItem> FiveDayForecast => _forecastWeather;

        public TodaysWeatherOutlook TodaysWeatherOutlook => _todaysWeather;

        public string ZipCode
        {
            get
            {
                return _zipCode;
            }
            set
            {
                _zipCode = value;
                UpdateWeatherData();
            }
        }

        #endregion

        #region Methods

        private async Task GetFiveDayForcast()
        {
            if (string.IsNullOrEmpty(_cityId))
            {
                // blank results for failure
                _forecastWeather = null;
            }

            using (var httpClient = new HttpClient())
            {
                try
                {
                    _forecastWeather = null;

                    var jsonData = await httpClient.GetStringAsync($"{BaseUrltoWeatherSvc}forecast?id={_cityId}{RequestKeyParm}{TempType}");
                    var response = (JContainer)JsonConvert.DeserializeObject(jsonData);

                    var resultCountString = (string) response["cnt"];
                    if (string.IsNullOrEmpty(resultCountString))
                    {
                        return;
                    }

                    int resultCount;
                    if (!int.TryParse(resultCountString, out resultCount))
                    {
                        return;
                    }

                    var currDate = DateTime.Today;
                    var results = new List<ForecastItem>();

                    // the data spans 8 readings per day
                    for (var reading = 0; reading < resultCount; reading += 8)
                    {
                        currDate = currDate.AddDays(1.0d);
                        var maxTemp = 0.0m;
                        var minTemp = decimal.MaxValue;

                        // scan a day's worth of readings to find the lowest and highest values
                        for (var dayReading = reading; (dayReading < reading+8) && (dayReading < resultCount); dayReading++)
                        {
                            string currMinRead;
                            string currMaxRead;

                            try
                            {
                                currMinRead = (string) response["list"][dayReading]["main"]["temp_min"];
                                currMaxRead = (string) response["list"][dayReading]["main"]["temp_max"];
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine(e);
                                throw;
                            }

                            decimal dTemp;

                            if (decimal.TryParse(currMinRead, out dTemp))
                            {
                                if (minTemp > dTemp)
                                {
                                    minTemp = dTemp;
                                }
                            }

                            if (!decimal.TryParse(currMaxRead, out dTemp)) continue;

                            if (maxTemp < dTemp)
                            {
                                maxTemp = dTemp;
                            }
                        }

                        results.Add(new ForecastItem()
                        {
                            TodayDate = currDate.ToString("d"),
                            MaxTemp = maxTemp.ToString("F"),
                            MinTemp = minTemp.ToString("F")
                        });
                    }

                    _forecastWeather = results;

                    // at this point we'll fire off an event to tell the caller that they have new data
                    SendWeatherEvent();
                }
                catch (Exception e)
                {
                    var m = e.Message;

                    // blank results for failure
                    _forecastWeather = null;
                }
            }
        }

        private async Task GetTodayWeatherAsync()
        {
            using (var httpClient = new HttpClient())
            {
                try
                {
                    var jsonData = await httpClient.GetStringAsync($"{BaseUrltoWeatherSvc}weather?zip={_zipCode}{RequestKeyParm}{TempType}");
                    var response = (JContainer)JsonConvert.DeserializeObject(jsonData);

                    _todaysWeather = new TodaysWeatherOutlook
                    {
                        CityName = (string)response["name"],
                        CurrentTemp = (string)response["main"]["temp"],
                        MinTemp = (string)response["main"]["temp_min"],
                        MaxTemp = (string)response["main"]["temp_max"]
                    };

                    _cityId = (string)response["id"];

                    // we now have enough information to startup the 5 day forecast now
                    await GetFiveDayForcast().ConfigureAwait(false);
                }
                catch (Exception)
                {
                    // blank results for failure
                    _cityId = null;
                    _todaysWeather = null;
                }
            }
        }

        private void SendWeatherEvent()
        {
            var weatherEvent = _eventAggregator.GetEvent<WeatherEvent>();
            weatherEvent?.Publish();
        }

        /// <summary>
        /// Call out to the weather service and get all required data
        /// </summary>
        private async void UpdateWeatherData()
        {
            // kick off today's weather and it will chain the call to pick up the 5 day forcast when it's done
            await GetTodayWeatherAsync().ConfigureAwait(false);
        }

        #endregion
    }
}
