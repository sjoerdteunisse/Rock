//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the T4\Model.tt template.
//
//     Changes to this file will be lost when the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Quartz;
using System.Web.Compilation;

using Rock.Models.Util;
using Rock.Repository.Util;

namespace Rock.Services.Util
{
    public partial class JobService
    {
		
        public IEnumerable<Rock.Models.Util.Job> GetActiveJobs()
        {
            return Repository.Find( t => t.Active == true );
        }

        public IJobDetail BuildQuartzJob(Job job)
        {
            // build the type object, will depend if the class is in an assembly or the App_Code folder
            Type type = null;
            if ( job.Assemby == string.Empty || job.Assemby == null )
            {
                type = BuildManager.GetType( job.Class, false );
            }
            else
            {
                string thetype = string.Format( "{0}, {1}", job.Class, job.Assemby );
                type = Type.GetType( thetype );
            }

            // create attributes if needed 
            // TODO: next line should be moved to Job creation UI, when it's created
            //Rock.Helpers.Attributes.CreateAttributes( type, "Rock.Models.Util.Job", "Class", job.Class, null );

            // load up job attributes (parameters) 
            Rock.Helpers.Attributes.LoadAttributes( job );

            JobDataMap map = new JobDataMap();

            foreach ( KeyValuePair<string, KeyValuePair<string, string>> attrib in job.AttributeValues )
            {
                map.Add( attrib.Key, attrib.Value.Value );
            }

            // create the quartz job object
            IJobDetail jobDetail = JobBuilder.Create( type )
            .WithDescription( job.Id.ToString() )
            .WithIdentity( new Guid().ToString(), job.Name )
            .UsingJobData(map)
            .Build();

            return jobDetail;
        }

        public ITrigger BuildQuartzTrigger(Job job)
        {
            // create quartz trigger
            ITrigger trigger = ( ICronTrigger )TriggerBuilder.Create()
                .WithIdentity( new Guid().ToString(), job.Name )
                .WithCronSchedule( job.CronExpression )
                .StartNow()
                .Build();

            return trigger;
        }

    }
}
