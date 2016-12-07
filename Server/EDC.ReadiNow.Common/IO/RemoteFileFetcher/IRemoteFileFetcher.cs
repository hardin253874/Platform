namespace EDC.ReadiNow.IO.RemoteFileFetcher
{
    public interface IRemoteFileFetcher
    {
        string GetToTemporaryFile(string url, string username, string password);

        void PutFromTemporaryFile(string fileHash, string url, string username, string password);
    }
}