using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Web.Cache;

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
        private const string YouthProgramConnectionTypeGuidString = "3830A69F-71FF-4923-8D46-DC2BC71F2815";
        private const string JerryJenkinsPersonGuidString = "B8E6242D-B52E-4659-AB13-751A5F4C0BE4";
        private const string KathyColePersonGuidString = "64BD1D38-D054-488F-86F6-38040242219E";
        private const string BarryBopPersonGuidString = "DFEFD90E-A993-493D-84D8-6903946523DB";
        private const string SimonSandsPersonGuidString = "D2D57C31-89C4-4A92-8917-894B49A42CAE";

        private static int PersonAliasJerryJenkinsId;
        private static int PersonAliasKathyColeId;
        private static int PersonAliasBarryBopId;
        private static int PersonAliasSimonSandsId;

        private static int TypeCareTeamId;
        private static int TypeYouthProgramId;

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
            var personService = new PersonService( rockContext );

            var typeCareTeamGuid = CareTeamConnectionTypeGuidString.AsGuid();
            var typeYouthProgramGuid = YouthProgramConnectionTypeGuidString.AsGuid();

            // People
            var personSimonSands = new Person
            {
                FirstName = "Simon",
                LastName = "Sands",
                Guid = SimonSandsPersonGuidString.AsGuid(),
                ForeignKey = ForeignKey
            };

            var personBarryBop = new Person
            {
                FirstName = "Barry",
                LastName = "Bop",
                Guid = BarryBopPersonGuidString.AsGuid(),
                ForeignKey = ForeignKey
            };

            var personKathyCole = new Person
            {
                FirstName = "Kathy",
                LastName = "Cole",
                Guid = KathyColePersonGuidString.AsGuid(),
                ForeignKey = ForeignKey
            };

            var personJerryJenkins = new Person
            {
                FirstName = "Jerry",
                LastName = "Jenkins",
                Guid = JerryJenkinsPersonGuidString.AsGuid(),
                ForeignKey = ForeignKey
            };

            personService.Add( personJerryJenkins );
            personService.Add( personSimonSands );
            personService.Add( personKathyCole );
            personService.Add( personBarryBop );

            rockContext.SaveChanges();

            PersonAliasJerryJenkinsId = personJerryJenkins.PrimaryAliasId.Value;
            PersonAliasBarryBopId = personBarryBop.PrimaryAliasId.Value;
            PersonAliasKathyColeId = personKathyCole.PrimaryAliasId.Value;
            PersonAliasSimonSandsId = personSimonSands.PrimaryAliasId.Value;

            // Statuses
            var youthProgramStatusAlpha = new ConnectionStatus
            {
                Name = "Youth Program Status: Alpha",
                ForeignKey = ForeignKey
            };

            var careTeamStatusAlpha = new ConnectionStatus
            {
                Name = "Care Team Status: Alpha",
                ForeignKey = ForeignKey
            };

            var careTeamStatusBravo = new ConnectionStatus
            {
                Name = "Care Team Status: Bravo",
                ForeignKey = ForeignKey
            };

            var careTeamStatusCharlie = new ConnectionStatus
            {
                Name = "Care Team Status: Charlie",
                ForeignKey = ForeignKey
            };

            // Opportunities
            var youthProgramOpportunityGroupLeader = new ConnectionOpportunity
            {
                Name = "Youth Program Opportunity: Group Leader",
                PublicName = "Youth Program Opportunity: Group Leader",
                ForeignKey = ForeignKey
            };

            var careTeamOpportunityHospitalVisitor = new ConnectionOpportunity
            {
                Name = "Care Team Opportunity: Hopsital Visitor",
                PublicName = "Care Team Opportunity: Hopsital Visitor",
                ForeignKey = ForeignKey
            };

            var careTeamOpportunityPrayerPartner = new ConnectionOpportunity
            {
                Name = "Care Team Opportunity: PrayerPartner",
                PublicName = "Care Team Opportunity: PrayerPartner",
                ForeignKey = ForeignKey
            };

            // Type
            var typeYouthProgram = new ConnectionType
            {
                Name = "Type: Youth Program",
                Guid = typeYouthProgramGuid,
                RequiresPlacementGroupToConnect = false,
                ConnectionStatuses = new List<ConnectionStatus> {
                    youthProgramStatusAlpha
                },
                ConnectionOpportunities = new List<ConnectionOpportunity> {
                    youthProgramOpportunityGroupLeader
                },
                ForeignKey = ForeignKey
            };

            var typeCareTeam = new ConnectionType
            {
                Name = "Type: Care Team",
                Guid = typeCareTeamGuid,
                RequiresPlacementGroupToConnect = true,
                ConnectionStatuses = new List<ConnectionStatus> {
                    careTeamStatusAlpha,
                    careTeamStatusBravo,
                    careTeamStatusCharlie
                },
                ConnectionOpportunities = new List<ConnectionOpportunity> {
                    careTeamOpportunityHospitalVisitor,
                    careTeamOpportunityPrayerPartner
                },
                ForeignKey = ForeignKey
            };

            // Save to the database
            connectionTypeService.Add( typeCareTeam );
            connectionTypeService.Add( typeYouthProgram );
            rockContext.SaveChanges();

            // Set static ids
            TypeCareTeamId = typeCareTeam.Id;
            TypeYouthProgramId = typeYouthProgram.Id;

            OpportunityHospitalVisitorId = careTeamOpportunityHospitalVisitor.Id;
            OpportunityPrayerPartnerId = careTeamOpportunityPrayerPartner.Id;

            StatusAlphaId = careTeamStatusAlpha.Id;
            StatusBravoId = careTeamStatusBravo.Id;
            StatusCharlieId = careTeamStatusCharlie.Id;

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

            careTeamOpportunityHospitalVisitor.ConnectionWorkflows.Add( workflowActivityAdded );
            careTeamOpportunityHospitalVisitor.ConnectionWorkflows.Add( workflowStatusChangedBravoToAlpha );
            typeCareTeam.ConnectionWorkflows.Add( workflowStatusChangedToCharlie );

            rockContext.SaveChanges();

            // Requests
            var connectionRequestService = new ConnectionRequestService( rockContext );

            var opportunityIds = new int[] {
                OpportunityHospitalVisitorId,
                OpportunityPrayerPartnerId
            };

            var personAliasIds = new int[] {
                PersonAliasSimonSandsId,
                PersonAliasBarryBopId,
                PersonAliasKathyColeId,
                PersonAliasJerryJenkinsId
            };

            foreach ( var opportunityId in opportunityIds )
            {
                foreach ( var requesterAliasId in personAliasIds )
                {
                    foreach ( var connectorAliasId in personAliasIds )
                    {
                        connectionRequestService.Add( new ConnectionRequest
                        {
                            ConnectorPersonAliasId = null,
                            ConnectionOpportunityId = opportunityId,
                            PersonAliasId = requesterAliasId,
                            ForeignKey = ForeignKey,
                            ConnectionStatusId = StatusAlphaId
                        } );

                        connectionRequestService.Add( new ConnectionRequest
                        {
                            ConnectorPersonAliasId = connectorAliasId,
                            ConnectionOpportunityId = opportunityId,
                            PersonAliasId = requesterAliasId,
                            ForeignKey = ForeignKey,
                            ConnectionStatusId = StatusAlphaId
                        } );
                    }
                }

                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Delete test data
        /// </summary>
        private static void DeleteTestData()
        {
            var personGuids = new Guid[] {
                JerryJenkinsPersonGuidString.AsGuid(),
                BarryBopPersonGuidString.AsGuid(),
                KathyColePersonGuidString.AsGuid(),
                SimonSandsPersonGuidString.AsGuid()
            };

            var typeGuids = new Guid[] {
                CareTeamConnectionTypeGuidString.AsGuid(),
                YouthProgramConnectionTypeGuidString.AsGuid()
            };

            var rockContext = new RockContext();

            var connectionOpportunityService = new ConnectionOpportunityService( rockContext );
            var opportunityQuery = connectionOpportunityService.Queryable().Where( co => typeGuids.Contains( co.ConnectionType.Guid ) );
            connectionOpportunityService.DeleteRange( opportunityQuery );

            var connectionTypeService = new ConnectionTypeService( rockContext );
            var typeQuery = connectionTypeService.Queryable().Where( ct => typeGuids.Contains( ct.Guid ) );
            connectionTypeService.DeleteRange( typeQuery );

            var personSearchKeyService = new PersonSearchKeyService( rockContext );
            var personSearchKeyQuery = personSearchKeyService.Queryable().Where( psk => personGuids.Contains( psk.PersonAlias.Person.Guid ) );
            personSearchKeyService.DeleteRange( personSearchKeyQuery );

            var personAliasService = new PersonAliasService( rockContext );
            var personAliasQuery = personAliasService.Queryable().Where( pa => personGuids.Contains( pa.Person.Guid ) );
            personAliasService.DeleteRange( personAliasQuery );

            var personService = new PersonService( rockContext );
            var personQuery = personService.Queryable().Where( p => personGuids.Contains( p.Guid ) );
            personService.DeleteRange( personQuery );

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

        #region CanConnect

        /// <summary>
        /// Tests CanConnect
        /// </summary>
        [TestMethod]
        public void CanConnect_RequiredPlacementNotMet()
        {
            var rockContext = new RockContext();
            var service = new ConnectionRequestService( rockContext );

            var result = service.CanConnect( new ConnectionRequestViewModel {
                PlacementGroupId = null,
                ConnectionState = ConnectionState.Active
            }, ConnectionTypeCache.Get( TypeCareTeamId ) );
            Assert.That.AreEqual( false, result );
        }

        /// <summary>
        /// Tests CanConnect
        /// </summary>
        [TestMethod]
        public void CanConnect_RequiredPlacementMet()
        {
            var rockContext = new RockContext();
            var service = new ConnectionRequestService( rockContext );

            var result = service.CanConnect( new ConnectionRequestViewModel
            {
                PlacementGroupId = 1,
                ConnectionState = ConnectionState.Active
            }, ConnectionTypeCache.Get( TypeCareTeamId ) );
            Assert.That.AreEqual( true, result );
        }

        /// <summary>
        /// Tests CanConnect
        /// </summary>
        [TestMethod]
        public void CanConnect_NotRequiredPlacementActive()
        {
            var rockContext = new RockContext();
            var service = new ConnectionRequestService( rockContext );

            var result = service.CanConnect( new ConnectionRequestViewModel
            {
                PlacementGroupId = 1,
                ConnectionState = ConnectionState.Active
            }, ConnectionTypeCache.Get( TypeYouthProgramId ) );
            Assert.That.AreEqual( true, result );
        }

        /// <summary>
        /// Tests CanConnect
        /// </summary>
        [TestMethod]
        public void CanConnect_NotRequiredPlacementInactive()
        {
            var rockContext = new RockContext();
            var service = new ConnectionRequestService( rockContext );

            var result = service.CanConnect( new ConnectionRequestViewModel
            {
                PlacementGroupId = null,
                ConnectionState = ConnectionState.Inactive
            }, ConnectionTypeCache.Get( TypeYouthProgramId ) );
            Assert.That.AreEqual( false, result );
        }

        #endregion CanConnect

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
            Assert.That.AreEqual( "Care Team Status: Alpha", result.FromStatusName );
            Assert.That.AreEqual( "Care Team Status: Bravo", result.ToStatusName );
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
            Assert.That.AreEqual( "Care Team Status: Bravo", result.FromStatusName );
            Assert.That.AreEqual( "Care Team Status: Alpha", result.ToStatusName );
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
            Assert.That.AreEqual( "Care Team Status: Bravo", result.FromStatusName );
            Assert.That.AreEqual( "Care Team Status: Charlie", result.ToStatusName );
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
            Assert.That.AreEqual( "Care Team Status: Alpha", result.FromStatusName );
            Assert.That.AreEqual( "Care Team Status: Charlie", result.ToStatusName );
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
            Assert.That.AreEqual( "Care Team Status: Charlie", result.FromStatusName );
            Assert.That.AreEqual( "Care Team Status: Alpha", result.ToStatusName );
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
            Assert.That.AreEqual( "Care Team Status: Bravo", result.FromStatusName );
            Assert.That.AreEqual( "Care Team Status: Alpha", result.ToStatusName );
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
            Assert.That.AreEqual( "Care Team Status: Bravo", result.FromStatusName );
            Assert.That.AreEqual( "Care Team Status: Charlie", result.ToStatusName );
        }

        #endregion DoesStatusChangeCauseWorkflows

        #region GetConnectionBoardStatusViewModels

        /// <summary>
        /// Tests GetConnectionBoardStatusViewModels
        /// </summary>
        [TestMethod]
        public void GetConnectionBoardStatusViewModels()
        {
            var rockContext = new RockContext();
            var service = new ConnectionRequestService( rockContext );

            var args = new ConnectionRequestViewModelQueryArgs { };
            var maxRequestsPerCol = 5;

            var result = service.GetConnectionBoardStatusViewModels(
                PersonAliasJerryJenkinsId,
                OpportunityHospitalVisitorId,
                args,
                null,
                maxRequestsPerCol );

            Assert.That.IsNotNull( result );

            foreach ( var statusViewModel in result )
            {
                Assert.That.IsNotNull( statusViewModel.Requests );
                Assert.That.IsTrue( statusViewModel.Requests.Count <= maxRequestsPerCol );
            }
        }

        #endregion GetConnectionBoardStatusViewModels
    }
}
