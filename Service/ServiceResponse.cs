namespace BlazorDemoCRUD.Service
{
    public class ServiceResponse<T>
    {
        public T Data { get; set; }

        public bool Success { get; set; } = true;
        public string Message { get; set; }
    }

    public class ServiceResponse
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; }
    }

    public class ServiceResponseKeyValue<T, T1>
    {
        public T Key { get; set; }
        public T1 Value { get; set; }
    }
}
