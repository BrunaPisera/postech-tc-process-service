namespace ProcessService.APP.Interfaces
{
    public interface IVideoProcessor
    {
        Task<string> ProcessAsync(byte[] videoBytes);
    }
}