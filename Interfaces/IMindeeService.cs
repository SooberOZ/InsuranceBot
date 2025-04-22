using InsuranceBot.Models;

namespace InsuranceBot.Interfaces
{
    public interface IMindeeService
    {
        Task<DocumentData> ExtractAsync(string filePath, string documentType);
        DocumentData SimulateVehicleDocumentProcessing(string filePath);

    }
}