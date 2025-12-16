using BlazorAdmin.Servers.Core.Services;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using Npgsql;
using Quartz;

namespace BlazorAdmin.Servers.Core.Data
{
    public static class QuartzExtension
    {
        public static IServiceCollection AddQuartzService(this IServiceCollection services,
            string connectionString, string dbProvider)
        {
            services.AddQuartz(q =>
            {
                q.UseDefaultThreadPool(x => x.MaxConcurrency = 5);
                q.UsePersistentStore(x =>
                {
                    //x.UseBinarySerializer();
                    x.UseProperties = true;
                    //x.UseClustering(); // sqlite 无法使用clustering

                    if (dbProvider == "Sqlite")
                    {
                        x.UseMicrosoftSQLite(opt => opt.ConnectionString = connectionString);
                    }
                    else if (dbProvider == "SqlServer")
                    {
                        x.UseSqlServer(opt => opt.ConnectionString = connectionString);
                    }
                    else if (dbProvider == "PostgreSQL")
                    {
                        x.UsePostgres(opt => opt.ConnectionString = connectionString);
                    }
                    else if (dbProvider == "MySQL")
                    {
                        x.UseMySql(opt => opt.ConnectionString = connectionString);
                    }

                    x.UseNewtonsoftJsonSerializer();
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

        public static void InitialQuartzTable(string connectionString, string dbProvider)
        {
            if (dbProvider == "Sqlite")
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
            else if (dbProvider == "SqlServer")
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = SqlServerScript;
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }
            }
            else if (dbProvider == "PostgreSQL")
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = PostgreSQLScript;
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }
            }
            else if (dbProvider == "MySQL")
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    // 先检查表是否存在
                    bool tableExists = false;
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'QRTZ_JOB_DETAILS'";
                        var result = cmd.ExecuteScalar();
                        tableExists = (result != null);
                    }

                    // 如果表不存在，则创建表
                    if (!tableExists)
                    {
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = MySQLScript;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    conn.Close();
                }
            }
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

        #endregion

        #region SqlServer Script

        private const string SqlServerScript = @"
IF NOT EXISTS (
    SELECT *
    FROM sys.objects
    WHERE object_id = OBJECT_ID(N'dbo.QRTZ_CALENDARS') AND type = N'U'
)
BEGIN

    CREATE TABLE [dbo].[QRTZ_CALENDARS] (
      [SCHED_NAME] nvarchar(120) NOT NULL,
      [CALENDAR_NAME] nvarchar(200) NOT NULL,
      [CALENDAR] varbinary(max) NOT NULL
    );

    CREATE TABLE [dbo].[QRTZ_CRON_TRIGGERS] (
      [SCHED_NAME] nvarchar(120) NOT NULL,
      [TRIGGER_NAME] nvarchar(150) NOT NULL,
      [TRIGGER_GROUP] nvarchar(150) NOT NULL,
      [CRON_EXPRESSION] nvarchar(120) NOT NULL,
      [TIME_ZONE_ID] nvarchar(80) 
    );

    CREATE TABLE [dbo].[QRTZ_FIRED_TRIGGERS] (
      [SCHED_NAME] nvarchar(120) NOT NULL,
      [ENTRY_ID] nvarchar(140) NOT NULL,
      [TRIGGER_NAME] nvarchar(150) NOT NULL,
      [TRIGGER_GROUP] nvarchar(150) NOT NULL,
      [INSTANCE_NAME] nvarchar(200) NOT NULL,
      [FIRED_TIME] bigint NOT NULL,
      [SCHED_TIME] bigint NOT NULL,
      [PRIORITY] int NOT NULL,
      [STATE] nvarchar(16) NOT NULL,
      [JOB_NAME] nvarchar(150) NULL,
      [JOB_GROUP] nvarchar(150) NULL,
      [IS_NONCONCURRENT] bit NULL,
      [REQUESTS_RECOVERY] bit NULL 
    );

    CREATE TABLE [dbo].[QRTZ_PAUSED_TRIGGER_GRPS] (
      [SCHED_NAME] nvarchar(120) NOT NULL,
      [TRIGGER_GROUP] nvarchar(150) NOT NULL 
    );

    CREATE TABLE [dbo].[QRTZ_SCHEDULER_STATE] (
      [SCHED_NAME] nvarchar(120) NOT NULL,
      [INSTANCE_NAME] nvarchar(200) NOT NULL,
      [LAST_CHECKIN_TIME] bigint NOT NULL,
      [CHECKIN_INTERVAL] bigint NOT NULL
    );

