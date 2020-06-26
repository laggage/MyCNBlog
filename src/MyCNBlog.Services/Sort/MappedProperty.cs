namespace MyCNBlog.Services.Sort
{
    public struct MappedProperty
    {
        public MappedProperty(string name, bool revert = false)
        {
            Name = name;
            Revert = revert;
        }

        public string Name { get; set; }
        public bool Revert { get; set; }
    }
}
