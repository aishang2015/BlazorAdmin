using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorAdmin.Core.Services
{
    public interface IJobService
    {
        Task Add<TJob>(string jobName, JobDataMap? keyValuePairs) where TJob : IJob;
        Task Add<TJob>(string jobName, string cron) where TJob : IJob;
        Task Remove(string jobName);
        Task Pause(string jobName);
        Task Resume(string jobName);
    }

    public class JobService : IJobService
    {
        private readonly ISchedulerFactory _schedulerFactory;

        public JobService(ISchedulerFactory schedulerFactory)
        {
            _schedulerFactory = schedulerFactory;
        }

        public async Task Add<TJob>(string jobName, JobDataMap? keyValuePairs) where TJob : IJob
        {
            var scheduler = await _schedulerFactory.GetScheduler();

            var job = JobBuilder.Create<TJob>()
               .WithIdentity(jobName)
               .DisallowConcurrentExecution()
               .SetJobData(keyValuePairs)
               .Build();

            var trigger = TriggerBuilder.Create()
               .WithIdentity(GetTriggerIdentity(jobName))
               .StartNow()
               .Build();

            await scheduler.ScheduleJob(job, trigger);
        }

        public async Task Add<TJob>(string jobName, string cron) where TJob : IJob
        {
            var scheduler = await _schedulerFactory.GetScheduler();

            if (await scheduler.CheckExists(new JobKey(jobName)))
            {
                return;
            }

            var job = JobBuilder.Create<TJob>()
               .WithIdentity(jobName)
               .DisallowConcurrentExecution()
               .Build();

            var trigger = TriggerBuilder.Create()
               .WithIdentity(GetTriggerIdentity(jobName))
               .WithCronSchedule(cron, m => m.WithMisfireHandlingInstructionIgnoreMisfires())
               .Build();

            await scheduler.ScheduleJob(job, trigger);
        }

        public async Task Pause(string jobName)
        {
            var scheduler = await _schedulerFactory.GetScheduler();

            if (!string.IsNullOrEmpty(jobName))
            {
                var jobTrigger = await scheduler.GetTrigger(new TriggerKey(jobName + "Trigger"));
                if (jobTrigger != null)
                {
                    await scheduler.PauseTrigger(jobTrigger.Key);
                }
            }

        }

        public async Task Remove(string jobName)
        {
            var scheduler = await _schedulerFactory.GetScheduler();

            if (!string.IsNullOrEmpty(jobName))
            {
                var jobKey = new JobKey(jobName);
                if (await scheduler.CheckExists(jobKey))
                {
                    await scheduler.DeleteJob(jobKey);
                }
            }
        }

        public async Task Resume(string jobName)
        {
            var scheduler = await _schedulerFactory.GetScheduler();

            if (!string.IsNullOrEmpty(jobName))
            {
                var jobTrigger = await scheduler.GetTrigger(new TriggerKey(jobName + "Trigger"));
                if (jobTrigger != null)
                {
                    await scheduler.ResumeTrigger(jobTrigger.Key);
                }
            }
        }

        private static string GetTriggerIdentity(string jobName)
        {
            return jobName + "Trigger";
        }
    }
}
