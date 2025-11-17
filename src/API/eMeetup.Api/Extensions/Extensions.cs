namespace eMeetup.Api.Extensions
{
    public static class Extensions
    {
        public static void AddApplicationServices(this IHostApplicationBuilder builder)
        {
            builder.AddRedisClient("redis");            
        }
    }
}
