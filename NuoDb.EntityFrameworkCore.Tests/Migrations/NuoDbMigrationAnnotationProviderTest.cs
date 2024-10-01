using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.TestUtilities;
using NuoDb.EntityFrameworkCore.NuoDb.Metadata.Internal;
using NuoDb.EntityFrameworkCore.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore;

namespace NuoDb.EntityFrameworkCore.Tests.Migrations
{
    public class NuoDbMigrationAnnotationProviderTest
    {
        private readonly TestHelpers.TestModelBuilder _modelBuilder = NuoDbTestHelpers.Instance.CreateConventionBuilder();
        private readonly NuoDbAnnotationProvider _provider = new NuoDbAnnotationProvider(new RelationalAnnotationProviderDependencies());
        private readonly Annotation _autoincrement = new(NuoDbAnnotationNames.Autoincrement, true);

        [ConditionalFact]
        public void Does_not_add_Autoincrement_for_OnAdd_integer_property_non_key()
        {
            var property = (IProperty)_modelBuilder.Entity<Entity>().Property(e => e.IntProp).ValueGeneratedOnAdd().Metadata;
            FinalizeModel();

            Assert.DoesNotContain(
                _provider.For(property.GetTableColumnMappings().Single().Column, true),
                a => a.Name == _autoincrement.Name && (bool)a.Value);
        }

        private IModel FinalizeModel()
            => _modelBuilder.FinalizeModel();
        private class Entity
        {
            public int Id { get; set; }
            public long IntProp { get; set; }
            public string StringProp { get; set; }
        }
    }
}
