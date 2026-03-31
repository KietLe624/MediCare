using MediCare.API.Common;

namespace MediCare.API.Entities
{
    public class Department : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public virtual ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
        public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
    }
}
