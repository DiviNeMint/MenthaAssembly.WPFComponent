using System;
using System.ComponentModel.DataAnnotations;

namespace MenthaAssembly
{
    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
    public sealed class EditorOptionAttribute : Attribute
    {
        public string NamePath { set; get; }

        public string TypeName { set; get; }

        public string IconPath { set; get; }

        public bool EnableContentPropertyChangedEvent { set; get; }


        //public double ValueDelta { get; set; }

        //public double ValueCtrlDelta { get; set; }

        //public double ValueMaximum { set; get; } = double.MaxValue;

        //public double ValueMinimum { get; set; } = double.MinValue;

    }
}
