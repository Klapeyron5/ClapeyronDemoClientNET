using System;
using System.Drawing;

namespace ObjectRecognition
{
    [Serializable()]
    public class ObjectColorData
    {

        public ObjectColorData()
        {
        }

        public ObjectColorData(Color color)
        {
            this.ObjectColor = color;
        }

        [System.Xml.Serialization.XmlIgnore]
        public Color ObjectColor;

        [System.Xml.Serialization.XmlElement(ElementName = "ObjectColor")]
        public string ObjectColor_XmlSurrogate
        {
            get { return ColorConverter.ToXmlString(ObjectColor); }
            set { ObjectColor = ColorConverter.FromXmlString(value); }
        }
    }
}
