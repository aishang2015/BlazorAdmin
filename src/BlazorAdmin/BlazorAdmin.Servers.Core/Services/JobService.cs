using Quartz;

namespace BlazorAdmin.Servers.Core.Services
{
    public interface IJobService
    {
        Task Add<TJob>(string jobName, JobDataMap? keyValuePairs) where TJob : IJob;
        Task Add<TJob>(string jobName, string cron, JobDataMap? keyValuePairs) where TJob : IJob;
        Task<string> AddWithoutName<TJob>(JobDataMap? keyValuePairs) where TJob : IJob;
        Task<string> AddWithoutName<TJob>(string cron, JobDataMap? keyValuePairs) where TJob : IJob;
        Task<string> AddWithoutName<TJob>(DateTime triggerTime, JobDataMap? keyValuePairs) where TJob : IJob;
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
               .SetJobData(keyValuePairs ?? new JobDataMap())
               .Build();

            var trigger = TriggerBuilder.Create()
               .WithIdentity(GetTriggerIdentity(jobName))
               .StartNow()
               .Build();

            await scheduler.ScheduleJob(job, trigger);
        }

        public async Task Add<TJob>(string jobName, string cron, JobDataMap? keyValuePairs) where TJob : IJob
        {
            var scheduler = await _schedulerFactory.GetScheduler();

            if (await scheduler.CheckExists(new JobKey(jobName)))
            {
                return;
            }

            var job = JobBuilder.Create<TJob>()
               .WithIdentity(jobName)
               .DisallowConcurrentExecution()
               .SetJobData(keyValuePairs ?? new JobDataMap())
               .Build();

            var trigger = TriggerBuilder.Create()
               .WithIdentity(GetTriggerIdentity(jobName))
               .WithCronSchedule(cron, m => m.WithMisfireHandlingInstructionIgnoreMisfires())
               .Build();

            await scheduler.ScheduleJob(job, trigger);
        }

        public async Task<string> AddWithoutName<TJob>(JobDataMap? keyValuePairs) where TJob : IJob
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            var jobName = typeof(TJob).Name + Guid.NewGuid().ToString();

            var job = JobBuilder.Create<TJob>()
               .WithIdentity(jobName)
               .DisallowConcurrentExecution()
               .SetJobData(keyValuePairs ?? new JobDataMap())
               .Build();

            var trigger = TriggerBuilder.Create()
               .WithIdentity(GetTriggerIdentity(jobName))
               .StartNow()
               .Build();

            await scheduler.ScheduleJob(job, trigger);
            return jobName;
        }

        public async Task<string> AddWithoutName<TJob>(string cron, JobDataMap? keyValuePairs) where TJob : IJob
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            var jobName = typeof(TJob).Name + Guid.NewGuid().ToString();

            var job = JobBuilder.Create<TJob>()
               .WithIdentity(jobName)
               .DisallowConcurrentExecution()
               .SetJobData(keyValuePairs ?? new JobDataMap())
               .Build();

            var trigger = TriggerBuilder.Create()
               .WithIdentity(GetTriggerIdentity(jobName))
               .WithCronSchedule(cron, m => m.WithMisfireHandlingInstructionIgnoreMisfires())
               .Build();

            await scheduler.ScheduleJob(job, trigger);
            return jobName;
        }

        public async Task<string> AddWithoutName<TJob>(DateTime triggerTime, JobDataMap? keyValuePairs) where TJob : IJob
        {
            var triggerTimeCron = $"{triggerTime.Second} {triggerTime.Minute} {triggerTime.Hour} {triggerTime.Day} {triggerTime.Month} ? {triggerTime.Year}";

            var scheduler = await _schedulerFactory.GetScheduler();
            var jobName = typeof(TJob).Name + Guid.NewGuid().ToString();

            var job = JobBuilder.Create<TJob>()
               .WithIdentity(jobName)
               .DisallowConcurrentExecution()
               .SetJobData(keyValuePairs ?? new JobDataMap())
               .Build();

            var trigger = TriggerBuilder.Create()
               .WithIdentity(GetTriggerIdentity(jobName))
               .WithCronSchedule(triggerTimeCron, m => m.WithMisfireHandlingInstructionIgnoreMisfires())
               .Build();

            await scheduler.ScheduleJob(job, trigger);
            return jobName;
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
