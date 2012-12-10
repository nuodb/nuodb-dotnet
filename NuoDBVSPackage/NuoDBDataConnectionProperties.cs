using Microsoft.VisualStudio.Data.AdoDotNet;
using Microsoft.VisualStudio.Data;

namespace NuoDB.VisualStudio.DataTools
{
    public class NuoDBDataConnectionProperties : AdoDotNetConnectionProperties
    {
        public NuoDBDataConnectionProperties() 
            : base("System.Data.NuoDB")
        {
            System.Diagnostics.Trace.WriteLine("NuoDBDataConnectionProperties()");
        }

        public override string[] GetBasicProperties()
        {
            System.Diagnostics.Trace.WriteLine("NuoDBDataConnectionProperties::GetBasicProperties()");

            return new string[] { "Server", "User", "Password", "Database" };
        }

        protected override void InitializeProperties()
        {
            System.Diagnostics.Trace.WriteLine("NuoDBDataConnectionProperties::InitializeProperties()");

            base.InitializeProperties();

            this.AddProperty("Server", typeof(System.String));
            this.AddProperty("User", typeof(System.String));
            this.AddProperty("Password", typeof(System.String));
            this.AddProperty("Database", typeof(System.String));
            this.AddProperty("Schema", typeof(System.String));
        }

    }
}
