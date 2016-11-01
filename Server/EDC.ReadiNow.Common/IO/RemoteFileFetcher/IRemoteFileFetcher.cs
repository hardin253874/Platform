namespace EDC.ReadiNow.IO.RemoteFileFetcher
{
    public interface IRemoteFileFetcher
    {
        string FetchToTemporaryFile(string url, string username, string password);
    }
}