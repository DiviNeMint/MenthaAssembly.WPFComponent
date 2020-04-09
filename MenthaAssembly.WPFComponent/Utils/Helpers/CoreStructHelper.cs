using System.Windows;

namespace MenthaAssembly
{
    public static class CoreStructHelper
    {

        public static Vector ToVector(this Int32Vector This)
            => new Vector(This.X, This.Y);



    }
}
