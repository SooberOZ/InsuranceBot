namespace InsuranceBot.Models
{
    public class DocumentData
    {
        public string Name { get; set; }
        public string LastName { get; set; }    
        public string DriverLicenceId { get; set; }
        public string Vin { get; set; }
        public override string ToString()
        {
            return $"Name: {Name}, LastName: {LastName}, DriverLicenceId: {DriverLicenceId}, VIN: {Vin}";
        }
    }
}