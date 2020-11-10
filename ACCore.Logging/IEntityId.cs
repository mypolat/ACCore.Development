using System.ComponentModel.DataAnnotations;

namespace ACCore.Logging
{
    public interface IEntityId<T>
    {
        [Key]
        public T Id { get; set; }
    }
}
