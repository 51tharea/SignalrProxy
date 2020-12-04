namespace SampleApplication.Responses
{
    public class ServiceResponse
    {
        public bool Status { get; set; } = false;
        public object Data { get; set; } = null;
        public string Message { get; set; } = null;
        public bool Error { get; set; } = false;
    }
}