    CREATE TABLE [dbo].[QRTZ_LOCKS] (
      [SCHED_NAME] nvarchar(120) NOT NULL,
      [LOCK_NAME] nvarchar(40) NOT NULL 
    );

    CREATE TABLE [dbo].[QRTZ_JOB_DETAILS] (
      [SCHED_NAME] nvarchar(120) NOT NULL,
      [JOB_NAME] nvarchar(150) NOT NULL,
      [JOB_GROUP] nvarchar(150) NOT NULL,
      [DESCRIPTION] nvarchar(250) NULL,
      [JOB_CLASS_NAME] nvarchar(250) NOT NULL,
      [IS_DURABLE] bit NOT NULL,
      [IS_NONCONCURRENT] bit NOT NULL,
      [IS_UPDATE_DATA] bit NOT NULL,
      [REQUESTS_RECOVERY] bit NOT NULL,
      [JOB_DATA] varbinary(max) NULL
    );

    CREATE TABLE [dbo].[QRTZ_SIMPLE_TRIGGERS] (
      [SCHED_NAME] nvarchar(120) NOT NULL,
      [TRIGGER_NAME] nvarchar(150) NOT NULL,
      [TRIGGER_GROUP] nvarchar(150) NOT NULL,
      [REPEAT_COUNT] int NOT NULL,
      [REPEAT_INTERVAL] bigint NOT NULL,
      [TIMES_TRIGGERED] int NOT NULL
    );

    CREATE TABLE [dbo].[QRTZ_SIMPROP_TRIGGERS] (
      [SCHED_NAME] nvarchar(120) NOT NULL,
      [TRIGGER_NAME] nvarchar(150) NOT NULL,
      [TRIGGER_GROUP] nvarchar(150) NOT NULL,
      [STR_PROP_1] nvarchar(512) NULL,
      [STR_PROP_2] nvarchar(512) NULL,
      [STR_PROP_3] nvarchar(512) NULL,
      [INT_PROP_1] int NULL,
      [INT_PROP_2] int NULL,
      [LONG_PROP_1] bigint NULL,
      [LONG_PROP_2] bigint NULL,
      [DEC_PROP_1] numeric(13,4) NULL,
      [DEC_PROP_2] numeric(13,4) NULL,
      [BOOL_PROP_1] bit NULL,
      [BOOL_PROP_2] bit NULL,
      [TIME_ZONE_ID] nvarchar(80) NULL 
    );

    CREATE TABLE [dbo].[QRTZ_BLOB_TRIGGERS] (
      [SCHED_NAME] nvarchar(120) NOT NULL,
      [TRIGGER_NAME] nvarchar(150) NOT NULL,
      [TRIGGER_GROUP] nvarchar(150) NOT NULL,
      [BLOB_DATA] varbinary(max) NULL
    );

    CREATE TABLE [dbo].[QRTZ_TRIGGERS] (
      [SCHED_NAME] nvarchar(120) NOT NULL,
      [TRIGGER_NAME] nvarchar(150) NOT NULL,
      [TRIGGER_GROUP] nvarchar(150) NOT NULL,
      [JOB_NAME] nvarchar(150) NOT NULL,
      [JOB_GROUP] nvarchar(150) NOT NULL,
      [DESCRIPTION] nvarchar(250) NULL,
      [NEXT_FIRE_TIME] bigint NULL,
      [PREV_FIRE_TIME] bigint NULL,
      [PRIORITY] int NULL,
      [TRIGGER_STATE] nvarchar(16) NOT NULL,
      [TRIGGER_TYPE] nvarchar(8) NOT NULL,
      [START_TIME] bigint NOT NULL,
      [END_TIME] bigint NULL,
      [CALENDAR_NAME] nvarchar(200) NULL,
      [MISFIRE_INSTR] int NULL,
      [JOB_DATA] varbinary(max) NULL
    );

    ALTER TABLE [dbo].[QRTZ_CALENDARS] WITH NOCHECK ADD
      CONSTRAINT [PK_QRTZ_CALENDARS] PRIMARY KEY  CLUSTERED
      (
        [SCHED_NAME],
        [CALENDAR_NAME]
      );

    ALTER TABLE [dbo].[QRTZ_CRON_TRIGGERS] WITH NOCHECK ADD
      CONSTRAINT [PK_QRTZ_CRON_TRIGGERS] PRIMARY KEY  CLUSTERED
      (
        [SCHED_NAME],
        [TRIGGER_NAME],
        [TRIGGER_GROUP]
      );

