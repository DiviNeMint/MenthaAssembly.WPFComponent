namespace MenthaAssembly.Views.Primitives
{
    public interface IBitsSource
    {
        public bool this[int Index] { set; get; }

        public int Count { get; }

    }
}