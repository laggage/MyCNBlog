using MyCNBlog.Core.Interfaces;

namespace MyCNBlog.Core.Abstractions
{
    public abstract class Entity : IEntity<int>
    {
        public int Id { get; set; }
        /// <summary>
        /// Default is false
        /// </summary>
        public bool IsDeleted { get; set; } 
    }
}
