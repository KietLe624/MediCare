using MediCare.API.Common;

namespace MediCare.API.Entities
{
    public class Medication : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? GenericName { get; set; }
        public string? Category { get; set; }
        public string? Unit { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
