using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared;

namespace Rock.Tests.Integration.RockTests.Model
{
    /// <summary>
    /// Tests for ConnectionRequestService that use the database
    /// </summary>
    [TestClass]
    public class ConnectionRequestServiceTests
    {
        private const string ForeignKey = nameof( ConnectionRequestServiceTests );
        private const string CareTeamConnectionTypeGuidString = "96825939-5C02-4112-AFF7-4EC71FBCA06D";

        private static int OpportunityPrayerPartnerId;
        private static int OpportunityHospitalVisitorId;

        private static int StatusAlphaId;
        private static int StatusBravoId;
        private static int StatusCharlieId;

        #region Setup Methods

        /// <summary>
        /// Create the data used to test
        /// </summary>
        private static void CreateTestData()
        {
            var rockContext = new RockContext();
            var connectionTypeService = new ConnectionTypeService( rockContext );
            var connectionTypeGuid = CareTeamConnectionTypeGuidString.AsGuid();

            // Statuses
            var statusAlpha = new ConnectionStatus
            {
                Name = "Status: Alpha",
                ForeignKey = ForeignKey
            };

            var statusBravo = new ConnectionStatus
            {
                Name = "Status: Bravo",
                ForeignKey = ForeignKey
            };

            var statusCharlie = new ConnectionStatus
            {
                Name = "Status: Charlie",
                ForeignKey = ForeignKey
            };

            // Opportunities
            var opportunityHospitalVisitor = new ConnectionOpportunity
            {
                Name = "Opportunity: Hopsital Visitor",
                PublicName = "Opportunity: Hopsital Visitor",
                ForeignKey = ForeignKey
            };

            var opportunityPrayerPartner = new ConnectionOpportunity
            {
                Name = "Opportunity: PrayerPartner",
                PublicName = "Opportunity: PrayerPartner",
                ForeignKey = ForeignKey
            };

            // Type
            var typeCareTeam = new ConnectionType
            {
                Name = "Type: Care Team",
                Guid = connectionTypeGuid,
                ConnectionStatuses = new List<ConnectionStatus> {
                    statusAlpha,
                    statusBravo,
                    statusCharlie
                },
                ConnectionOpportunities = new List<ConnectionOpportunity> {
                    opportunityHospitalVisitor,
                    opportunityPrayerPartner
                },
                ForeignKey = ForeignKey
            };

            // Save to the database
            connectionTypeService.Add( typeCareTeam );
            rockContext.SaveChanges();

            // Set static ids
            OpportunityHospitalVisitorId = opportunityHospitalVisitor.Id;
            OpportunityPrayerPartnerId = opportunityPrayerPartner.Id;

            StatusAlphaId = statusAlpha.Id;
            StatusBravoId = statusBravo.Id;
            StatusCharlieId = statusCharlie.Id;

            // Workflow triggers
            var workflowTypeService = new WorkflowTypeService( rockContext );
            var firstWorkflowTypeId = workflowTypeService.Queryable().FirstOrDefault()?.Id ?? 0;

            var workflowActivityAdded = new ConnectionWorkflow
            {
                WorkflowTypeId = firstWorkflowTypeId,
                TriggerType = ConnectionWorkflowTriggerType.ActivityAdded,
            };

            var workflowStatusChangedBravoToAlpha = new ConnectionWorkflow
            {
                WorkflowTypeId = firstWorkflowTypeId,
                TriggerType = ConnectionWorkflowTriggerType.StatusChanged,
                QualifierValue = $"|{StatusBravoId}|{StatusAlphaId}|"
            };

            var workflowStatusChangedToCharlie = new ConnectionWorkflow
            {
                WorkflowTypeId = firstWorkflowTypeId,
                TriggerType = ConnectionWorkflowTriggerType.StatusChanged,
                QualifierValue = $"||{StatusCharlieId}|"
            };

            opportunityHospitalVisitor.ConnectionWorkflows.Add( workflowActivityAdded );
            opportunityHospitalVisitor.ConnectionWorkflows.Add( workflowStatusChangedBravoToAlpha );
            typeCareTeam.ConnectionWorkflows.Add( workflowStatusChangedToCharlie );

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Delete test data
        /// </summary>
        private static void DeleteTestData()
        {
            var rockContext = new RockContext();
            var connectionTypeGuid = CareTeamConnectionTypeGuidString.AsGuid();

            var connectionOpportunityService = new ConnectionOpportunityService( rockContext );
            var opportunityQuery = connectionOpportunityService.Queryable().Where( co => co.ConnectionType.Guid == connectionTypeGuid );
            connectionOpportunityService.DeleteRange( opportunityQuery );

            var connectionTypeService = new ConnectionTypeService( rockContext );
            var typeQuery = connectionTypeService.Queryable().Where( ct => ct.Guid == connectionTypeGuid );
            connectionTypeService.DeleteRange( typeQuery );

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Runs before any tests in this class are executed.
        /// </summary>
        [ClassInitialize]
        public static void ClassInitialize( TestContext _ )
        {
            DeleteTestData();
            CreateTestData();
        }

        /// <summary>
        /// Runs after all tests in this class is executed.
        /// </summary>
        [ClassCleanup]
        public static void ClassCleanup()
        {
            DeleteTestData();
        }

        #endregion Setup Methods

        #region DoesStatusChangeCauseWorkflows

        /// <summary>
        /// Tests DoesStatusChangeCauseWorkflows
        /// </summary>
        [TestMethod]
        public void DoesStatusChangeCauseWorkflows_AlphaToBravo()
        {
            var rockContext = new RockContext();
            var service = new ConnectionRequestService( rockContext );

            var result = service.DoesStatusChangeCauseWorkflows( OpportunityHospitalVisitorId, StatusAlphaId, StatusBravoId );
            Assert.That.IsNotNull( result );

            Assert.That.AreEqual( false, result.DoesCauseWorkflows );
            Assert.That.AreEqual( "Status: Alpha", result.FromStatusName );
            Assert.That.AreEqual( "Status: Bravo", result.ToStatusName );
        }

        /// <summary>
        /// Tests DoesStatusChangeCauseWorkflows
        /// </summary>
        [TestMethod]
        public void DoesStatusChangeCauseWorkflows_BravoToAlpha()
        {
            var rockContext = new RockContext();
            var service = new ConnectionRequestService( rockContext );

            var result = service.DoesStatusChangeCauseWorkflows( OpportunityHospitalVisitorId, StatusBravoId, StatusAlphaId );
            Assert.That.IsNotNull( result );

            Assert.That.AreEqual( true, result.DoesCauseWorkflows );
            Assert.That.AreEqual( "Status: Bravo", result.FromStatusName );
            Assert.That.AreEqual( "Status: Alpha", result.ToStatusName );
        }

        /// <summary>
        /// Tests DoesStatusChangeCauseWorkflows
        /// </summary>
        [TestMethod]
        public void DoesStatusChangeCauseWorkflows_BravoToCharlie()
        {
            var rockContext = new RockContext();
            var service = new ConnectionRequestService( rockContext );

            var result = service.DoesStatusChangeCauseWorkflows( OpportunityHospitalVisitorId, StatusBravoId, StatusCharlieId );
            Assert.That.IsNotNull( result );

            Assert.That.AreEqual( true, result.DoesCauseWorkflows );
            Assert.That.AreEqual( "Status: Bravo", result.FromStatusName );
            Assert.That.AreEqual( "Status: Charlie", result.ToStatusName );
        }

        /// <summary>
        /// Tests DoesStatusChangeCauseWorkflows
        /// </summary>
        [TestMethod]
        public void DoesStatusChangeCauseWorkflows_AlphaToCharlie()
        {
            var rockContext = new RockContext();
            var service = new ConnectionRequestService( rockContext );

            var result = service.DoesStatusChangeCauseWorkflows( OpportunityHospitalVisitorId, StatusAlphaId, StatusCharlieId );
            Assert.That.IsNotNull( result );

            Assert.That.AreEqual( true, result.DoesCauseWorkflows );
            Assert.That.AreEqual( "Status: Alpha", result.FromStatusName );
            Assert.That.AreEqual( "Status: Charlie", result.ToStatusName );
        }

        /// <summary>
        /// Tests DoesStatusChangeCauseWorkflows
        /// </summary>
        [TestMethod]
        public void DoesStatusChangeCauseWorkflows_CharlieToAlpha()
        {
            var rockContext = new RockContext();
            var service = new ConnectionRequestService( rockContext );

            var result = service.DoesStatusChangeCauseWorkflows( OpportunityHospitalVisitorId, StatusCharlieId, StatusAlphaId );
            Assert.That.IsNotNull( result );

            Assert.That.AreEqual( false, result.DoesCauseWorkflows );
            Assert.That.AreEqual( "Status: Charlie", result.FromStatusName );
            Assert.That.AreEqual( "Status: Alpha", result.ToStatusName );
        }

        /// <summary>
        /// Tests DoesStatusChangeCauseWorkflows
        /// </summary>
        [TestMethod]
        public void DoesStatusChangeCauseWorkflows_BravoToAlpha_PrayerPartner()
        {
            var rockContext = new RockContext();
            var service = new ConnectionRequestService( rockContext );

            var result = service.DoesStatusChangeCauseWorkflows( OpportunityPrayerPartnerId, StatusBravoId, StatusAlphaId );
            Assert.That.IsNotNull( result );

            Assert.That.AreEqual( false, result.DoesCauseWorkflows );
            Assert.That.AreEqual( "Status: Bravo", result.FromStatusName );
            Assert.That.AreEqual( "Status: Alpha", result.ToStatusName );
        }

        /// <summary>
        /// Tests DoesStatusChangeCauseWorkflows
        /// </summary>
        [TestMethod]
        public void DoesStatusChangeCauseWorkflows_BravoToCharlie_PrayerPartner()
        {
            var rockContext = new RockContext();
            var service = new ConnectionRequestService( rockContext );

            var result = service.DoesStatusChangeCauseWorkflows( OpportunityPrayerPartnerId, StatusBravoId, StatusCharlieId );
            Assert.That.IsNotNull( result );

            Assert.That.AreEqual( true, result.DoesCauseWorkflows );
            Assert.That.AreEqual( "Status: Bravo", result.FromStatusName );
            Assert.That.AreEqual( "Status: Charlie", result.ToStatusName );
        }

        #endregion DoesStatusChangeCauseWorkflows
    }
}