    ALTER TABLE [dbo].[QRTZ_FIRED_TRIGGERS] WITH NOCHECK ADD
      CONSTRAINT [PK_QRTZ_FIRED_TRIGGERS] PRIMARY KEY  CLUSTERED
      (
        [SCHED_NAME],
        [ENTRY_ID]
      );

    ALTER TABLE [dbo].[QRTZ_PAUSED_TRIGGER_GRPS] WITH NOCHECK ADD
      CONSTRAINT [PK_QRTZ_PAUSED_TRIGGER_GRPS] PRIMARY KEY  CLUSTERED
      (
        [SCHED_NAME],
        [TRIGGER_GROUP]
      );

    ALTER TABLE [dbo].[QRTZ_SCHEDULER_STATE] WITH NOCHECK ADD
      CONSTRAINT [PK_QRTZ_SCHEDULER_STATE] PRIMARY KEY  CLUSTERED
      (
        [SCHED_NAME],
        [INSTANCE_NAME]
      );

    ALTER TABLE [dbo].[QRTZ_LOCKS] WITH NOCHECK ADD
      CONSTRAINT [PK_QRTZ_LOCKS] PRIMARY KEY  CLUSTERED
      (
        [SCHED_NAME],
        [LOCK_NAME]
      );

    ALTER TABLE [dbo].[QRTZ_JOB_DETAILS] WITH NOCHECK ADD
      CONSTRAINT [PK_QRTZ_JOB_DETAILS] PRIMARY KEY  CLUSTERED
      (
        [SCHED_NAME],
        [JOB_NAME],
        [JOB_GROUP]
      );

    ALTER TABLE [dbo].[QRTZ_SIMPLE_TRIGGERS] WITH NOCHECK ADD
      CONSTRAINT [PK_QRTZ_SIMPLE_TRIGGERS] PRIMARY KEY  CLUSTERED
      (
        [SCHED_NAME],
        [TRIGGER_NAME],
        [TRIGGER_GROUP]
      );

    ALTER TABLE [dbo].[QRTZ_SIMPROP_TRIGGERS] WITH NOCHECK ADD
      CONSTRAINT [PK_QRTZ_SIMPROP_TRIGGERS] PRIMARY KEY  CLUSTERED
      (
        [SCHED_NAME],
        [TRIGGER_NAME],
        [TRIGGER_GROUP]
      );

    ALTER TABLE [dbo].[QRTZ_TRIGGERS] WITH NOCHECK ADD
      CONSTRAINT [PK_QRTZ_TRIGGERS] PRIMARY KEY  CLUSTERED
      (
        [SCHED_NAME],
        [TRIGGER_NAME],
        [TRIGGER_GROUP]
      );

    ALTER TABLE [dbo].[QRTZ_BLOB_TRIGGERS] WITH NOCHECK ADD
      CONSTRAINT [PK_QRTZ_BLOB_TRIGGERS] PRIMARY KEY  CLUSTERED
      (
        [SCHED_NAME],
        [TRIGGER_NAME],
        [TRIGGER_GROUP]
      );

    ALTER TABLE [dbo].[QRTZ_CRON_TRIGGERS] ADD
      CONSTRAINT [FK_QRTZ_CRON_TRIGGERS_QRTZ_TRIGGERS] FOREIGN KEY
      (
        [SCHED_NAME],
        [TRIGGER_NAME],
        [TRIGGER_GROUP]
      ) REFERENCES [dbo].[QRTZ_TRIGGERS] (
        [SCHED_NAME],
        [TRIGGER_NAME],
        [TRIGGER_GROUP]
      ) ON DELETE CASCADE;

    ALTER TABLE [dbo].[QRTZ_SIMPLE_TRIGGERS] ADD
      CONSTRAINT [FK_QRTZ_SIMPLE_TRIGGERS_QRTZ_TRIGGERS] FOREIGN KEY
      (
        [SCHED_NAME],
        [TRIGGER_NAME],
        [TRIGGER_GROUP]
      ) REFERENCES [dbo].[QRTZ_TRIGGERS] (
        [SCHED_NAME],
        [TRIGGER_NAME],
        [TRIGGER_GROUP]
      ) ON DELETE CASCADE;

    ALTER TABLE [dbo].[QRTZ_SIMPROP_TRIGGERS] ADD
      CONSTRAINT [FK_QRTZ_SIMPROP_TRIGGERS_QRTZ_TRIGGERS] FOREIGN KEY
      (
	    [SCHED_NAME],
        [TRIGGER_NAME],
        [TRIGGER_GROUP]
      ) REFERENCES [dbo].[QRTZ_TRIGGERS] (
        [SCHED_NAME],
        [TRIGGER_NAME],
        [TRIGGER_GROUP]
      ) ON DELETE CASCADE;

