namespace NuvasiveWeatherApp.Models
{
    /// <summary>
    /// This class servers as a Data Transfer Object (DTO) for Today's Weather Outlook
    /// </summary>
    public class TodaysWeatherOutlook
    {
        public string CityName { get; set; }

        public string CurrentTemp { get; set; }

        public string MinTemp { get; set; }

        public string MaxTemp { get; set; }
    }
}
