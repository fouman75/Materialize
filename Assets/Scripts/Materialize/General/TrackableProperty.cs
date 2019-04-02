#region

using System.Xml.Serialization;

#endregion

namespace Materialize.General
{
    public struct TrackableProperty
    {
        private float _value;

        public float Value
        {
            get
            {
                Changed = false;
                return _value;
            }
            set
            {
                _value = value;
                Changed = true;
            }
        }

        [XmlIgnore]
        // ReSharper disable once ConvertToAutoPropertyWhenPossible
        public float Anonymous
        {
            get => _value;
            set => _value = value;
        }

        public static implicit operator float(TrackableProperty prop)
        {
            return prop._value;
        }

        [XmlIgnore] public bool Changed { get; private set; }
    }
}