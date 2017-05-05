namespace NuvasiveWeatherApp.ViewModels
{
    #region using statements

    using System;
    using System.Collections.Generic;

    using Services;
    using Models;

    using Prism.Commands;
    using Prism.Mvvm;
    using Prism.Navigation;
    using Prism.Events;

    #endregion

    public class MainPageViewModel : BindableBase, INavigationAware
    {
        #region Fields

        private DelegateCommand _getWeatherCommand;

        private List<ForecastItem> _fiveDayForecast;

        private readonly IOpenWeatherMgr _weatherMgr;

        private string _cityName;
        private string _currTemp;
        private string _lastZipCode;
        private string _minTemp;
        private string _maxTemp;
        private string _title;
        private string _zipCode;
        private bool _zipCodeIsReset;

        #endregion

        #region Constructor

        public MainPageViewModel(IEventAggregator eventAggregator, IOpenWeatherMgr weatherMgr)
        {
            _weatherMgr = weatherMgr;

            eventAggregator.GetEvent<WeatherEvent>().Subscribe(OnWeatherChangedEvent);
        }

        #endregion

        #region Properties

        public string CityName
        {
            get { return _cityName; }
            set { SetProperty(ref _cityName, value); }
        }

        public string CurrTemp
        {
            get { return _currTemp; }
            set { SetProperty(ref _currTemp, value); }
        }

        public DelegateCommand GetWeatherCommand =>
            _getWeatherCommand ?? (_getWeatherCommand = new DelegateCommand(() =>
            {
                // TODO: all exception throwing is evil in mobile devices, but I need more time to handle them in a better way
                if (_weatherMgr == null)
                    throw new Exception("Internal failure while trying to access weather service");

                // rerun the weather getter if the user has input a new zipcode
                if (!CanGetWeather) return;

                CanGetWeather = false;
                _lastZipCode = _zipCode;
                _weatherMgr.ZipCode = _zipCode;
            }));

        public bool CanGetWeather
        {
            get { return _zipCodeIsReset; }
            set
            {
                _zipCodeIsReset = value;
                OnPropertyChanged(() => CanGetWeather);
            }
        }

        public string MinTemp
        {
            get { return _minTemp; }
            set { SetProperty(ref _minTemp, value); }
        }

        public string MaxTemp
        {
            get { return _maxTemp; }
            set { SetProperty(ref _maxTemp, value); }
        }

        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public string ZipCode
        {
            get { return _zipCode; }
            set
            {
                if (SetProperty(ref _zipCode, value))
                {
                    CanGetWeather = !string.IsNullOrEmpty(_zipCode) && _lastZipCode != _zipCode;
                }
            }
        }

        public List<ForecastItem> FiveDayForecast
        {
            get { return _fiveDayForecast; }
            set
            {
                _fiveDayForecast = value;
                OnPropertyChanged(() => FiveDayForecast);
            }
        }

        #endregion

        #region Events

        public void OnNavigatedFrom(NavigationParameters parameters)
        {

        }
        
        public void OnNavigatedTo(NavigationParameters parameters)
        {
            CanGetWeather = false;
        }

        public void OnNavigatingTo(NavigationParameters parameters)
        {
            
        }

        public void OnWeatherChangedEvent()
        {
            var todayWeather = _weatherMgr.TodaysWeatherOutlook;
            if (todayWeather != null)
            {
                CityName = todayWeather.CityName;
                CurrTemp = todayWeather.CurrentTemp;
                MinTemp = todayWeather.MinTemp;
                MaxTemp = todayWeather.MaxTemp;
            }

            var forecast = _weatherMgr.FiveDayForecast;
            if (forecast != null)
            {
                FiveDayForecast = new List<ForecastItem>(forecast);
            }
        }

        #endregion
    }
}