    ALTER TABLE [dbo].[QRTZ_TRIGGERS] ADD
      CONSTRAINT [FK_QRTZ_TRIGGERS_QRTZ_JOB_DETAILS] FOREIGN KEY
      (
        [SCHED_NAME],
        [JOB_NAME],
        [JOB_GROUP]
      ) REFERENCES [dbo].[QRTZ_JOB_DETAILS] (
        [SCHED_NAME],
        [JOB_NAME],
        [JOB_GROUP]
      );

    -- Create indexes
    CREATE INDEX [IDX_QRTZ_T_G_J]                 ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, JOB_GROUP, JOB_NAME);
    CREATE INDEX [IDX_QRTZ_T_C]                   ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, CALENDAR_NAME);

    CREATE INDEX [IDX_QRTZ_T_N_G_STATE]           ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, TRIGGER_GROUP, TRIGGER_STATE);
    CREATE INDEX [IDX_QRTZ_T_STATE]               ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, TRIGGER_STATE);
    CREATE INDEX [IDX_QRTZ_T_N_STATE]             ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, TRIGGER_NAME, TRIGGER_GROUP, TRIGGER_STATE);
    CREATE INDEX [IDX_QRTZ_T_NEXT_FIRE_TIME]      ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, NEXT_FIRE_TIME);
    CREATE INDEX [IDX_QRTZ_T_NFT_ST]              ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, TRIGGER_STATE, NEXT_FIRE_TIME);
    CREATE INDEX [IDX_QRTZ_T_NFT_ST_MISFIRE]      ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, MISFIRE_INSTR, NEXT_FIRE_TIME, TRIGGER_STATE);
    CREATE INDEX [IDX_QRTZ_T_NFT_ST_MISFIRE_GRP]  ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, MISFIRE_INSTR, NEXT_FIRE_TIME, TRIGGER_GROUP, TRIGGER_STATE);

    CREATE INDEX [IDX_QRTZ_FT_INST_JOB_REQ_RCVRY] ON [dbo].[QRTZ_FIRED_TRIGGERS](SCHED_NAME, INSTANCE_NAME, REQUESTS_RECOVERY);
    CREATE INDEX [IDX_QRTZ_FT_G_J]                ON [dbo].[QRTZ_FIRED_TRIGGERS](SCHED_NAME, JOB_GROUP, JOB_NAME);
    CREATE INDEX [IDX_QRTZ_FT_G_T]                ON [dbo].[QRTZ_FIRED_TRIGGERS](SCHED_NAME, TRIGGER_GROUP, TRIGGER_NAME);
