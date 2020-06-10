using System.Windows;
using System.Windows.Media;

namespace MenthaAssembly.Devices
{
    public partial class CursorResource : ResourceDictionary
    {
        private static CursorResource _Instance;
        public static CursorResource Instance
        {
            get
            {
                if (_Instance is null)
                    _Instance = new CursorResource();

                return _Instance;
            }
        }

        public CursorResource()
        {
            InitializeComponent();
        }

        public DrawingImage this[string Key]
            => base[Key] as DrawingImage;

        public DrawingImage Eyedropper
            => this[nameof(Eyedropper)];

        public DrawingImage GrabHand
            => this[nameof(GrabHand)];

        public DrawingImage RotateArrow
            => this[nameof(RotateArrow)];

    }
}
