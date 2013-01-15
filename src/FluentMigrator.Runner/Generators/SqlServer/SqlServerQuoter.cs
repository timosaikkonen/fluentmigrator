using System.Text;
using System.Xml;
using FluentMigrator.Runner.Generators.Generic;

namespace FluentMigrator.Runner.Generators.SqlServer
{
    public class SqlServerQuoter : GenericQuoter
    {
        public override string QuoteValue(object value)
        {
            var document = value as XmlDocument;
            if (document != null)
            {
                return "N" + ValueQuote + document.OuterXml + ValueQuote;
            }
            return base.QuoteValue(value);
        }

        public override string OpenQuote { get { return "["; } }

        public override string CloseQuote { get { return "]"; } }

        public override string CloseQuoteEscapeString { get { return "]]"; } }

        public override string OpenQuoteEscapeString { get { return string.Empty; } }

        public override string QuoteSchemaName(string schemaName)
        {
            return (string.IsNullOrEmpty(schemaName)) ? "[dbo]" : Quote(schemaName);
        }
    }
}
