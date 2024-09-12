namespace ApiApplication.Configurations
{
    public class MoviesApiConfig
    {
        public MoviesApiConfig()
        {
            CacheExpiryInMinutes = 30;
        }

        public string ServiceUrl { get; set; }
        public string ApiKey { get; set; }
        public int CacheExpiryInMinutes { get; set; }
    }
}
