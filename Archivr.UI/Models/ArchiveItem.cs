using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Archivr.UI.Models
{
    public class ArchiveItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string? Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public ArchiveItemType ItemType { get; set; }

        public string? ParentId { get; set; }
        [ForeignKey(nameof(ParentId))]
        [JsonIgnore]
        public ArchiveItem? Parent { get; set; }
        [JsonIgnore]

        public List<ArchiveItem>? Children { get; set; }
    }
}
