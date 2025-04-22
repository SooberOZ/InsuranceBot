using InsuranceBot.Interfaces;
using InsuranceBot.Models;
using Mindee;
using Mindee.Input;
using Mindee.Product.DriverLicense;

namespace InsuranceBot.Services
{
    public class MindeeService: IMindeeService
    {
        private readonly MindeeClient _mindeeClient;

        public MindeeService(MindeeClient mindeeClient)
        {
            _mindeeClient = mindeeClient;
        }

        public async Task<DocumentData> ExtractAsync(string filePath, string documentType)
        {
            
            // Загружаем файл как источник
            var inputSource = new LocalInputSource(filePath);

            // Отправляем файл и автоматически обрабатываем polling
            if (documentType == "DriverLicense")
            {
                // Обработка водительских прав
                var response = await _mindeeClient.EnqueueAndParseAsync<DriverLicenseV1>(inputSource);
                var predictions = response.Document.Inference.Prediction;

                return new DocumentData
                {
                    Name = predictions.FirstName.Value.ToString(),
                    LastName = predictions.LastName.Value.ToString(),
                    DriverLicenceId = predictions.Id.Value.ToString()
                };
            }
            else if (documentType == "VehicleDocument")
            {
                // Мок-обработка документов на машину
                return SimulateVehicleDocumentProcessing(filePath);
            }
            else
            {
                throw new ArgumentException("Unsupported document type");
            }
        }

        //TODO: Реализовать обработку документов на машину
        public DocumentData SimulateVehicleDocumentProcessing(string filePath)
        {
            // Симуляция обработки документа на машину
            return new DocumentData
            {
                Name = "Иван",
                LastName = "Иванов",
                Vin = "1HGCM82633A123456"
            };
        }
    }
}