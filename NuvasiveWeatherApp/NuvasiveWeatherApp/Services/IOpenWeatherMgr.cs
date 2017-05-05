namespace NuvasiveWeatherApp.Services
{
    #region using statements

    using System.Collections.Generic;

    using Models;

    #endregion

    public interface IOpenWeatherMgr
    {
        List<ForecastItem> FiveDayForecast { get; }

        TodaysWeatherOutlook TodaysWeatherOutlook { get; }

        string ZipCode { get; set; }
    }
}
