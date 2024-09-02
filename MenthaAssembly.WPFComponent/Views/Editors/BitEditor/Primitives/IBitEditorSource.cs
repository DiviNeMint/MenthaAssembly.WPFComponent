namespace MenthaAssembly.Views.Primitives
{
    public interface IBitEditorSource
    {
        public bool this[int Index] { set; get; }

        public int Count { get; }

    }
}