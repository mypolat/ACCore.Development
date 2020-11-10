using ACCore.Logging;
using System;
using System.ComponentModel.DataAnnotations;

namespace ACCore.TestProject
{
    public class Person : IEntityId<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
    }
}
