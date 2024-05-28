using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using NuoDb.Data.Client;
using Xunit.Abstractions;

namespace NuoDb.EntityFrameworkCore.Tests.Query
{
    public class NorthwindWhereQueryNuoDbTest : NorthwindWhereQueryRelationalTestBase<
        NorthwindQueryNuoDbFixture<NoopModelCustomizer>>
    {
        public NorthwindWhereQueryNuoDbTest(
            NorthwindQueryNuoDbFixture<NoopModelCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            ClearLog();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        protected override bool CanExecuteQueryString
            => true;

        public override async Task Where_simple(bool async)
        {
            await base.Where_simple(async);
        }

        public override async Task Where_as_queryable_expression(bool async)
        {
            await base.Where_as_queryable_expression(async);
        }

        public override async Task<string> Where_simple_closure(bool async)
        {
            //Expression<Func<Order, bool>> _filter = o => o.CustomerID == "ALFKI";
            //var customers= this.CreateContext().Set<Customer>().Where(c => c.Orders.AsQueryable().Any(_filter)).ToList();
            // var city = "London";
            // var customers = this.CreateContext().Set<Customer>().Where(c => c.City == city).ToQueryString();
            var queryString = await base.Where_simple_closure(async);

            return null;
        }

        public override async Task Where_indexer_closure(bool async)
        {
            await base.Where_indexer_closure(async);
        }

        public override async Task Where_dictionary_key_access_closure(bool async)
        {
            await base.Where_dictionary_key_access_closure(async);
        }

        public override async Task Where_tuple_item_closure(bool async)
        {
            await base.Where_tuple_item_closure(async);
        }

        public override async Task Where_named_tuple_item_closure(bool async)
        {
            await base.Where_named_tuple_item_closure(async);
        }

        public override async Task Where_simple_closure_constant(bool async)
        {
            await base.Where_simple_closure_constant(async);
        }

        public override async Task Where_simple_closure_via_query_cache(bool async)
        {
            await base.Where_simple_closure_via_query_cache(async);
        }

        public override async Task Where_method_call_nullable_type_closure_via_query_cache(bool async)
        {
            await base.Where_method_call_nullable_type_closure_via_query_cache(async);
        }

        public override async Task Where_method_call_nullable_type_reverse_closure_via_query_cache(bool async)
        {
            await base.Where_method_call_nullable_type_reverse_closure_via_query_cache(async);
        }

        public override async Task Where_method_call_closure_via_query_cache(bool async)
        {
            await base.Where_method_call_closure_via_query_cache(async);
        }

        public override async Task Where_field_access_closure_via_query_cache(bool async)
        {
            await base.Where_field_access_closure_via_query_cache(async);
        }

        public override async Task Where_property_access_closure_via_query_cache(bool async)
        {
            await base.Where_property_access_closure_via_query_cache(async);
        }

        public override async Task Where_static_field_access_closure_via_query_cache(bool async)
        {
            await base.Where_static_field_access_closure_via_query_cache(async);
        }

        public override async Task Where_static_property_access_closure_via_query_cache(bool async)
        {
            await base.Where_static_property_access_closure_via_query_cache(async);
        }

        public override async Task Where_nested_field_access_closure_via_query_cache(bool async)
        {
            await base.Where_nested_field_access_closure_via_query_cache(async);
        }

        public override async Task Where_nested_property_access_closure_via_query_cache(bool async)
        {
            await base.Where_nested_property_access_closure_via_query_cache(async);
        }

        public override async Task Where_new_instance_field_access_query_cache(bool async)
        {
            await base.Where_new_instance_field_access_query_cache(async);
        }

        public override async Task Where_new_instance_field_access_closure_via_query_cache(bool async)
        {
            await base.Where_new_instance_field_access_closure_via_query_cache(async);
        }

        public override async Task Where_simple_closure_via_query_cache_nullable_type(bool async)
        {
            await base.Where_simple_closure_via_query_cache_nullable_type(async);
        }

        public override async Task Where_simple_closure_via_query_cache_nullable_type_reverse(bool async)
        {
            await base.Where_simple_closure_via_query_cache_nullable_type_reverse(async);
        }

        public override void Where_subquery_closure_via_query_cache()
        {
            base.Where_subquery_closure_via_query_cache();
        }

        public override async Task Where_bitwise_or(bool async)
        {
            await base.Where_bitwise_or(async);
        }

        public override async Task Where_bitwise_and(bool async)
        {
            await base.Where_bitwise_and(async);
        }

        public override async Task Where_bitwise_xor(bool async)
        {
            await base.Where_bitwise_xor(async);
        }

        public override async Task Where_simple_shadow(bool async)
        {
            await base.Where_simple_shadow(async);
        }

        public override async Task Where_simple_shadow_projection(bool async)
        {
            await base.Where_simple_shadow_projection(async);
        }

        public override async Task Where_shadow_subquery_FirstOrDefault(bool async)
        {
            await base.Where_shadow_subquery_FirstOrDefault(async);
        }

        public override async Task Where_subquery_correlated(bool async)
        {
            await base.Where_subquery_correlated(async);
        }

        public override async Task Where_equals_method_string(bool async)
        {
            await base.Where_equals_method_string(async);
        }

        public override async Task Where_equals_method_int(bool async)
        {
            await base.Where_equals_method_int(async);
        }

        public override async Task Where_equals_using_object_overload_on_mismatched_types(bool async)
        {
            await base.Where_equals_using_object_overload_on_mismatched_types(async);

            Assert.Contains(
                "Possible unintended use of method 'Equals' for arguments 'e.EmployeeID' and '@__longPrm_0' of different types in a query. This comparison will always return false.",
                Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));
        }

        public override async Task Where_equals_using_int_overload_on_mismatched_types(bool async)
        {
            await base.Where_equals_using_int_overload_on_mismatched_types(async);
        }

        public override async Task Where_equals_on_mismatched_types_nullable_int_long(bool async)
        {
            await base.Where_equals_on_mismatched_types_nullable_int_long(async);

            Assert.Contains(
                "Possible unintended use of method 'Equals' for arguments 'e.ReportsTo' and '@__longPrm_0' of different types in a query. This comparison will always return false.",
                Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));
            Assert.Contains(
                "Possible unintended use of method 'Equals' for arguments '@__longPrm_0' and 'e.ReportsTo' of different types in a query. This comparison will always return false.",
                Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));
        }

        public override async Task Where_equals_on_mismatched_types_nullable_long_nullable_int(bool async)
        {
            await base.Where_equals_on_mismatched_types_nullable_long_nullable_int(async);

            Assert.Contains(
                "Possible unintended use of method 'Equals' for arguments 'e.ReportsTo' and '@__nullableLongPrm_0' of different types in a query. This comparison will always return false.",
                Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));
            Assert.Contains(
                "Possible unintended use of method 'Equals' for arguments '@__nullableLongPrm_0' and 'e.ReportsTo' of different types in a query. This comparison will always return false.",
                Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));
        }

        public override async Task Where_equals_on_mismatched_types_int_nullable_int(bool async)
        {
            await base.Where_equals_on_mismatched_types_int_nullable_int(async);
        }

        public override async Task Where_equals_on_matched_nullable_int_types(bool async)
        {
            await base.Where_equals_on_matched_nullable_int_types(async);
        }

        public override async Task Where_equals_on_null_nullable_int_types(bool async)
        {
            await base.Where_equals_on_null_nullable_int_types(async);
        }

        public override async Task Where_comparison_nullable_type_not_null(bool async)
        {
            await base.Where_comparison_nullable_type_not_null(async);
        }

        public override async Task Where_comparison_nullable_type_null(bool async)
        {
            await base.Where_comparison_nullable_type_null(async);
        }

        public override async Task Where_string_length(bool async)
        {
            await base.Where_string_length(async);
        }

        public override async Task Where_string_indexof(bool async)
        {
            await base.Where_string_indexof(async);
        }

        public override async Task Where_string_replace(bool async)
        {
            await base.Where_string_replace(async);
        }

        public override async Task Where_string_substring(bool async)
        {
            await base.Where_string_substring(async);
        }

        public override async Task Where_datetime_now(bool async)
        {
            await base.Where_datetime_now(async);
        }

        public override async Task Where_datetime_utcnow(bool async)
        {
            await base.Where_datetime_utcnow(async);
        }

        public override async Task Where_datetimeoffset_utcnow(bool async)
        {
            await base.Where_datetimeoffset_utcnow(async);
        }

        public override async Task Where_datetime_today(bool async)
        {
            await base.Where_datetime_today(async);
        }

        public override async Task Where_datetime_date_component(bool async)
        {
            var myDatetime = new DateTime(1998, 5, 4);
            var ctx = this.CreateContext();
            var results = ctx.Set<Order>().Where(o => o.OrderDate.Value.Date == myDatetime).ToList();
            await base.Where_datetime_date_component(async);
        }

        public override async Task Where_date_add_year_constant_component(bool async)
        {
            await base.Where_date_add_year_constant_component(async);
        }

        public override async Task Where_datetime_year_component(bool async)
        {
            await base.Where_datetime_year_component(async);
        }

        public override async Task Where_datetime_month_component(bool async)
        {
            await base.Where_datetime_month_component(async);
        }

        public override async Task Where_datetime_dayOfYear_component(bool async)
        {
            await base.Where_datetime_dayOfYear_component(async);
        }

        public override async Task Where_datetime_day_component(bool async)
        {
            await base.Where_datetime_day_component(async);
        }

        public override async Task Where_datetime_hour_component(bool async)
        {
            await base.Where_datetime_hour_component(async);
        }

        public override async Task Where_datetime_minute_component(bool async)
        {
            await base.Where_datetime_minute_component(async);
        }

        public override async Task Where_datetime_second_component(bool async)
        {
            await base.Where_datetime_second_component(async);
        }

        public override async Task Where_datetime_millisecond_component(bool async)
        {
            await base.Where_datetime_millisecond_component(async);
        }

        public override async Task Where_datetimeoffset_now_component(bool async)
        {
            await base.Where_datetimeoffset_now_component(async);
        }

        public override async Task Where_datetimeoffset_utcnow_component(bool async)
        {
            await base.Where_datetimeoffset_utcnow_component(async);
        }

        public override async Task Where_simple_reversed(bool async)
        {
            await base.Where_simple_reversed(async);
        }

        public override async Task Where_is_null(bool async)
        {
            await base.Where_is_null(async);
        }

        public override async Task Where_null_is_null(bool async)
        {
            await base.Where_null_is_null(async);
        }

        public override async Task Where_constant_is_null(bool async)
        {
            await base.Where_constant_is_null(async);
        }

        public override async Task Where_is_not_null(bool async)
        {
            await base.Where_is_not_null(async);
        }

        public override async Task Where_null_is_not_null(bool async)
        {
            await base.Where_null_is_not_null(async);
        }

        public override async Task Where_constant_is_not_null(bool async)
        {
            await base.Where_constant_is_not_null(async);
        }

        public override async Task Where_identity_comparison(bool async)
        {
            await base.Where_identity_comparison(async);
        }

        public override async Task Where_in_optimization_multiple(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Where_in_optimization_multiple(async));
        }

        public override async Task Where_not_in_optimization1(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Where_not_in_optimization1(async));
        }

        public override async Task Where_not_in_optimization2(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Where_not_in_optimization2(async));
        }

        public override async Task Where_not_in_optimization3(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Where_not_in_optimization3(async));
        }

        public override async Task Where_not_in_optimization4(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Where_not_in_optimization4(async));
        }

        public override async Task Where_select_many_and(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Where_select_many_and(async));
        }

        public override async Task Where_primitive(bool async)
        {
            await base.Where_primitive(async);
        }

        public override async Task Where_bool_member(bool async)
        {
            await base.Where_bool_member(async);
        }

        public override async Task Where_bool_member_false(bool async)
        {
            await base.Where_bool_member_false(async);
        }

        public override async Task Where_bool_member_negated_twice(bool async)
        {
            await base.Where_bool_member_negated_twice(async);
        }

        public override async Task Where_bool_member_shadow(bool async)
        {
            await base.Where_bool_member_shadow(async);
        }

        public override async Task Where_bool_member_false_shadow(bool async)
        {
            await base.Where_bool_member_false_shadow(async);
        }

        public override async Task Where_bool_member_equals_constant(bool async)
        {
            await base.Where_bool_member_equals_constant(async);
        }

        public override async Task Where_bool_member_in_complex_predicate(bool async)
        {
            await base.Where_bool_member_in_complex_predicate(async);
        }

        public override async Task Where_bool_member_compared_to_binary_expression(bool async)
        {
            await base.Where_bool_member_compared_to_binary_expression(async);
        }

        public override async Task Where_not_bool_member_compared_to_not_bool_member(bool async)
        {
            await base.Where_not_bool_member_compared_to_not_bool_member(async);
        }

        public override async Task Where_negated_boolean_expression_compared_to_another_negated_boolean_expression(
            bool async)
        {
            await base.Where_negated_boolean_expression_compared_to_another_negated_boolean_expression(async);
        }

        public override async Task Where_not_bool_member_compared_to_binary_expression(bool async)
        {
            await base.Where_not_bool_member_compared_to_binary_expression(async);
        }

        public override async Task Where_bool_parameter(bool async)
        {
            await base.Where_bool_parameter(async);
        }

        public override async Task Where_bool_parameter_compared_to_binary_expression(bool async)
        {
            await base.Where_bool_parameter_compared_to_binary_expression(async);
        }

        public override async Task Where_bool_member_and_parameter_compared_to_binary_expression_nested(bool async)
        {
            await base.Where_bool_member_and_parameter_compared_to_binary_expression_nested(async);
        }

        public override async Task Where_de_morgan_or_optimized(bool async)
        {
            await base.Where_de_morgan_or_optimized(async);
        }

        public override async Task Where_de_morgan_and_optimized(bool async)
        {
            await base.Where_de_morgan_and_optimized(async);
        }

        public override async Task Where_complex_negated_expression_optimized(bool async)
        {
            await base.Where_complex_negated_expression_optimized(async);
        }

        public override async Task Where_short_member_comparison(bool async)
        {
            await base.Where_short_member_comparison(async);
        }

        public override async Task Where_comparison_to_nullable_bool(bool async)
        {
            await base.Where_comparison_to_nullable_bool(async);
        }

        public override async Task Where_true(bool async)
        {
            await base.Where_true(async);
        }

        public override async Task Where_false(bool async)
        {
            await base.Where_false(async);
        }

        public override async Task Where_default(bool async)
        {
            await base.Where_default(async);
        }

        public override async Task Where_expression_invoke_1(bool async)
        {
            await base.Where_expression_invoke_1(async);
        }

        public override async Task Where_expression_invoke_2(bool async)
        {
            await base.Where_expression_invoke_2(async);
        }

        public override async Task Where_expression_invoke_3(bool async)
        {
            await base.Where_expression_invoke_3(async);
        }

        public override async Task Where_concat_string_int_comparison1(bool async)
        {
            await base.Where_concat_string_int_comparison1(async);
        }

        public override async Task Where_concat_string_int_comparison2(bool async)
        {
            await base.Where_concat_string_int_comparison2(async);
        }

        public override async Task Where_concat_string_int_comparison3(bool async)
        {
            await base.Where_concat_string_int_comparison3(async);
        }

        public override async Task Where_concat_string_int_comparison4(bool async)
        {
            await base.Where_concat_string_int_comparison4(async);
        }

        public override async Task Where_concat_string_string_comparison(bool async)
        {
            await base.Where_concat_string_string_comparison(async);
        }

        public override async Task Where_string_concat_method_comparison(bool async)
        {
            await base.Where_string_concat_method_comparison(async);
        }

        public override async Task Where_string_concat_method_comparison_2(bool async)
        {
            await base.Where_string_concat_method_comparison_2(async);
        }

        public override async Task Where_string_concat_method_comparison_3(bool async)
        {
            await base.Where_string_concat_method_comparison_3(async);
        }

        public override async Task Where_ternary_boolean_condition_true(bool async)
        {
            await base.Where_ternary_boolean_condition_true(async);
        }

        public override async Task Where_ternary_boolean_condition_false(bool async)
        {
            await base.Where_ternary_boolean_condition_false(async);
        }

        public override async Task Where_ternary_boolean_condition_with_another_condition(bool async)
        {
            await base.Where_ternary_boolean_condition_with_another_condition(async);
        }

        public override async Task Where_ternary_boolean_condition_with_false_as_result_true(bool async)
        {
            await base.Where_ternary_boolean_condition_with_false_as_result_true(async);
        }

        public override async Task Where_ternary_boolean_condition_with_false_as_result_false(bool async)
        {
            await base.Where_ternary_boolean_condition_with_false_as_result_false(async);
        }

        public override async Task Where_compare_constructed_equal(bool async)
        {
            await base.Where_compare_constructed_equal(async);
        }

        public override async Task Where_compare_constructed_multi_value_equal(bool async)
        {
            await base.Where_compare_constructed_multi_value_equal(async);
        }

        public override async Task Where_compare_constructed_multi_value_not_equal(bool async)
        {
            await base.Where_compare_constructed_multi_value_not_equal(async);
        }

        public override async Task Where_compare_tuple_constructed_equal(bool async)
        {
            await base.Where_compare_tuple_constructed_equal(async);
        }

        public override async Task Where_compare_tuple_constructed_multi_value_equal(bool async)
        {
            await base.Where_compare_tuple_constructed_multi_value_equal(async);
        }

        public override async Task Where_compare_tuple_constructed_multi_value_not_equal(bool async)
        {
            await base.Where_compare_tuple_constructed_multi_value_not_equal(async);
        }

        public override async Task Where_compare_tuple_create_constructed_equal(bool async)
        {
            await base.Where_compare_tuple_create_constructed_equal(async);
        }

        public override async Task Where_compare_tuple_create_constructed_multi_value_equal(bool async)
        {
            await base.Where_compare_tuple_create_constructed_multi_value_equal(async);
        }

        public override async Task Where_compare_tuple_create_constructed_multi_value_not_equal(bool async)
        {
            await base.Where_compare_tuple_create_constructed_multi_value_not_equal(async);
        }

        public override async Task Where_compare_null(bool async)
        {
            await base.Where_compare_null(async);
        }

        public override async Task Where_compare_null_with_cast_to_object(bool async)
        {
            await base.Where_compare_null_with_cast_to_object(async);
        }

        public override async Task Where_compare_with_both_cast_to_object(bool async)
        {
            await base.Where_compare_with_both_cast_to_object(async);
        }

        public override async Task Where_Is_on_same_type(bool async)
        {
            await base.Where_Is_on_same_type(async);
        }

        public override async Task Where_chain(bool async)
        {
            await base.Where_chain(async);
        }

        public override void Where_navigation_contains()
        {
            base.Where_navigation_contains();
        }

        public override async Task Where_array_index(bool async)
        {
            await base.Where_array_index(async);
        }

        public override async Task Where_multiple_contains_in_subquery_with_or(bool async)
        {
            await base.Where_multiple_contains_in_subquery_with_or(async);
        }

        public override async Task Where_multiple_contains_in_subquery_with_and(bool async)
        {
            await base.Where_multiple_contains_in_subquery_with_and(async);
        }

        public override async Task Where_contains_on_navigation(bool async)
        {
            await base.Where_contains_on_navigation(async);
        }

        public override async Task Where_subquery_FirstOrDefault_is_null(bool async)
        {
            await base.Where_subquery_FirstOrDefault_is_null(async);
        }

        public override async Task Where_subquery_FirstOrDefault_compared_to_entity(bool async)
        {
            await base.Where_subquery_FirstOrDefault_compared_to_entity(async);
        }

        public override async Task Time_of_day_datetime(bool async)
        {
             await base.Time_of_day_datetime(async);
        }

        public override async Task TypeBinary_short_circuit(bool async)
        {
            await base.TypeBinary_short_circuit(async);
        }

        public override async Task Where_is_conditional(bool async)
        {
            await base.Where_is_conditional(async);
        }

        public override async Task Enclosing_class_settable_member_generates_parameter(bool async)
        {
            await base.Enclosing_class_settable_member_generates_parameter(async);
        }

        public override async Task Enclosing_class_readonly_member_generates_parameter(bool async)
        {
            await base.Enclosing_class_readonly_member_generates_parameter(async);
        }

        public override async Task Enclosing_class_const_member_does_not_generate_parameter(bool async)
        {
            await base.Enclosing_class_const_member_does_not_generate_parameter(async);
        }

        public override async Task Generic_Ilist_contains_translates_to_server(bool async)
        {
            await base.Generic_Ilist_contains_translates_to_server(async);
        }

        public override async Task Filter_non_nullable_value_after_FirstOrDefault_on_empty_collection(bool async)
        {
            await base.Filter_non_nullable_value_after_FirstOrDefault_on_empty_collection(async);
        }

        public override async Task Like_with_non_string_column_using_ToString(bool async)
        {
            await base.Like_with_non_string_column_using_ToString(async);
        }

        public override async Task Like_with_non_string_column_using_double_cast(bool async)
        {
            await base.Like_with_non_string_column_using_double_cast(async);
        }

        public override async Task Using_same_parameter_twice_in_query_generates_one_sql_parameter(bool async)
        {
            await base.Using_same_parameter_twice_in_query_generates_one_sql_parameter(async);
        }

        public override async Task Where_Queryable_ToList_Count(bool async)
        {
            await base.Where_Queryable_ToList_Count(async);
        }

        public override async Task Where_Queryable_ToList_Contains(bool async)
        {
            await base.Where_Queryable_ToList_Contains(async);
        }

        public override async Task Where_Queryable_ToArray_Count(bool async)
        {
            await base.Where_Queryable_ToArray_Count(async);
        }

        public override async Task Where_Queryable_ToArray_Contains(bool async)
        {
            await base.Where_Queryable_ToArray_Contains(async);
        }

        public override async Task Where_Queryable_AsEnumerable_Count(bool async)
        {
            await base.Where_Queryable_AsEnumerable_Count(async);
        }

        public override async Task Where_Queryable_AsEnumerable_Contains(bool async)
        {
            await base.Where_Queryable_AsEnumerable_Contains(async);
        }

        public override async Task Where_Queryable_AsEnumerable_Contains_negated(bool async)
        {
            await base.Where_Queryable_AsEnumerable_Contains_negated(async);
        }

        public override async Task Where_Queryable_ToList_Count_member(bool async)
        {
            await base.Where_Queryable_ToList_Count_member(async);
        }

        public override async Task Where_Queryable_ToArray_Length_member(bool async)
        {
            await base.Where_Queryable_ToArray_Length_member(async);
        }

        public override async Task Where_collection_navigation_ToList_Count(bool async)
        {
            await base.Where_collection_navigation_ToList_Count(async);
        }

        public override async Task Where_collection_navigation_ToList_Contains(bool async)
        {
            await base.Where_collection_navigation_ToList_Contains(async);
        }

        public override async Task Where_collection_navigation_ToArray_Count(bool async)
        {
            await base.Where_collection_navigation_ToArray_Count(async);
        }

        public override async Task Where_collection_navigation_ToArray_Contains(bool async)
        {
            await base.Where_collection_navigation_ToArray_Contains(async);
        }

        public override async Task Where_collection_navigation_AsEnumerable_Count(bool async)
        {
            await base.Where_collection_navigation_AsEnumerable_Count(async);
        }

        public override async Task Where_collection_navigation_AsEnumerable_Contains(bool async)
        {
            await base.Where_collection_navigation_AsEnumerable_Contains(async);
        }

        public override async Task Where_collection_navigation_ToList_Count_member(bool async)
        {
            await base.Where_collection_navigation_ToList_Count_member(async);
        }

        public override async Task Where_collection_navigation_ToArray_Length_member(bool async)
        {
            await base.Where_collection_navigation_ToArray_Length_member(async);
        }

        public override async Task Where_list_object_contains_over_value_type(bool async)
        {
            await base.Where_list_object_contains_over_value_type(async);
        }

        public override async Task Where_array_of_object_contains_over_value_type(bool async)
        {
            await base.Where_array_of_object_contains_over_value_type(async);
        }

        public override async Task Multiple_OrElse_on_same_column_converted_to_in_with_overlap(bool async)
        {
            await base.Multiple_OrElse_on_same_column_converted_to_in_with_overlap(async);
        }

        public override async Task Multiple_OrElse_on_same_column_with_null_constant_comparison_converted_to_in(
            bool async)
        {
            await base.Multiple_OrElse_on_same_column_with_null_constant_comparison_converted_to_in(async);
        }

        public override async Task Constant_array_Contains_OrElse_comparison_with_constant_gets_combined_to_one_in(
            bool async)
        {
            await base.Constant_array_Contains_OrElse_comparison_with_constant_gets_combined_to_one_in(async);
        }

        public override async Task
            Constant_array_Contains_OrElse_comparison_with_constant_gets_combined_to_one_in_with_overlap(bool async)
        {
            await base.Constant_array_Contains_OrElse_comparison_with_constant_gets_combined_to_one_in_with_overlap(
                async);
        }

        public override async Task Constant_array_Contains_OrElse_another_Contains_gets_combined_to_one_in_with_overlap(
            bool async)
        {
            await base.Constant_array_Contains_OrElse_another_Contains_gets_combined_to_one_in_with_overlap(async);
        }

        public override async Task
            Constant_array_Contains_AndAlso_another_Contains_gets_combined_to_one_in_with_overlap(bool async)
        {
            await base.Constant_array_Contains_AndAlso_another_Contains_gets_combined_to_one_in_with_overlap(async);
        }

        public override async Task Multiple_AndAlso_on_same_column_converted_to_in_using_parameters(bool async)
        {
            await base.Multiple_AndAlso_on_same_column_converted_to_in_using_parameters(async);

            // issue #21462
        }

        public override async Task Array_of_parameters_Contains_OrElse_comparison_with_constant_gets_combined_to_one_in(
            bool async)
        {
            await base.Array_of_parameters_Contains_OrElse_comparison_with_constant_gets_combined_to_one_in(async);

            // issue #21462
        }

        public override async Task Multiple_OrElse_on_same_column_with_null_parameter_comparison_converted_to_in(
            bool async)
        {
            await base.Multiple_OrElse_on_same_column_with_null_parameter_comparison_converted_to_in(async);

            // issue #21462
        }

        public override async Task Parameter_array_Contains_OrElse_comparison_with_constant(bool async)
        {
            await base.Parameter_array_Contains_OrElse_comparison_with_constant(async);
        }

        public override async Task Parameter_array_Contains_OrElse_comparison_with_parameter_with_overlap(bool async)
        {
            await base.Parameter_array_Contains_OrElse_comparison_with_parameter_with_overlap(async);
        }

        public override async Task Two_sets_of_comparison_combine_correctly(bool async)
        {
            await base.Two_sets_of_comparison_combine_correctly(async);
        }

        public override async Task Two_sets_of_comparison_combine_correctly2(bool async)
        {
            await base.Two_sets_of_comparison_combine_correctly2(async);
        }

        public override async Task Filter_with_EF_Property_using_closure_for_property_name(bool async)
        {
            await base.Filter_with_EF_Property_using_closure_for_property_name(async);
        }

        public override async Task Filter_with_EF_Property_using_function_for_property_name(bool async)
        {
            await base.Filter_with_EF_Property_using_function_for_property_name(async);
        }

        public override async Task FirstOrDefault_over_scalar_projection_compared_to_null(bool async)
        {
            await base.FirstOrDefault_over_scalar_projection_compared_to_null(async);
        }

        public override async Task FirstOrDefault_over_scalar_projection_compared_to_not_null(bool async)
        {
            await base.FirstOrDefault_over_scalar_projection_compared_to_not_null(async);
        }

        public override async Task FirstOrDefault_over_custom_projection_compared_to_null(bool async)
        {
            await base.FirstOrDefault_over_custom_projection_compared_to_null(async);
        }

        public override async Task FirstOrDefault_over_custom_projection_compared_to_not_null(bool async)
        {
            await base.FirstOrDefault_over_custom_projection_compared_to_not_null(async);
        }

        public override async Task SingleOrDefault_over_custom_projection_compared_to_null(bool async)
        {
            await base.SingleOrDefault_over_custom_projection_compared_to_null(async);
        }

        public override async Task SingleOrDefault_over_custom_projection_compared_to_not_null(bool async)
        {
            await base.SingleOrDefault_over_custom_projection_compared_to_not_null(async);
        }

        public override async Task LastOrDefault_over_custom_projection_compared_to_null(bool async)
        {
            await base.LastOrDefault_over_custom_projection_compared_to_null(async);
        }

        public override async Task LastOrDefault_over_custom_projection_compared_to_not_null(bool async)
        {
            await base.LastOrDefault_over_custom_projection_compared_to_not_null(async);
        }

        public override async Task First_over_custom_projection_compared_to_null(bool async)
        {
            await base.First_over_custom_projection_compared_to_null(async);
        }

        public override async Task First_over_custom_projection_compared_to_not_null(bool async)
        {
            await base.First_over_custom_projection_compared_to_not_null(async);
        }

        public override async Task Single_over_custom_projection_compared_to_null(bool async)
        {
            await base.Single_over_custom_projection_compared_to_null(async);
        }

        public override async Task Single_over_custom_projection_compared_to_not_null(bool async)
        {
            await base.Single_over_custom_projection_compared_to_not_null(async);
        }

        public override async Task Last_over_custom_projection_compared_to_null(bool async)
        {
            await base.Last_over_custom_projection_compared_to_null(async);
        }

        public override async Task Last_over_custom_projection_compared_to_not_null(bool async)
        {
            await base.Last_over_custom_projection_compared_to_not_null(async);
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}