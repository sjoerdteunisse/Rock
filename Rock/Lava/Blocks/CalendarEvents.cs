// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DotLiquid;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Ical.Net.DataTypes;

namespace Rock.Lava.Blocks
{
    /// <summary>
    /// A Lava Block that provides access to a filtered set of events from a specified calendar.
    /// Lava objects are created in the block context to provide access to the set of events matching the filter parmeters.
    /// The <c>EventItems</c> collection contains information about the Event instances.
    /// The <c>EventItemOccurrences</c> collection contains the actual occurrences of the event that match the filter.
    /// </summary>
    public class CalendarEvents : RockLavaBlockBase
    {
        /// <summary>
        /// The name of the element as it is used in the source document.
        /// </summary>
        public static string TagSourceName = "calendarevents";
        public static string ParameterCalendarId = "calendarid";
        public static string ParameterMaxOccurrences = "maxoccurrences";
        public static string ParameterStartDate = "startdate";
        public static string ParameterDateRange = "daterange";
        public static string ParameterAudienceIds = "audienceids";
        public static int MaximumResultSetSize = 10000;

        private string _attributesMarkup;
        private bool _renderErrors = true;

        LavaElementAttributes _settings = new LavaElementAttributes();

        /// <summary>
        /// Method that will be run at Rock startup
        /// </summary>
        public override void OnStartup()
        {
            Template.RegisterTag<CalendarEvents>( TagSourceName );
        }

        /// <summary>
        /// Initializes the specified tag name.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="markup">The markup.</param>
        /// <param name="tokens">The tokens.</param>
        public override void Initialize( string tagName, string markup, List<string> tokens )
        {
            _attributesMarkup = markup;

            base.Initialize( tagName, markup, tokens );
        }

        /// <summary>
        /// Renders the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        public override void Render( Context context, TextWriter result )
        {

            try
            {
                RenderInternal( context, result );
            }
            catch ( Exception ex )
            {
                var message = "Calendar Events not available. " + ex.Message;

                if ( _renderErrors )
                {
                    result.Write( message );
                }
                else
                {
                    ExceptionLogService.LogException( ex );
                }
            }
        }

        private void RenderInternal( Context context, TextWriter result )
        {
            // Parse the attributes markup and validate the parameters.
            _settings.ParseFromMarkup( _attributesMarkup, context );

            var unknownNames = _settings.GetUnknownAttributes( new List<string> { ParameterCalendarId, ParameterAudienceIds, ParameterDateRange, ParameterMaxOccurrences, ParameterStartDate } );

            if ( unknownNames.Any() )
            {
                throw new Exception( $"Invalid configuration setting \"{unknownNames.AsDelimited( "," )}\"." );
            }

            var rockContext = new RockContext();

            // Get the Event Calendar.
            var calendar = ResolveCalendarSettingOrThrow( rockContext, _settings.GetStringValue( ParameterCalendarId ) );

            if ( calendar == null )
            {
                throw new Exception( "The specified Calendar is invalid." );
            }

            // Get the Start Date.
            var startDate = _settings.GetDateTimeValue( ParameterStartDate, RockDateTime.Today );

            // Get the Date Range.
            DateTime? endDate = null;

            var dateRange = _settings.GetStringValue( ParameterDateRange, string.Empty ).ToLower();

            if ( !string.IsNullOrEmpty( dateRange ) )
            {
                string rangePeriod;

                if ( dateRange.IsDigitsOnly() )
                {
                    rangePeriod = "d";
                }
                else
                {
                    rangePeriod = dateRange.Right( 1 );

                    dateRange = dateRange.Substring( 0, dateRange.Length - 1 );
                }

                int? rangeAmount = dateRange.AsIntegerOrNull();

                if ( rangeAmount == null )
                {
                    throw new Exception( "The specified Date Range is invalid." );
                }

                // Get the end date by adding the range increment to the start date.
                // A range of 1 indicates that the start date and end date are the sameno change to the start date.
                var increment = rangeAmount.Value - 1;

                if ( rangePeriod == "m" )
                {
                    // Range in Months
                    endDate = startDate.Value.AddMonths( rangeAmount.Value );

                }
                else if ( rangePeriod == "w" )
                {
                    // Range in Weeks.
                    endDate = startDate.Value.AddDays( increment * 7 );
                }
                else
                {
                    // Range in Days.
                    // A range of 1 day indicates that the start date and end date should be the same.
                    endDate = startDate.Value.AddDays( increment );
                }

                // Adjust the calculated end date to the previous day because the time period is inclusive of the start and end dates.
                // For example, a range of 1 day requires that the start date and end date are the same.
                endDate = endDate.Value.AddDays( -1 );
            }

            // Get the Maximum Occurrences
            int maxOccurrences = 100;

            if ( _settings.HasValue( ParameterMaxOccurrences ) )
            {
                maxOccurrences = _settings.GetIntegerValue( ParameterMaxOccurrences, null ) ?? 0;

                if ( maxOccurrences == 0 )
                {
                    throw new Exception( $"Invalid configuration setting \"maxoccurrences\"." );
                }

            }

            // Get the Audiences
            var audienceIdList = ResolveAudienceSettingOrThrow( rockContext, _settings.GetStringValue( ParameterAudienceIds, string.Empty ) );

            var events = GetCalendarEventOccurrences( calendar.Id, audienceIdList, maxOccurrences, startDate, endDate );

            AddLavaMergeFieldsToContext( context, events );

            RenderAll( this.NodeList, context, result );
        }

