namespace SignalrProxy
{
    public class HubClientOptions
    {
        public string ConnectType { get; set; }
        public string DisconnectType { get; set; }
        public string UserDetectedType { get; set; }
        public int InitialWorkerCount { get; set; }
    }
}