using System.Collections.Generic;
using System.Linq;
using DotLiquid;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Lava.Blocks;
using Rock.Model;
using Rock.Tests.Shared;

namespace Rock.Tests.Integration.Lava
{
    [TestClass]
    public class EventOccurrencesCommandTests
    {
        private static string StaffMeetingEventGuidString = "93104654-DAFA-489B-A175-5F2AB3A846F1";
        private static string PublicCalendarGuidString = "8A444668-19AF-4417-9C74-09F842572974";
        private static string StaffAudienceGuidString = "833EE2C7-F83A-4744-AD14-6907554DF8AE";
        private static string YouthAudienceGuidString = "59CD7FD8-6A62-4C3B-8966-1520E74EED58";
        
        private static string LavaTemplateEventOccurrences = @";
        

{% eventoccurrences {parameters} %}
  {% assign eventItemOccurrenceCount = EventItemOccurrences | Size %}
  <<EventCount = {{ EventItemOccurrences | Size }}>>
  {% for eventItemOccurrence in EventItemOccurrences %}
    <<{{ eventItemOccurrence.Name }}|{{ eventItemOccurrence.Date | Date: 'yyyy-MM-dd' }}|{{ eventItemOccurrence.Time }}|{{ eventItemOccurrence.Location }}>>
    <<Calendars: {{ eventItemOccurrence.CalendarNames | Join:', ' }}>>
    <<Audiences: {{ eventItemOccurrence.AudienceNames | Join:', ' }}>>
  {% endfor %}
{% endeventoccurrences %}
";


        [ClassInitialize]
        public static void ClassInitialize( TestContext testContext )
        {
            // Initialize the Lava Engine.
            Liquid.UseRubyDateFormat = false;
            Template.NamingConvention = new DotLiquid.NamingConventions.CSharpNamingConvention();

            Template.RegisterFilter( typeof( Rock.Lava.RockFilters ) );

            // Register the Lava commands required for testing.
            var lavaCommand = new EventOccurrences();

            lavaCommand.OnStartup();
        }

        private string GetTestTemplate( string parameters )
        {
            var template = LavaTemplateEventOccurrences;

            return template.Replace( "{parameters}", parameters );
        }

        [TestMethod]
        public void EventOccurrencesCommand_WithUnknownParameterName_RendersErrorMessage()
        {
            var template = GetTestTemplate( "eventid:'1' unknown_parameter:'any_value'" );

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "Event Occurrences not available. Invalid configuration setting \"unknown_parameter\"." );
        }

