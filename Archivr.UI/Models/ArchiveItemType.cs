using System.Text.Json.Serialization;

namespace Archivr.UI.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ArchiveItemType
    {
        File = 10,
        GenericItem = 11,
        Box = 20,
        Locker = 30,
        StorageFacility = 40,
    }
}
