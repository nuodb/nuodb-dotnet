// Guids.cs
// MUST match guids.h
using System;

namespace NuoDB.VisualStudio.DataTools
{
    static class GuidList
    {
        public const string guidNuoDBVSPackagePkgString = "407f565d-8772-4770-8243-5a1f396aae9d";
        public const string guidNuoDBVSPackageCmdSetString = "41f5f498-5d4f-4811-844e-6e6bd65125c8";
        public const string guidNuoDBObjectFactoryServiceString = "41f5f498-5d4f-4811-844e-6e6bd65125c9";
        public const string guidNuoDBDataSourceString = "0DFA90C2-CB54-4889-B3F4-77BA5E28FAAC";
        public const string guidNuoDBDataProvider = "80AF79F8-2574-48FB-9E78-3ABB020ABB3F";

        public static readonly Guid guidNuoDBVSPackageCmdSet = new Guid(guidNuoDBVSPackageCmdSetString);
        public static readonly Guid guidNuoDBObjectFactoryService = new Guid(guidNuoDBObjectFactoryServiceString);
        public static readonly Guid guidNuoDBDataSource = new Guid(guidNuoDBDataSourceString);
    };
}