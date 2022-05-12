namespace ordercloud.integrations.library
{
    public static class DecimalExtensions
    {
        public static bool IsNullOrZero(this decimal? value)
        {
            return value == null || value == 0;
        }
    }
}
