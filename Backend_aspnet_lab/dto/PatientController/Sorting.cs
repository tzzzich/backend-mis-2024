using System.Text.Json.Serialization;

namespace Backend_aspnet_lab.dto.PatientController
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Sorting
    {
        NameAsc, 
        NameDesc, 
        CreateAsc, 
        CreateDesc, 
        InspectionAsc, 
        InspectionDesc
    }
}
