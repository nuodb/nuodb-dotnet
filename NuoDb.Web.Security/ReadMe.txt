This directory contains a basic implementation of a .NET Membership provider for NuoDB.
An ASP.NET application can use this Membership provider by doing the following:

1) Startup a NuoDB database and create or select a schema which will be used to hold the membership data.

2) Modify the applications web.config to use the NuoDB .NET Membership provider.  Below is
   an example web.config that uses as the archive for the users' data, the schema "MEMBERS" for the 
   database test@localhost, using the username "dba" and password "goalie". Upon startup, the 
   membership provider will create the table Users, if not already defined.

  <?xml version="1.0"?>
  <configuration>
    <connectionStrings>
      <add name="NuoDbServices" connectionString="Server=localhost;Database=test;User=dba;Password=goalie;Schema=MEMBERS;Pooling=True;"/>
    </connectionStrings>
    <system.web>
      <authentication mode="Forms"/>
      <machineKey validationKey="C50B3C89CB21F4F1422FF158A5B42D0E8DB8CB5CDA1742572A487D9401E3400267682B202B746511891C1BAF47F8D25C07F6C39A104696DB51F17C529AD3CABE" decryptionKey="8A9BE8FD67AF6979E7D20198CFEA50DD3D3799C77AF2B72F" validation="SHA1"/>
      <membership defaultProvider="NuoDbProvider">
        <providers>
          <add connectionStringName="NuoDbServices" enablePasswordRetrieval="true" enablePasswordReset="true" requiresQuestionAndAnswer="true" writeExceptionsToEventLog="true" name="NuoDbProvider" type="NuoDb.Web.Security.NuoDbMembershipProvider"/>
        </providers>
      </membership>
    </system.web>
  </configuration>