        [TestMethod]
        public void EventOccurrencesCommand_WithEventAsName_RetrievesOccurrencesInCorrectEvent()
        {
            var template = GetTestTemplate( "eventid:'Staff Meeting' startdate:'2020-1-1' daterange:'12m' maxoccurrences:2" );

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "<<Staff Meeting|2020-01-01|12:00 AM|All Campuses>>" );
            Assert.That.Contains( output, "<<Staff Meeting|2020-01-15|12:00 AM|All Campuses>>" );

        }

        [TestMethod]
        public void EventOccurrencesCommand_WithEventAsId_RetrievesOccurrencesInCorrectEvent()
        {
            // Get Event Item Id for "Warrior Youth Event".
            var rockContext = new RockContext();

            var eventItemService = new EventItemService( rockContext );
            var eventId = eventItemService.GetId( StaffMeetingEventGuidString.AsGuid() );

            Assert.That.IsNotNull( eventId, "Expected test data not found." );

            var template = GetTestTemplate( $"eventid:{eventId} startdate:'2020-1-1' daterange:'12m' maxoccurrences:2" );

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "<<Staff Meeting|2020-01-01|12:00 AM|All Campuses>>" );
            Assert.That.Contains( output, "<<Staff Meeting|2020-01-15|12:00 AM|All Campuses>>" );

        }

        [TestMethod]
        public void EventOccurrencesCommand_WithEventAsGuid_RetrievesOccurrencesInCorrectEvent()
        {
            var template = GetTestTemplate( $"eventid:'{StaffMeetingEventGuidString}' startdate:'2020-1-1' daterange:'12m' maxoccurrences:2" );

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "<<Staff Meeting|2020-01-01|12:00 AM|All Campuses>>" );
            Assert.That.Contains( output, "<<Staff Meeting|2020-01-15|12:00 AM|All Campuses>>" );
        }

        [TestMethod]
        public void EventOccurrencesCommand_WithEventNotSpecified_RendersErrorMessage()
        {
            var template = GetTestTemplate( "startdate:'2020-1-1' daterange:'12m' maxoccurrences:2" );

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "Event Occurrences not available. An Event reference must be specified." );
        }

        [TestMethod]
        public void EventOccurrencesCommand_WithEventInvalidValue_RendersErrorMessage()
        {
            var template = GetTestTemplate( "eventid:'no_event' startdate:'2020-1-1' daterange:'12m' maxoccurrences:2" );

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "Event Occurrences not available. Cannot find an Event matching the reference \"no_event\"." );
        }
        
        //[TestMethod]
        //public void EventOccurrencesCommand_WithAudienceAsName_RetrievesOccurrencesWithMatchingAudience()
        //{
        //    var template = GetTestTemplate( "calendarid:'Public' audienceids:'Youth' startdate:'2020-1-1' daterange:'12m' maxoccurrences:2" );

        //    var output = template.ResolveMergeFields( null );

        //    Assert.That.Contains( output, "<Audiences: All Church, Adults, Youth>" );
        //}
        //public void EventOccurrencesCommand_WithAudienceAsMultipleValues_RetrievesOccurrencesWithAnyMatchingAudience()
        //{
        //    var template = GetTestTemplate( "calendarid:'Public' audienceids:'Men,Women' startdate:'2020-1-1' daterange:'12m' maxoccurrences:2" );

        //    var output = template.ResolveMergeFields( null );

        //    Assert.That.Contains( output, "<Audiences: Internal>" );
        //}

        //[TestMethod]
        //public void EventOccurrencesCommand_WithAudienceAsId_RetrievesOccurrencesWithMatchingAudience()
        //{
        //    var rockContext = new RockContext();

        //    var audienceGuid = SystemGuid.DefinedType.CONTENT_CHANNEL_AUDIENCE_TYPE.AsGuid();

        //    var definedValueId = new DefinedTypeService( rockContext ).Queryable()
        //        .FirstOrDefault( x => x.Guid == audienceGuid )
        //        .DefinedValues.FirstOrDefault( x => x.Value == "All Church" ).Id;

        //    var template = GetTestTemplate( $"calendarid:'Public' audienceids:'{definedValueId}' startdate:'2018-1-1'" );

        //    var output = template.ResolveMergeFields( null );

        //    Assert.That.Contains( output, "<Audiences: All Church," );
        //}

        //[TestMethod]
        //public void EventOccurrencesCommand_WithAudienceAsGuid_RetrievesOccurrencesWithMatchingAudience()
        //{
        //    var template = GetTestTemplate( $"calendarid:'Public' audienceids:'{YouthAudienceGuidString}' startdate:'2018-1-1'" );

        //    var output = template.ResolveMergeFields( null );

        //    Assert.That.Contains( output, "<Audiences: All Church, Adults, Youth" );
        //}

        //[TestMethod]
        //public void EventOccurrencesCommand_WithAudienceInvalidValue_RendersErrorMessage()
        //{
        //    var template = GetTestTemplate( "eventid:'Staff Meeting' audienceids:'no_audience'" );

        //    var output = template.ResolveMergeFields( null );

        //    Assert.That.Contains( output, "Calendar Events not available. Cannot apply an audience filter for the reference \"no_audience\"." );
        //}

        [TestMethod]
        public void EventOccurrencesCommand_WithDateRangeInMonths_ReturnsExpectedEvents()
        {
            var template = GetTestTemplate( "eventid:'Staff Meeting' startdate:'2020-1-1' daterange:'3m'" );

            var output = template.ResolveMergeFields( null );

            //Assert.That.Contains( output, "<EventCount = 2>" );
            Assert.That.Contains( output, "<<Staff Meeting|2020-01-01|12:00 AM|All Campuses>>" );
            Assert.That.Contains( output, "<<Staff Meeting|2020-02-26|12:00 AM|All Campuses>>" );
            Assert.That.Contains( output, "<<Staff Meeting|2020-03-25|12:00 AM|All Campuses>>" );

            // Staff Meeting recurs every 2 weeks, so our date range of 3 months weeks should not include the meeting in month 4.
            Assert.That.DoesNotContain( output, "<<Staff Meeting|2020-04-08|12:00 AM|All Campuses>>" );
        }

        [TestMethod]
        public void EventOccurrencesCommand_WithDateRangeInWeeks_ReturnsExpectedEvents()
        {
            var template = GetTestTemplate( "eventid:'Staff Meeting' startdate:'2020-1-1' daterange:'5w'" );

            var output = template.ResolveMergeFields( null );

            // Staff Meeting recurs every 2 weeks, so our date range of 5 weeks should only include 2 occurrences.
            //Assert.That.Contains( output, "<EventCount = 2>" );
            Assert.That.Contains( output, "<<Staff Meeting|2020-01-01|12:00 AM|All Campuses>>" );
            Assert.That.Contains( output, "<<Staff Meeting|2020-01-15|12:00 AM|All Campuses>>" );

            Assert.That.DoesNotContain( output, "<<Staff Meeting|2020-01-29|12:00 AM|All Campuses>>" );
        }

        [TestMethod]
        public void EventOccurrencesCommand_WithDateRangeInDays_ReturnsExpectedEvents()
        {
            var template = GetTestTemplate( "eventid:'Staff Meeting' startdate:'2020-1-1' daterange:'27d'" );

            var output = template.ResolveMergeFields( null );

            // Staff Meeting recurs every 2 weeks, so our date range of 27d should only include 2 occurrences.
            //Assert.That.Contains( output, "<EventCount = 2>" );
            Assert.That.Contains( output, "<<Staff Meeting|2020-01-01|12:00 AM|All Campuses>>" );
            Assert.That.Contains( output, "<<Staff Meeting|2020-01-15|12:00 AM|All Campuses>>" );

            Assert.That.DoesNotContain( output, "<<Staff Meeting|2020-01-29|12:00 AM|All Campuses>>" );
        }

        [TestMethod]
        public void EventOccurrencesCommand_WithDateRangeContainingNoEvents_ReturnsNoEvents()
        {
            var template = GetTestTemplate( "eventid:'Staff Meeting' startdate:'1020-1-1' daterange:'12m'" );

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "<EventCount = 0>" );
            //Assert.That.Contains( output, "<<Staff Meeting|2020-01-01|12:00 AM|All Campuses>>" );
            //Assert.That.Contains( output, "<<Staff Meeting|2020-12-30|12:00 AM|All Campuses>>" );
        }

        [TestMethod]
        public void EventOccurrencesCommand_WithDateRangeUnspecified_ReturnsAllEvents()
        {
            var template = GetTestTemplate( "eventid:'Staff Meeting' startdate:'2020-1-1' maxoccurrences:200" );

            var output = template.ResolveMergeFields( null );

            // Ensure that the maximum number of occurrences has been retrieved.
            Assert.That.Contains( output, "<EventCount = 200>" );
        }

        [TestMethod]
        public void EventOccurrencesCommand_WithDateRangeInvalidValue_RendersErrorMessage()
        {
            var template = GetTestTemplate( "eventid:'Staff Meeting' daterange:'invalid'" );

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "Event Occurrences not available. The specified Date Range is invalid." );
        }

        [TestMethod]
        public void EventOccurrencesCommand_WithMaxOccurrencesUnspecified_ReturnsDefaultNumberOfOccurrences()
        {
            // First, ensure that there are more than the default maximum number of events to return.
            // The default maximum is 100 events.
            var template1 = GetTestTemplate( "eventid:'Staff Meeting' startdate:'2020-1-1' maxoccurrences:101" );

            var output1 = template1.ResolveMergeFields( null );

            Assert.That.Contains( output1, "<EventCount = 101>" );

            // Now ensure that the default limit is applied.
            var template2 = GetTestTemplate( "eventid:'Staff Meeting' startdate:'2020-1-1'" );

            var output2 = template2.ResolveMergeFields( null );

            Assert.That.Contains( output2, "<EventCount = 100>" );
        }

        [TestMethod]
        public void EventOccurrencesCommand_WithMaxOccurrencesLessThanAvailableEvents_ReturnsMaxOccurrences()
        {
            // First, ensure that there are more than the test maximum number of events to return.
            var template1 = GetTestTemplate( "eventid:'Staff Meeting' startdate:'2020-1-1' maxoccurrences:11" );

            var output1 = template1.ResolveMergeFields( null );

            Assert.That.Contains( output1, "<EventCount = 11>" );

            // Now ensure that the maxoccurences limit is applied.
            var template = GetTestTemplate( "eventid:'Staff Meeting' startdate:'2020-1-1' maxoccurrences:10" );

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "<EventCount = 10>" );
        }

        [TestMethod]
        public void EventOccurrencesCommand_WithMaxOccurrencesInvalidValue_RendersErrorMessage()
        {
            var template = GetTestTemplate( "eventid:'Staff Meeting' startdate:'2020-1-1' maxoccurrences:'invalid_value'" );

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "Event Occurrences not available. Invalid configuration setting \"maxoccurrences\"." );
        }

        /*
        var expectedOutput = @"
    Staff Meeting|1/01/2020|12:00 AM|All Campuses|
  
    Warrior Youth Event|5/01/2020|3:00 PM|All Campuses|
  
    Staff Meeting|15/01/2020|12:00 AM|All Campuses|
  
    Staff Meeting|29/01/2020|12:00 AM|All Campuses|
  
    Staff Meeting|12/02/2020|12:00 AM|All Campuses|
  
    Staff Meeting|26/02/2020|12:00 AM|All Campuses|
  
    Staff Meeting|11/03/2020|12:00 AM|All Campuses|
  
    Staff Meeting|25/03/2020|12:00 AM|All Campuses|
  
    Staff Meeting|8/04/2020|12:00 AM|All Campuses|
  
    Staff Meeting|22/04/2020|12:00 AM|All Campuses|
  
    Staff Meeting|6/05/2020|12:00 AM|All Campuses|
  
    Staff Meeting|20/05/2020|12:00 AM|All Campuses|
  
    Staff Meeting|3/06/2020|12:00 AM|All Campuses|
  
    Staff Meeting|17/06/2020|12:00 AM|All Campuses|
  
    Staff Meeting|1/07/2020|12:00 AM|All Campuses|
  
    Staff Meeting|15/07/2020|12:00 AM|All Campuses|
  
    Staff Meeting|29/07/2020|12:00 AM|All Campuses|
  
    Staff Meeting|12/08/2020|12:00 AM|All Campuses|
  
    Staff Meeting|26/08/2020|12:00 AM|All Campuses|
  
    Staff Meeting|9/09/2020|12:00 AM|All Campuses|
  
    Staff Meeting|23/09/2020|12:00 AM|All Campuses|
  
    Staff Meeting|7/10/2020|12:00 AM|All Campuses|
  
    Staff Meeting|21/10/2020|12:00 AM|All Campuses|
  
    Staff Meeting|4/11/2020|12:00 AM|All Campuses|
  
    Staff Meeting|18/11/2020|12:00 AM|All Campuses|
  
    Staff Meeting|2/12/2020|12:00 AM|All Campuses|
  
    Staff Meeting|16/12/2020|12:00 AM|All Campuses|
  
    Staff Meeting|30/12/2020|12:00 AM|All Campuses|
";
            Assert.That.Contains( output, "Liquid error: Execution Timeout Expired." );
        }
        */
        /*
                [TestMethod]
                public void SqlSelectLongTimeoutShouldPass()
                {
                    var lavaScript = @"{% sql timeout:'40' %}

                    WAITFOR DELAY '00:00:35';
                    SELECT TOP 5 * 
                    FROM Person
                    {% endsql %}

                    [
                    {%- for item in results -%}
                        {
                                ""CreatedDateTime"": {{ item.CreatedDateTime | ToJSON }},
                                ""LastName"": {{ item.LastName | ToJSON }},
                        }{% unless forloop.last -%},{% endunless %}
                    {%- endfor -%}
                    ]";

                    var output = lavaScript.ResolveMergeFields( new Dictionary<string, object>(), null, "Sql" );
                    Assert.That.IsFalse( output.Contains( "Liquid error" ) );
                }

                [TestMethod]
                public void SqlSelectNoTimeoutShouldPass()
                {
                    var lavaScript = @"{% sql %}

                    SELECT TOP 5 * 
                    FROM Person
                    {% endsql %}

                    [
                    {%- for item in results -%}
                        {
                                ""CreatedDateTime"": {{ item.CreatedDateTime | ToJSON }},
                                ""LastName"": {{ item.LastName | ToJSON }},
                        }{% unless forloop.last -%},{% endunless %}
                    {%- endfor -%}
                    ]";

                    var output = lavaScript.ResolveMergeFields( new Dictionary<string, object>(), null, "Sql" );
                    Assert.That.IsFalse( output.Contains( "Liquid error" ) );
                }

                [TestMethod]
                public void SqlSelectNoTimeoutButQueryLongerThen30SecondsShouldFail()
                {
                    var lavaScript = @"{% sql %}

                    WAITFOR DELAY '00:00:35';
                    SELECT TOP 5 * 
                    FROM Person
                    {% endsql %}

                    [
                    {%- for item in results -%}
                        {
                                ""CreatedDateTime"": {{ item.CreatedDateTime | ToJSON }},
                                ""LastName"": {{ item.LastName | ToJSON }},
                        }{% unless forloop.last -%},{% endunless %}
                    {%- endfor -%}
                    ]";

                    var output = lavaScript.ResolveMergeFields( new Dictionary<string, object>(), null, "Sql" );
                    Assert.That.Contains( output, "Liquid error: Execution Timeout Expired." );
                }

                [TestMethod]
                public void SqlCommandShortTimeoutShouldFail()
                {
                    var lavaScript = @"{% sql statement:'command' timeout:'10' %}
                        WAITFOR DELAY '00:00:20';
                        DELETE FROM [DefinedValue] WHERE 1 != 1
                    {% endsql %}

                    {{ results }} {{ 'record' | PluralizeForQuantity:results }} were deleted.";

                    var output = lavaScript.ResolveMergeFields( new Dictionary<string, object>(), null, "Sql" );
                    Assert.That.Contains( output, "Liquid error: Execution Timeout Expired." );
                }

                [TestMethod]
                public void SqlCommandLongTimeoutShouldPass()
                {
                    var lavaScript = @"{% sql statement:'command' timeout:'40' %}
                        WAITFOR DELAY '00:00:35';
                        DELETE FROM [DefinedValue] WHERE 1 != 1
                    {% endsql %}

                    {{ results }} {{ 'record' | PluralizeForQuantity:results }} were deleted.";

                    var output = lavaScript.ResolveMergeFields( new Dictionary<string, object>(), null, "Sql" );
                    Assert.That.IsFalse( output.Contains( "Liquid error" ) );
                }

                [TestMethod]
                public void SqlCommandNoTimeoutShouldPass()
                {
                    var lavaScript = @"{% sql statement:'command' %}
                        DELETE FROM [DefinedValue] WHERE 1 != 1
                    {% endsql %}

                    {{ results }} {{ 'record' | PluralizeForQuantity:results }} were deleted.";

                    var output = lavaScript.ResolveMergeFields( new Dictionary<string, object>(), null, "Sql" );
                    Assert.That.IsFalse( output.Contains( "Liquid error" ) );
                }

                [TestMethod]
                public void SqlCommandNoTimeoutButQueryLongerThen30SecondsShouldFail()
                {
                    var lavaScript = @"{% sql statement:'command' %}
                        WAITFOR DELAY '00:00:35';
                        DELETE FROM [DefinedValue] WHERE 1 != 1
                    {% endsql %}

                    {{ results }} {{ 'record' | PluralizeForQuantity:results }} were deleted.";

                    var output = lavaScript.ResolveMergeFields( new Dictionary<string, object>(), null, "Sql" );
                    Assert.That.Contains( output, "Liquid error: Execution Timeout Expired." );
                }
        */
    }

}
