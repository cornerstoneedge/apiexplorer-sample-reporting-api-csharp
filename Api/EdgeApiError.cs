namespace Api
{
    public class EdgeApiErrorValue
    {
        public EdgeApiError Error { get; set; }
    }

    public class EdgeApiError
    {
        public string Code { get; set; }

        public string Message { get; set; }
    }
}