using Intertwine.Domain.Interfaces;

namespace Intertwine.Domain.Abstractions
{
    public class ActivatableEntity : BaseEntity, IActivatable
    {
        public bool IsActive { get; set; } = true;
    }
}