        private List<int> ResolveAudienceSettingOrThrow( RockContext rockContext, string audienceSettingValue )
        {
            var audienceIdList = new List<int>();

            if ( !string.IsNullOrWhiteSpace( audienceSettingValue ) )
            {
                var definedType = DefinedTypeCache.Get( SystemGuid.DefinedType.CONTENT_CHANNEL_AUDIENCE_TYPE );

                var audiences = audienceSettingValue.SplitDelimitedValues( "," );

                foreach ( var audience in audiences )
                {
                    DefinedValueCache definedValue = null;

                    // Get by ID.
                    var audienceId = audience.AsIntegerOrNull();

                    if ( audienceId != null )
                    {
                        definedValue = definedType.DefinedValues.FirstOrDefault( x => x.Id == audienceId );

                    }

                    // Get by Guid.
                    if ( definedValue == null )
                    {
                        var audienceGuid = audience.AsGuidOrNull();

                        if ( audienceGuid != null )
                        {
                            definedValue = definedType.DefinedValues.FirstOrDefault( x => x.Guid == audienceGuid );
                        }
                    }

                    // Get by Value.
                    if ( definedValue == null )
                    {
                        var audienceValue = audience.Trim();

                        definedValue = definedType.DefinedValues.FirstOrDefault( x => x.Value == audienceValue );
                    }

                    // Report an error if the Audience is invalid.
                    if ( definedValue == null )
                    {
                        throw new Exception( $"Cannot apply an audience filter for the reference \"{ audience }\"." );
                    }

                    audienceIdList.Add( definedValue.Id );
                }
            }

            return audienceIdList;
        }

        private EventCalendar ResolveCalendarSettingOrThrow( RockContext rockContext, string calendarSettingValue )
        {
            var calendarService = new EventCalendarService( rockContext );

            EventCalendar calendar = null;

            // Verify that a calendar reference has been provided.
            if ( string.IsNullOrWhiteSpace( calendarSettingValue ) )
            {
                throw new Exception( $"A calendar reference must be specified." );
            }

            // Get by ID.
            var calendarId = calendarSettingValue.AsIntegerOrNull();

            if ( calendarId != null )
            {
                calendar = calendarService.Get( calendarId.Value );
            }

            // Get by Guid.
            if ( calendar == null )
            {
                var calendarGuid = calendarSettingValue.AsGuidOrNull();

                if ( calendarGuid != null )
                {
                    calendar = calendarService.Get( calendarGuid.Value );
                }
            }

            // Get By Name.
            if ( calendar == null )
            {
                var calendarName = calendarSettingValue.ToString();

                if ( !string.IsNullOrWhiteSpace( calendarName ) )
                {
                    calendar = calendarService.Queryable()
                        .Where( x => x.Name != null && x.Name.Equals( calendarName, StringComparison.OrdinalIgnoreCase ) )
                        .FirstOrDefault();
                }
            }

            if ( calendar == null )
            {
                throw new Exception( $"Cannot find a calendar matching the reference \"{ calendarSettingValue }\"." );
            }

            return calendar;
        }

