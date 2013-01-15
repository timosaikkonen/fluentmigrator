using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace FluentMigrator.Model
{
    public interface IVersionMetadata : IXmlSerializable
    {
        object this[string key] { get; set; }
    }

    [Serializable]
    public class VersionMetadata : IVersionMetadata
    {
        public VersionMetadata()
        {
            _metadata = new Dictionary<string, object>();
        }
        public VersionMetadata(XmlReader reader)
            : this()
        {
            ReadXml(reader);
        }

        private Dictionary<string, object> _metadata;

        public object this[string key]
        {
            get { return _metadata[key]; }
            set { _metadata[key] = value; }
        }

        public VersionMetadataItem[] Metadata
        {
            get { return _metadata.Select(m => new VersionMetadataItem() { Key = m.Key, Value = m.Value }).ToArray(); }
            set { _metadata = value == null ? new Dictionary<string, object>() : value.ToDictionary(pair => pair.Key, pair => pair.Value); }
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            var serializer = new XmlSerializer(typeof(VersionMetadataItem));

            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty)
                return;

            reader.ReadStartElement("metadata");

            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                reader.ReadStartElement("item");

                var item = (VersionMetadataItem)serializer.Deserialize(reader);
                _metadata.Add(item.Key, item.Value);

                reader.ReadEndElement();
                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            var serializer = new XmlSerializer(typeof(VersionMetadataItem));

            writer.WriteStartElement("metadata");

            foreach (var item in Metadata)
            {
                writer.WriteStartElement("item");
                serializer.Serialize(writer, item);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }
    }

    [Serializable]
    public class VersionMetadataItem
    {
        [XmlElement(ElementName = "key")]
        public string Key { get; set; }

        [XmlElement(ElementName = "value")]
        public object Value { get; set; }
    }
}
