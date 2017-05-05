namespace NuvasiveWeatherApp.Models
{
    #region using statements

    using Prism.Mvvm;

    #endregion

    /// <summary>
    /// This class servers as a Data Transfer Object (DTO) for one day of a forecast
    /// </summary>
    public class ForecastItem : BindableBase
    {
        #region Fields

        private string _todayDate;
        private string _maxTemp;
        private string _minTemp;

        #endregion

        #region Properties

        public string TodayDate
        {
            get { return _todayDate; }
            set
            {
                _todayDate = value;
                SetProperty(ref _todayDate, value);
            }
        }

        public string MaxTemp
        {
            get { return _maxTemp; }
            set
            {
                _maxTemp = value;
                SetProperty(ref _maxTemp, value);
            }
        }

        public string MinTemp
        {
            get { return _minTemp; }
            set
            {
                _minTemp = value;
                SetProperty(ref _minTemp, value);
            }
        }

        #endregion
    }
}
