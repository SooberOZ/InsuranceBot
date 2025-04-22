namespace InsuranceBot.Interfaces
{
    public interface IOpenAIService
    {
        Task<string> SendMessageAndGetResponse(string prompt);
    }
}