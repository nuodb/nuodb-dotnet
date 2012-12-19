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
            if (identifierPart is string)
            {
                if (withQuotes)
                    return String.Format("\"{0}\"", identifierPart);
                else
                    return (string)identifierPart;
            }
            return null;
        }

        protected override string[] SplitIntoParts(string typeName, string identifier)
        {
            return identifier.Split(new char[] { '.' });
        }

        protected override object UnformatPart(string typeName, string identifierPart)
        {
            return identifierPart;
        }
    }
}
