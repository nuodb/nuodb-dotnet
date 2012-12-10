using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Data.AdoDotNet;

namespace NuoDB.VisualStudio.DataTools
{
    class NuoDBObjectIdentifierConverter : AdoDotNetObjectIdentifierConverter
    {
        public NuoDBObjectIdentifierConverter(Microsoft.VisualStudio.Data.DataConnection dataConnection)
            : base(dataConnection)
        {
        }

        protected override string FormatPart(string typeName, object identifierPart, bool withQuotes)
        {
            string s = base.FormatPart(typeName, identifierPart, withQuotes);
            return s;
        }

        protected override string[] SplitIntoParts(string typeName, string identifier)
        {
            string[] s = base.SplitIntoParts(typeName, identifier);
            return s;
        }

        protected override object UnformatPart(string typeName, string identifierPart)
        {
            object s = base.UnformatPart(typeName, identifierPart);
            return s;
        }
    }
}
