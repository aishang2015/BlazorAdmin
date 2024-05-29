using BlazorAdmin.Core.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorAdmin.Core.Extension
{
    public static class QuartzExtension
    {
        public static IServiceCollection AddQuartzService(this IServiceCollection services,
            string connectionString)
        {
            services.AddQuartz(q =>
            {
                q.UseDefaultThreadPool(x => x.MaxConcurrency = 5);
                q.UsePersistentStore(x =>
                {
                    x.UseBinarySerializer();
                    x.UseProperties = true;
                    //x.UseClustering(); // sqlite 无法使用clustering
                    x.UseMicrosoftSQLite(opt => opt.ConnectionString = connectionString);
                });
            });

            services.AddQuartzHostedService(options =>
            {
                // when shutting down we want jobs to complete gracefully
                options.WaitForJobsToComplete = true;
            });

            services.AddSingleton<IJobService, JobService>();

            return services;
        }

        #region Sqlite Script

        private const string SqliteScript = @"

            CREATE TABLE IF NOT EXISTS QRTZ_JOB_DETAILS
              (
                SCHED_NAME NVARCHAR(120) NOT NULL,
	            JOB_NAME NVARCHAR(150) NOT NULL,
                JOB_GROUP NVARCHAR(150) NOT NULL,
                DESCRIPTION NVARCHAR(250) NULL,
                JOB_CLASS_NAME   NVARCHAR(250) NOT NULL,
                IS_DURABLE BIT NOT NULL,
                IS_NONCONCURRENT BIT NOT NULL,
                IS_UPDATE_DATA BIT  NOT NULL,
	            REQUESTS_RECOVERY BIT NOT NULL,
                JOB_DATA BLOB NULL,
                PRIMARY KEY (SCHED_NAME,JOB_NAME,JOB_GROUP)
            );

            CREATE TABLE IF NOT EXISTS QRTZ_TRIGGERS
              (
                SCHED_NAME NVARCHAR(120) NOT NULL,
	            TRIGGER_NAME NVARCHAR(150) NOT NULL,
                TRIGGER_GROUP NVARCHAR(150) NOT NULL,
                JOB_NAME NVARCHAR(150) NOT NULL,
                JOB_GROUP NVARCHAR(150) NOT NULL,
                DESCRIPTION NVARCHAR(250) NULL,
                NEXT_FIRE_TIME BIGINT NULL,
                PREV_FIRE_TIME BIGINT NULL,
                PRIORITY INTEGER NULL,
                TRIGGER_STATE NVARCHAR(16) NOT NULL,
                TRIGGER_TYPE NVARCHAR(8) NOT NULL,
                START_TIME BIGINT NOT NULL,
                END_TIME BIGINT NULL,
                CALENDAR_NAME NVARCHAR(200) NULL,
                MISFIRE_INSTR INTEGER NULL,
                JOB_DATA BLOB NULL,
                PRIMARY KEY (SCHED_NAME,TRIGGER_NAME,TRIGGER_GROUP),
                FOREIGN KEY (SCHED_NAME,JOB_NAME,JOB_GROUP)
                    REFERENCES QRTZ_JOB_DETAILS(SCHED_NAME,JOB_NAME,JOB_GROUP)
            );

            CREATE TABLE IF NOT EXISTS QRTZ_SIMPLE_TRIGGERS
              (
                SCHED_NAME NVARCHAR(120) NOT NULL,
	            TRIGGER_NAME NVARCHAR(150) NOT NULL,
                TRIGGER_GROUP NVARCHAR(150) NOT NULL,
                REPEAT_COUNT BIGINT NOT NULL,
                REPEAT_INTERVAL BIGINT NOT NULL,
                TIMES_TRIGGERED BIGINT NOT NULL,
                PRIMARY KEY (SCHED_NAME,TRIGGER_NAME,TRIGGER_GROUP),
                FOREIGN KEY (SCHED_NAME,TRIGGER_NAME,TRIGGER_GROUP)
                    REFERENCES QRTZ_TRIGGERS(SCHED_NAME,TRIGGER_NAME,TRIGGER_GROUP) ON DELETE CASCADE
            );

            CREATE TRIGGER IF NOT EXISTS DELETE_SIMPLE_TRIGGER DELETE ON QRTZ_TRIGGERS
            BEGIN
	            DELETE FROM QRTZ_SIMPLE_TRIGGERS WHERE SCHED_NAME=OLD.SCHED_NAME AND TRIGGER_NAME=OLD.TRIGGER_NAME AND TRIGGER_GROUP=OLD.TRIGGER_GROUP;
            END
            ;

            CREATE TABLE IF NOT EXISTS QRTZ_SIMPROP_TRIGGERS 
              (
                SCHED_NAME NVARCHAR (120) NOT NULL ,
                TRIGGER_NAME NVARCHAR (150) NOT NULL ,
                TRIGGER_GROUP NVARCHAR (150) NOT NULL ,
                STR_PROP_1 NVARCHAR (512) NULL,
                STR_PROP_2 NVARCHAR (512) NULL,
                STR_PROP_3 NVARCHAR (512) NULL,
                INT_PROP_1 INT NULL,
                INT_PROP_2 INT NULL,
                LONG_PROP_1 BIGINT NULL,
                LONG_PROP_2 BIGINT NULL,
                DEC_PROP_1 NUMERIC NULL,
                DEC_PROP_2 NUMERIC NULL,
                BOOL_PROP_1 BIT NULL,
                BOOL_PROP_2 BIT NULL,
                TIME_ZONE_ID NVARCHAR(80) NULL,
	            PRIMARY KEY (SCHED_NAME,TRIGGER_NAME,TRIGGER_GROUP),
	            FOREIGN KEY (SCHED_NAME,TRIGGER_NAME,TRIGGER_GROUP)
                    REFERENCES QRTZ_TRIGGERS(SCHED_NAME,TRIGGER_NAME,TRIGGER_GROUP) ON DELETE CASCADE
            );

            CREATE TRIGGER IF NOT EXISTS DELETE_SIMPROP_TRIGGER DELETE ON QRTZ_TRIGGERS
            BEGIN
	            DELETE FROM QRTZ_SIMPROP_TRIGGERS WHERE SCHED_NAME=OLD.SCHED_NAME AND TRIGGER_NAME=OLD.TRIGGER_NAME AND TRIGGER_GROUP=OLD.TRIGGER_GROUP;
            END
            ;

            CREATE TABLE IF NOT EXISTS QRTZ_CRON_TRIGGERS
              (
                SCHED_NAME NVARCHAR(120) NOT NULL,
	            TRIGGER_NAME NVARCHAR(150) NOT NULL,
                TRIGGER_GROUP NVARCHAR(150) NOT NULL,
                CRON_EXPRESSION NVARCHAR(250) NOT NULL,
                TIME_ZONE_ID NVARCHAR(80),
                PRIMARY KEY (SCHED_NAME,TRIGGER_NAME,TRIGGER_GROUP),
                FOREIGN KEY (SCHED_NAME,TRIGGER_NAME,TRIGGER_GROUP)
                    REFERENCES QRTZ_TRIGGERS(SCHED_NAME,TRIGGER_NAME,TRIGGER_GROUP) ON DELETE CASCADE
            );

            CREATE TRIGGER IF NOT EXISTS DELETE_CRON_TRIGGER DELETE ON QRTZ_TRIGGERS
            BEGIN
	            DELETE FROM QRTZ_CRON_TRIGGERS WHERE SCHED_NAME=OLD.SCHED_NAME AND TRIGGER_NAME=OLD.TRIGGER_NAME AND TRIGGER_GROUP=OLD.TRIGGER_GROUP;
            END
            ;

            CREATE TABLE IF NOT EXISTS QRTZ_BLOB_TRIGGERS
              (
                SCHED_NAME NVARCHAR(120) NOT NULL,
	            TRIGGER_NAME NVARCHAR(150) NOT NULL,
                TRIGGER_GROUP NVARCHAR(150) NOT NULL,
                BLOB_DATA BLOB NULL,
                PRIMARY KEY (SCHED_NAME,TRIGGER_NAME,TRIGGER_GROUP),
                FOREIGN KEY (SCHED_NAME,TRIGGER_NAME,TRIGGER_GROUP)
                    REFERENCES QRTZ_TRIGGERS(SCHED_NAME,TRIGGER_NAME,TRIGGER_GROUP) ON DELETE CASCADE
            );

            CREATE TRIGGER IF NOT EXISTS DELETE_BLOB_TRIGGER DELETE ON QRTZ_TRIGGERS
            BEGIN
	            DELETE FROM QRTZ_BLOB_TRIGGERS WHERE SCHED_NAME=OLD.SCHED_NAME AND TRIGGER_NAME=OLD.TRIGGER_NAME AND TRIGGER_GROUP=OLD.TRIGGER_GROUP;
            END
            ;

            CREATE TABLE IF NOT EXISTS QRTZ_CALENDARS
              (
                SCHED_NAME NVARCHAR(120) NOT NULL,
	            CALENDAR_NAME  NVARCHAR(200) NOT NULL,
                CALENDAR BLOB NOT NULL,
                PRIMARY KEY (SCHED_NAME,CALENDAR_NAME)
            );

            CREATE TABLE IF NOT EXISTS QRTZ_PAUSED_TRIGGER_GRPS
              (
                SCHED_NAME NVARCHAR(120) NOT NULL,
	            TRIGGER_GROUP NVARCHAR(150) NOT NULL, 
                PRIMARY KEY (SCHED_NAME,TRIGGER_GROUP)
            );

            CREATE TABLE IF NOT EXISTS QRTZ_FIRED_TRIGGERS
              (
                SCHED_NAME NVARCHAR(120) NOT NULL,
	            ENTRY_ID NVARCHAR(140) NOT NULL,
                TRIGGER_NAME NVARCHAR(150) NOT NULL,
                TRIGGER_GROUP NVARCHAR(150) NOT NULL,
                INSTANCE_NAME NVARCHAR(200) NOT NULL,
                FIRED_TIME BIGINT NOT NULL,
                SCHED_TIME BIGINT NOT NULL,
	            PRIORITY INTEGER NOT NULL,
                STATE NVARCHAR(16) NOT NULL,
                JOB_NAME NVARCHAR(150) NULL,
                JOB_GROUP NVARCHAR(150) NULL,
                IS_NONCONCURRENT BIT NULL,
                REQUESTS_RECOVERY BIT NULL,
                PRIMARY KEY (SCHED_NAME,ENTRY_ID)
            );

            CREATE TABLE IF NOT EXISTS QRTZ_SCHEDULER_STATE
              (
                SCHED_NAME NVARCHAR(120) NOT NULL,
	            INSTANCE_NAME NVARCHAR(200) NOT NULL,
                LAST_CHECKIN_TIME BIGINT NOT NULL,
                CHECKIN_INTERVAL BIGINT NOT NULL,
                PRIMARY KEY (SCHED_NAME,INSTANCE_NAME)
            );

            CREATE TABLE IF NOT EXISTS QRTZ_LOCKS
              (
                SCHED_NAME NVARCHAR(120) NOT NULL,
	            LOCK_NAME  NVARCHAR(40) NOT NULL, 
                PRIMARY KEY (SCHED_NAME,LOCK_NAME)
            );";

        public static void InitialSqliteQuartzTable(string connectionString)
        {
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = SqliteScript;
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
            }
        }

        #endregion
    }


}
