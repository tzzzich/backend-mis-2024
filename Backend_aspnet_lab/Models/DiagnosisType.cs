using System.Text.Json.Serialization;

namespace Backend_aspnet_lab.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DiagnosisType
    {
        Main,
        Concomitant,
        Complication
    }
}