        private List<EventOccurrenceSummary> GetCalendarEventOccurrences( int calendarId, List<int> audienceIdList, int maxOccurrences, DateTime? startDate, DateTime? endDate )
        {
            var rockContext = new RockContext();

            var eventItemOccurrenceService = new EventItemOccurrenceService( rockContext );

            // Get active and approved Event Occurrences in the specified calendar.
            var qryOccurrences = eventItemOccurrenceService
                    .Queryable( "EventItem, EventItem.EventItemAudiences,Schedule" )
                    .Where( m =>
                        m.EventItem.EventCalendarItems.Any( i => i.EventCalendarId == calendarId ) &&
                        m.EventItem.IsActive &&
                        m.EventItem.IsApproved );

            // Filter by Audience
            if ( audienceIdList != null
                 && audienceIdList.Any() )
            {
                qryOccurrences = qryOccurrences.Where( i => i.EventItem.EventItemAudiences.Any( c => audienceIdList.Contains( c.DefinedValueId ) ) );
            }

            // Get the occurrences
            if ( maxOccurrences < 1 || maxOccurrences > MaximumResultSetSize )
            {
                maxOccurrences = 100;
            }

            if ( startDate == null )
            {
                startDate = RockDateTime.Today;
            }

            // Querying the schedule occurrences requires a specific end date, but the ICal library throws an Exception for values of Date.MaxValue,
            // so we must set an arbitrary date here.
            if ( endDate == null )
            {
                endDate = startDate.Value.AddYears( 100 );
            }

            var occurrencesWithDates = qryOccurrences.ToList()
                .Select( o =>
                {
                    var eventOccurrenceDate = new EventOccurrenceDate
                    {
                        EventItemOccurrence = o

                    };

                    if ( o.Schedule != null )
                    {
                        eventOccurrenceDate.ScheduleOccurrences = o.Schedule.GetICalOccurrences( startDate.Value, endDate ).ToList();
                    }
                    else
                    {
                        eventOccurrenceDate.ScheduleOccurrences = new List<Occurrence>();
                    }

                    return eventOccurrenceDate;
                } )
                .Where( d => d.ScheduleOccurrences.Any() )
                .ToList();

            var eventOccurrenceSummaries = new List<EventOccurrenceSummary>();

            bool finished = false;

            foreach ( var occurrenceDates in occurrencesWithDates )
            {
                var eventItemOccurrence = occurrenceDates.EventItemOccurrence;

                foreach ( var scheduleOccurrence in occurrenceDates.ScheduleOccurrences )
                {
                    var datetime = scheduleOccurrence.Period.StartTime.Value;
                    var occurrenceEndTime = scheduleOccurrence.Period.EndTime;

                    if ( datetime >= startDate
                         && ( endDate == null || datetime < endDate ) )
                    {
                        eventOccurrenceSummaries.Add( new EventOccurrenceSummary
                        {
                            EventItemOccurrence = eventItemOccurrence,
                            Name = eventItemOccurrence.EventItem.Name,
                            DateTime = datetime,
                            Date = datetime.ToShortDateString(),
                            Time = datetime.ToShortTimeString(),
                            EndDate = occurrenceEndTime != null ? occurrenceEndTime.Value.ToShortDateString() : null,
                            EndTime = occurrenceEndTime != null ? occurrenceEndTime.Value.ToShortTimeString() : null,
                            Campus = eventItemOccurrence.Campus != null ? eventItemOccurrence.Campus.Name : "All Campuses",
                            Location = eventItemOccurrence.Campus != null ? eventItemOccurrence.Campus.Name : "All Campuses",
                            LocationDescription = eventItemOccurrence.Location,
                            Description = eventItemOccurrence.EventItem.Description,
                            Summary = eventItemOccurrence.EventItem.Summary,
                            OccurrenceNote = eventItemOccurrence.Note.SanitizeHtml(),
                            DetailPage = string.IsNullOrWhiteSpace( eventItemOccurrence.EventItem.DetailsUrl ) ? null : eventItemOccurrence.EventItem.DetailsUrl,
                            CalendarNames = eventItemOccurrence.EventItem.EventCalendarItems.Select( x => x.EventCalendar.Name ).ToList(),
                            AudienceNames = eventItemOccurrence.EventItem.EventItemAudiences.Select( x => x.DefinedValue.Value ).ToList(),
                        } );

                        // Exit if the occurrence limit has been reached.
                        if ( eventOccurrenceSummaries.Count >= maxOccurrences )
                        {
                            finished = true;
                            break;
                        }
                    }
                }

                if ( finished )
                {
                    break;
                }
            }

            return eventOccurrenceSummaries;
        }

        private void AddLavaMergeFieldsToContext( Context context, List<EventOccurrenceSummary> eventOccurrenceSummaries )
        {
            var eventSummaries = eventOccurrenceSummaries
                .OrderBy( e => e.DateTime )
                .GroupBy( e => e.Name )
                .Select( e => e.ToList() )
                .ToList();

            eventOccurrenceSummaries = eventOccurrenceSummaries
                .OrderBy( e => e.DateTime )
                .ThenBy( e => e.Name )
                .ToList();

            context["EventItems"] = eventSummaries;
            context["EventItemOccurrences"] = eventOccurrenceSummaries;
        }
    }

    #region Helper Classes

    /// <summary>
    /// A class to store event item occurrence data for liquid
    /// </summary>
    [DotLiquid.LiquidType( "EventItemOccurrence", "DateTime", "Name", "Date", "Time", "EndDate", "EndTime", "Campus", "Location", "LocationDescription", "Description", "Summary", "OccurrenceNote", "DetailPage", "CalendarNames", "AudienceNames" )]
    public class EventOccurrenceSummary
    {
        public EventItemOccurrence EventItemOccurrence { get; set; }

        public DateTime DateTime { get; set; }

        public string Name { get; set; }

        public string Date { get; set; }

        public string Time { get; set; }

        public string EndDate { get; set; }

        public string EndTime { get; set; }

        public string Campus { get; set; }

        public string Location { get; set; }

        public string LocationDescription { get; set; }

        public string Summary { get; set; }

        public string Description { get; set; }

        public string OccurrenceNote { get; set; }

        public string DetailPage { get; set; }

        public List<string> CalendarNames { get; set; }

        public List<string> AudienceNames { get; set; }
    }

    /// <summary>
    /// A block-level viewmodel for event item occurrences dates.
    /// </summary>
    public class EventOccurrenceDate
    {
        public EventItemOccurrence EventItemOccurrence { get; set; }

        public List<Occurrence> ScheduleOccurrences { get; set; }
    }

    #endregion
}
