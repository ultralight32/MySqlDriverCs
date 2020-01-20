using System.Globalization;
using System.Text;

namespace MySqlDriverCs
{
    internal class CollationEntry
    {
        public int CollationId { get; }
        public string Charset { get; }
        public string Collation { get; }
        public string Def { get; }
        public string Sortlen { get; }

        public CollationEntry(string collationId, string charset, string collation, string def, string sortlen)
        {
            CollationId = int.Parse(collationId, CultureInfo.InvariantCulture);
            Charset = charset;
            Collation = collation;
            Def = def;
            Sortlen = sortlen;
        }

        public Encoding Encoding
        {
            get
            {
                if (Charset == "utf8")
                    return new UTF8Encoding();
                else
                {
                    return new ASCIIEncoding();
                }
            }
        }
    }
}