END
        ";


        #endregion

        #region Postgresql Script

        private const string PostgreSQLScript = @"
        CREATE TABLE IF NOT EXISTS qrtz_job_details
          (
            sched_name TEXT NOT NULL,
	        job_name  TEXT NOT NULL,
            job_group TEXT NOT NULL,
            description TEXT NULL,
            job_class_name   TEXT NOT NULL, 
            is_durable BOOL NOT NULL,
            is_nonconcurrent BOOL NOT NULL,
            is_update_data BOOL NOT NULL,
	        requests_recovery BOOL NOT NULL,
            job_data BYTEA NULL,
            PRIMARY KEY (sched_name,job_name,job_group)
        );

        CREATE TABLE IF NOT EXISTS qrtz_triggers
          (
            sched_name TEXT NOT NULL,
	        trigger_name TEXT NOT NULL,
            trigger_group TEXT NOT NULL,
            job_name  TEXT NOT NULL, 
            job_group TEXT NOT NULL,
            description TEXT NULL,
            next_fire_time BIGINT NULL,
            prev_fire_time BIGINT NULL,
            priority INTEGER NULL,
            trigger_state TEXT NOT NULL,
            trigger_type TEXT NOT NULL,
            start_time BIGINT NOT NULL,
            end_time BIGINT NULL,
            calendar_name TEXT NULL,
            misfire_instr SMALLINT NULL,
            job_data BYTEA NULL,
            PRIMARY KEY (sched_name,trigger_name,trigger_group),
            FOREIGN KEY (sched_name,job_name,job_group) 
		        REFERENCES qrtz_job_details(sched_name,job_name,job_group) 
        );

        CREATE TABLE IF NOT EXISTS qrtz_simple_triggers
          (
            sched_name TEXT NOT NULL,
	        trigger_name TEXT NOT NULL,
            trigger_group TEXT NOT NULL,
            repeat_count BIGINT NOT NULL,
            repeat_interval BIGINT NOT NULL,
            times_triggered BIGINT NOT NULL,
            PRIMARY KEY (sched_name,trigger_name,trigger_group),
            FOREIGN KEY (sched_name,trigger_name,trigger_group) 
		        REFERENCES qrtz_triggers(sched_name,trigger_name,trigger_group) ON DELETE CASCADE
        );

        CREATE TABLE IF NOT EXISTS QRTZ_SIMPROP_TRIGGERS 
          (
            sched_name TEXT NOT NULL,
            trigger_name TEXT NOT NULL ,
            trigger_group TEXT NOT NULL ,
            str_prop_1 TEXT NULL,
            str_prop_2 TEXT NULL,
            str_prop_3 TEXT NULL,
            int_prop_1 INTEGER NULL,
            int_prop_2 INTEGER NULL,
            long_prop_1 BIGINT NULL,
            long_prop_2 BIGINT NULL,
            dec_prop_1 NUMERIC NULL,
            dec_prop_2 NUMERIC NULL,
            bool_prop_1 BOOL NULL,
            bool_prop_2 BOOL NULL,
	        time_zone_id TEXT NULL,
	        PRIMARY KEY (sched_name,trigger_name,trigger_group),
            FOREIGN KEY (sched_name,trigger_name,trigger_group) 
		        REFERENCES qrtz_triggers(sched_name,trigger_name,trigger_group) ON DELETE CASCADE
        );

        CREATE TABLE IF NOT EXISTS qrtz_cron_triggers
          (
            sched_name TEXT NOT NULL,
            trigger_name TEXT NOT NULL,
            trigger_group TEXT NOT NULL,
            cron_expression TEXT NOT NULL,
            time_zone_id TEXT,
            PRIMARY KEY (sched_name,trigger_name,trigger_group),
            FOREIGN KEY (sched_name,trigger_name,trigger_group) 
		        REFERENCES qrtz_triggers(sched_name,trigger_name,trigger_group) ON DELETE CASCADE
        );

        CREATE TABLE IF NOT EXISTS qrtz_blob_triggers
          (
            sched_name TEXT NOT NULL,
            trigger_name TEXT NOT NULL,
            trigger_group TEXT NOT NULL,
            blob_data BYTEA NULL,
            PRIMARY KEY (sched_name,trigger_name,trigger_group),
            FOREIGN KEY (sched_name,trigger_name,trigger_group) 
		        REFERENCES qrtz_triggers(sched_name,trigger_name,trigger_group) ON DELETE CASCADE
        );

        CREATE TABLE IF NOT EXISTS qrtz_calendars
          (
            sched_name TEXT NOT NULL,
            calendar_name  TEXT NOT NULL, 
            calendar BYTEA NOT NULL,
            PRIMARY KEY (sched_name,calendar_name)
        );

        CREATE TABLE IF NOT EXISTS qrtz_paused_trigger_grps
          (
            sched_name TEXT NOT NULL,
            trigger_group TEXT NOT NULL, 
            PRIMARY KEY (sched_name,trigger_group)
        );

        CREATE TABLE IF NOT EXISTS qrtz_fired_triggers 
          (
            sched_name TEXT NOT NULL,
            entry_id TEXT NOT NULL,
            trigger_name TEXT NOT NULL,
            trigger_group TEXT NOT NULL,
            instance_name TEXT NOT NULL,
            fired_time BIGINT NOT NULL,
	        sched_time BIGINT NOT NULL,
            priority INTEGER NOT NULL,
            state TEXT NOT NULL,
            job_name TEXT NULL,
            job_group TEXT NULL,
            is_nonconcurrent BOOL NOT NULL,
            requests_recovery BOOL NULL,
            PRIMARY KEY (sched_name,entry_id)
        );

        CREATE TABLE IF NOT EXISTS qrtz_scheduler_state 
          (
            sched_name TEXT NOT NULL,
            instance_name TEXT NOT NULL,
            last_checkin_time BIGINT NOT NULL,
            checkin_interval BIGINT NOT NULL,
            PRIMARY KEY (sched_name,instance_name)
        );

        CREATE TABLE IF NOT EXISTS qrtz_locks
          (
            sched_name TEXT NOT NULL,
            lock_name  TEXT NOT NULL, 
            PRIMARY KEY (sched_name,lock_name)
        );

        create index IF NOT EXISTS idx_qrtz_j_req_recovery on qrtz_job_details(requests_recovery);
        create index IF NOT EXISTS idx_qrtz_t_next_fire_time on qrtz_triggers(next_fire_time);
        create index IF NOT EXISTS idx_qrtz_t_state on qrtz_triggers(trigger_state);
        create index IF NOT EXISTS idx_qrtz_t_nft_st on qrtz_triggers(next_fire_time,trigger_state);
        create index IF NOT EXISTS idx_qrtz_ft_trig_name on qrtz_fired_triggers(trigger_name);
        create index IF NOT EXISTS idx_qrtz_ft_trig_group on qrtz_fired_triggers(trigger_group);
        create index IF NOT EXISTS idx_qrtz_ft_trig_nm_gp on qrtz_fired_triggers(sched_name,trigger_name,trigger_group);
        create index IF NOT EXISTS idx_qrtz_ft_trig_inst_name on qrtz_fired_triggers(instance_name);
        create index IF NOT EXISTS idx_qrtz_ft_job_name on qrtz_fired_triggers(job_name);
        create index IF NOT EXISTS idx_qrtz_ft_job_group on qrtz_fired_triggers(job_group);
        create index IF NOT EXISTS idx_qrtz_ft_job_req_recovery on qrtz_fired_triggers(requests_recovery);
        ";

        #endregion

        #region MySQL Script

        private const string MySQLScript = @"


            CREATE TABLE QRTZ_JOB_DETAILS(
            SCHED_NAME VARCHAR(120) NOT NULL,
            JOB_NAME VARCHAR(200) NOT NULL,
            JOB_GROUP VARCHAR(200) NOT NULL,
            DESCRIPTION VARCHAR(250) NULL,
            JOB_CLASS_NAME VARCHAR(250) NOT NULL,
            IS_DURABLE BOOLEAN NOT NULL,
            IS_NONCONCURRENT BOOLEAN NOT NULL,
            IS_UPDATE_DATA BOOLEAN NOT NULL,
            REQUESTS_RECOVERY BOOLEAN NOT NULL,
            JOB_DATA BLOB NULL,
            PRIMARY KEY (SCHED_NAME,JOB_NAME,JOB_GROUP))
            ENGINE=InnoDB;

            CREATE TABLE QRTZ_TRIGGERS (
            SCHED_NAME VARCHAR(120) NOT NULL,
            TRIGGER_NAME VARCHAR(200) NOT NULL,
            TRIGGER_GROUP VARCHAR(200) NOT NULL,
            JOB_NAME VARCHAR(200) NOT NULL,
            JOB_GROUP VARCHAR(200) NOT NULL,
            DESCRIPTION VARCHAR(250) NULL,
            NEXT_FIRE_TIME BIGINT(19) NULL,
            PREV_FIRE_TIME BIGINT(19) NULL,
            PRIORITY INTEGER NULL,
            TRIGGER_STATE VARCHAR(16) NOT NULL,
            TRIGGER_TYPE VARCHAR(8) NOT NULL,
            START_TIME BIGINT(19) NOT NULL,
            END_TIME BIGINT(19) NULL,
            CALENDAR_NAME VARCHAR(200) NULL,
            MISFIRE_INSTR SMALLINT(2) NULL,
            JOB_DATA BLOB NULL,
            PRIMARY KEY (SCHED_NAME,TRIGGER_NAME,TRIGGER_GROUP),
            FOREIGN KEY (SCHED_NAME,JOB_NAME,JOB_GROUP)
            REFERENCES QRTZ_JOB_DETAILS(SCHED_NAME,JOB_NAME,JOB_GROUP))
            ENGINE=InnoDB;

            CREATE TABLE QRTZ_SIMPLE_TRIGGERS (
            SCHED_NAME VARCHAR(120) NOT NULL,
            TRIGGER_NAME VARCHAR(200) NOT NULL,
            TRIGGER_GROUP VARCHAR(200) NOT NULL,
            REPEAT_COUNT BIGINT(7) NOT NULL,
            REPEAT_INTERVAL BIGINT(12) NOT NULL,
            TIMES_TRIGGERED BIGINT(10) NOT NULL,
            PRIMARY KEY (SCHED_NAME,TRIGGER_NAME,TRIGGER_GROUP),
            FOREIGN KEY (SCHED_NAME,TRIGGER_NAME,TRIGGER_GROUP)
            REFERENCES QRTZ_TRIGGERS(SCHED_NAME,TRIGGER_NAME,TRIGGER_GROUP))
            ENGINE=InnoDB;

            CREATE TABLE QRTZ_CRON_TRIGGERS (
            SCHED_NAME VARCHAR(120) NOT NULL,
            TRIGGER_NAME VARCHAR(200) NOT NULL,
            TRIGGER_GROUP VARCHAR(200) NOT NULL,
            CRON_EXPRESSION VARCHAR(120) NOT NULL,
            TIME_ZONE_ID VARCHAR(80),
            PRIMARY KEY (SCHED_NAME,TRIGGER_NAME,TRIGGER_GROUP),
            FOREIGN KEY (SCHED_NAME,TRIGGER_NAME,TRIGGER_GROUP)
            REFERENCES QRTZ_TRIGGERS(SCHED_NAME,TRIGGER_NAME,TRIGGER_GROUP))
            ENGINE=InnoDB;

            CREATE TABLE QRTZ_SIMPROP_TRIGGERS
              (          
                SCHED_NAME VARCHAR(120) NOT NULL,
                TRIGGER_NAME VARCHAR(200) NOT NULL,
                TRIGGER_GROUP VARCHAR(200) NOT NULL,
                STR_PROP_1 VARCHAR(512) NULL,
                STR_PROP_2 VARCHAR(512) NULL,
                STR_PROP_3 VARCHAR(512) NULL,
                INT_PROP_1 INT NULL,
                INT_PROP_2 INT NULL,
                LONG_PROP_1 BIGINT NULL,
                LONG_PROP_2 BIGINT NULL,
                DEC_PROP_1 NUMERIC(13,4) NULL,
                DEC_PROP_2 NUMERIC(13,4) NULL,
                BOOL_PROP_1 BOOLEAN NULL,
                BOOL_PROP_2 BOOLEAN NULL,
                TIME_ZONE_ID VARCHAR(80) NULL,
                PRIMARY KEY (SCHED_NAME,TRIGGER_NAME,TRIGGER_GROUP),
                FOREIGN KEY (SCHED_NAME,TRIGGER_NAME,TRIGGER_GROUP) 
                REFERENCES QRTZ_TRIGGERS(SCHED_NAME,TRIGGER_NAME,TRIGGER_GROUP))
            ENGINE=InnoDB;

            CREATE TABLE QRTZ_BLOB_TRIGGERS (
            SCHED_NAME VARCHAR(120) NOT NULL,
            TRIGGER_NAME VARCHAR(200) NOT NULL,
            TRIGGER_GROUP VARCHAR(200) NOT NULL,
            BLOB_DATA BLOB NULL,
            PRIMARY KEY (SCHED_NAME,TRIGGER_NAME,TRIGGER_GROUP),
            INDEX (SCHED_NAME,TRIGGER_NAME, TRIGGER_GROUP),
            FOREIGN KEY (SCHED_NAME,TRIGGER_NAME,TRIGGER_GROUP)
            REFERENCES QRTZ_TRIGGERS(SCHED_NAME,TRIGGER_NAME,TRIGGER_GROUP))
            ENGINE=InnoDB;

            CREATE TABLE QRTZ_CALENDARS (
            SCHED_NAME VARCHAR(120) NOT NULL,
            CALENDAR_NAME VARCHAR(200) NOT NULL,
            CALENDAR BLOB NOT NULL,
            PRIMARY KEY (SCHED_NAME,CALENDAR_NAME))
            ENGINE=InnoDB;

            CREATE TABLE QRTZ_PAUSED_TRIGGER_GRPS (
            SCHED_NAME VARCHAR(120) NOT NULL,
            TRIGGER_GROUP VARCHAR(200) NOT NULL,
            PRIMARY KEY (SCHED_NAME,TRIGGER_GROUP))
            ENGINE=InnoDB;

            CREATE TABLE QRTZ_FIRED_TRIGGERS (
            SCHED_NAME VARCHAR(120) NOT NULL,
            ENTRY_ID VARCHAR(140) NOT NULL,
            TRIGGER_NAME VARCHAR(200) NOT NULL,
            TRIGGER_GROUP VARCHAR(200) NOT NULL,
            INSTANCE_NAME VARCHAR(200) NOT NULL,
            FIRED_TIME BIGINT(19) NOT NULL,
            SCHED_TIME BIGINT(19) NOT NULL,
            PRIORITY INTEGER NOT NULL,
            STATE VARCHAR(16) NOT NULL,
            JOB_NAME VARCHAR(200) NULL,
            JOB_GROUP VARCHAR(200) NULL,
            IS_NONCONCURRENT BOOLEAN NULL,
            REQUESTS_RECOVERY BOOLEAN NULL,
            PRIMARY KEY (SCHED_NAME,ENTRY_ID))
            ENGINE=InnoDB;

            CREATE TABLE QRTZ_SCHEDULER_STATE (
            SCHED_NAME VARCHAR(120) NOT NULL,
            INSTANCE_NAME VARCHAR(200) NOT NULL,
            LAST_CHECKIN_TIME BIGINT(19) NOT NULL,
            CHECKIN_INTERVAL BIGINT(19) NOT NULL,
            PRIMARY KEY (SCHED_NAME,INSTANCE_NAME))
            ENGINE=InnoDB;

            CREATE TABLE QRTZ_LOCKS (
            SCHED_NAME VARCHAR(120) NOT NULL,
            LOCK_NAME VARCHAR(40) NOT NULL,
            PRIMARY KEY (SCHED_NAME,LOCK_NAME))
            ENGINE=InnoDB;

            CREATE INDEX IDX_QRTZ_J_REQ_RECOVERY ON QRTZ_JOB_DETAILS(SCHED_NAME,REQUESTS_RECOVERY);
            CREATE INDEX IDX_QRTZ_J_GRP ON QRTZ_JOB_DETAILS(SCHED_NAME,JOB_GROUP);

            CREATE INDEX IDX_QRTZ_T_J ON QRTZ_TRIGGERS(SCHED_NAME,JOB_NAME,JOB_GROUP);
            CREATE INDEX IDX_QRTZ_T_JG ON QRTZ_TRIGGERS(SCHED_NAME,JOB_GROUP);
            CREATE INDEX IDX_QRTZ_T_C ON QRTZ_TRIGGERS(SCHED_NAME,CALENDAR_NAME);
            CREATE INDEX IDX_QRTZ_T_G ON QRTZ_TRIGGERS(SCHED_NAME,TRIGGER_GROUP);
            CREATE INDEX IDX_QRTZ_T_STATE ON QRTZ_TRIGGERS(SCHED_NAME,TRIGGER_STATE);
            CREATE INDEX IDX_QRTZ_T_N_STATE ON QRTZ_TRIGGERS(SCHED_NAME,TRIGGER_NAME,TRIGGER_GROUP,TRIGGER_STATE);
            CREATE INDEX IDX_QRTZ_T_N_G_STATE ON QRTZ_TRIGGERS(SCHED_NAME,TRIGGER_GROUP,TRIGGER_STATE);
            CREATE INDEX IDX_QRTZ_T_NEXT_FIRE_TIME ON QRTZ_TRIGGERS(SCHED_NAME,NEXT_FIRE_TIME);
            CREATE INDEX IDX_QRTZ_T_NFT_ST ON QRTZ_TRIGGERS(SCHED_NAME,TRIGGER_STATE,NEXT_FIRE_TIME);
            CREATE INDEX IDX_QRTZ_T_NFT_MISFIRE ON QRTZ_TRIGGERS(SCHED_NAME,MISFIRE_INSTR,NEXT_FIRE_TIME);
            CREATE INDEX IDX_QRTZ_T_NFT_ST_MISFIRE ON QRTZ_TRIGGERS(SCHED_NAME,MISFIRE_INSTR,NEXT_FIRE_TIME,TRIGGER_STATE);
            CREATE INDEX IDX_QRTZ_T_NFT_ST_MISFIRE_GRP ON QRTZ_TRIGGERS(SCHED_NAME,MISFIRE_INSTR,NEXT_FIRE_TIME,TRIGGER_GROUP,TRIGGER_STATE);

            CREATE INDEX IDX_QRTZ_FT_TRIG_INST_NAME ON QRTZ_FIRED_TRIGGERS(SCHED_NAME,INSTANCE_NAME);
            CREATE INDEX IDX_QRTZ_FT_INST_JOB_REQ_RCVRY ON QRTZ_FIRED_TRIGGERS(SCHED_NAME,INSTANCE_NAME,REQUESTS_RECOVERY);
            CREATE INDEX IDX_QRTZ_FT_J_G ON QRTZ_FIRED_TRIGGERS(SCHED_NAME,JOB_NAME,JOB_GROUP);
            CREATE INDEX IDX_QRTZ_FT_JG ON QRTZ_FIRED_TRIGGERS(SCHED_NAME,JOB_GROUP);
            CREATE INDEX IDX_QRTZ_FT_T_G ON QRTZ_FIRED_TRIGGERS(SCHED_NAME,TRIGGER_NAME,TRIGGER_GROUP);
            CREATE INDEX IDX_QRTZ_FT_TG ON QRTZ_FIRED_TRIGGERS(SCHED_NAME,TRIGGER_GROUP);

";

        #endregion
    }


}
