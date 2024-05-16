using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuoDb.Data.Client;
using Xunit.Abstractions;

namespace NuoDb.EntityFrameworkCore.Tests.Query
{
    public class
        NorthwindJoinQueryNuoDbTest : NorthwindJoinQueryRelationalTestBase<
        NorthwindQueryNuoDbFixture<NoopModelCustomizer>>
    {
        public NorthwindJoinQueryNuoDbTest(NorthwindQueryNuoDbFixture<NoopModelCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task GroupJoin_subquery_projection_outer_mixed(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.GroupJoin_subquery_projection_outer_mixed(async));
        }

        public override async Task Inner_join_with_tautology_predicate_converts_to_cross_join(bool async)
        {
           // no cross join support
        } 

        public override async Task Join_complex_condition(bool async)
        {
            // no cross join support
        }

        public override async Task Join_select_many(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Join_select_many(async));
        }

        public override async Task SelectMany_with_client_eval_with_collection_shaper(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.SelectMany_with_client_eval_with_collection_shaper(async));
        }

        public override async Task SelectMany_with_client_eval(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.SelectMany_with_client_eval(async));
        }

        public override async Task SelectMany_with_client_eval_with_collection_shaper_ignored(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.SelectMany_with_client_eval_with_collection_shaper_ignored(async));
        }

        public override async Task SelectMany_with_selecting_outer_element(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.SelectMany_with_selecting_outer_element(async));
        }

        public override async Task SelectMany_with_selecting_outer_entity(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.SelectMany_with_selecting_outer_entity(async));
        }

        public override async Task SelectMany_with_selecting_outer_entity_column_and_inner_column(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.SelectMany_with_selecting_outer_entity_column_and_inner_column(async));
        }

        public override async Task Take_in_collection_projection_with_FirstOrDefault_on_top_level(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Take_in_collection_projection_with_FirstOrDefault_on_top_level(async));
        }
    }
}
