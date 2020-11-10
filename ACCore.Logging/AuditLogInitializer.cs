using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;

namespace ACCore.Logging
{
    public class AuditLogInitializer
    {
        const string _connectionString = @"Server=xx;Database=xx;Trusted_Connection=True;";
        const string _tableName = "Logs";

        public static LoggerConfiguration CreateLoggerConfiguration()
        {
            return new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.MSSqlServer(
                    _connectionString,
                    new MSSqlServerSinkOptions
                    {
                        TableName = _tableName,
                        AutoCreateSqlTable = true
                    },
                    sinkOptionsSection: null,
                    appConfiguration: null,
                    restrictedToMinimumLevel: LevelAlias.Minimum,
                    formatProvider: null,
                    columnOptions: BuildColumnOptions(),
                    columnOptionsSection: null,
                    logEventFormatter: null);
        }

        private static ColumnOptions BuildColumnOptions()
        {
            var columnOptions = new ColumnOptions
            {
                AdditionalColumns = new Collection<SqlColumn>
                {
                    new SqlColumn { DataType = SqlDbType.NVarChar, ColumnName = nameof(AuditLog.EnvDomainName) },
                    new SqlColumn { DataType = SqlDbType.NVarChar, ColumnName = nameof(AuditLog.EnvMachineName) },
                    new SqlColumn { DataType = SqlDbType.NVarChar, ColumnName = nameof(AuditLog.EnvUsername) },

                    new SqlColumn { DataType = SqlDbType.NVarChar, ColumnName = nameof(AuditLog.UserId) },
                    new SqlColumn { DataType = SqlDbType.NVarChar, ColumnName = nameof(AuditLog.UserIpAddress) },
                    new SqlColumn { DataType = SqlDbType.NVarChar, ColumnName = nameof(AuditLog.Username) },

                    new SqlColumn { DataType = SqlDbType.NVarChar, ColumnName = nameof(AuditLog.Action) },
                    new SqlColumn { DataType = SqlDbType.NVarChar, ColumnName = nameof(AuditLog.Table) },
                    new SqlColumn { DataType = SqlDbType.NVarChar, ColumnName = nameof(AuditLog.PrimaryId) },
                    new SqlColumn { DataType = SqlDbType.NVarChar, ColumnName = nameof(AuditLog.OriginalValues) },
                    new SqlColumn { DataType = SqlDbType.NVarChar, ColumnName = nameof(AuditLog.CurrentValues) },
                }
            };

            columnOptions.Store.Remove(StandardColumn.Message);
            columnOptions.Store.Remove(StandardColumn.MessageTemplate);
            columnOptions.Store.Remove(StandardColumn.Properties);

            return columnOptions;
        }

        public static void TrackDbContext<TEntity>(DbContext dbContext)
            where TEntity : class, IEntityId<Guid>
        {
            Log.Logger = CreateLoggerConfiguration().CreateLogger();

            foreach (var currentEntity in dbContext.ChangeTracker.Entries<TEntity>())
            {
                var dict = new Dictionary<string, string>();

                dict.Add($"{{{nameof(AuditLog.Table)}}}", currentEntity.Metadata.FindAnnotation("Relational:TableName").Value.ToString());
                dict.Add($"{{{nameof(AuditLog.PrimaryId)}}}", currentEntity.Entity.Id.ToString());
                dict.Add($"{{{nameof(AuditLog.Action)}}}", currentEntity.State.ToString());

                switch (currentEntity.State)
                {
                    case EntityState.Added:
                    case EntityState.Deleted:
                        dict.Add($"{{{nameof(AuditLog.OriginalValues)}}}", null);
                        dict.Add($"{{{nameof(AuditLog.CurrentValues)}}}", null);
                        break;

                    case EntityState.Modified:
                        dict.Add($"{{{nameof(AuditLog.OriginalValues)}}}", ConvertToJson(dbContext.Entry(currentEntity.Entity).OriginalValues.ToObject()));
                        dict.Add($"{{{nameof(AuditLog.CurrentValues)}}}", ConvertToJson(dbContext.Entry(currentEntity.Entity).CurrentValues.ToObject()));
                        break;
                }

                var messageTemplate = string.Join("", dict.OrderBy(t => t.Key).Select(t => t.Key));
                var messageValues = dict.OrderBy(t => t.Key).Select(t => (object)t.Value).ToArray();

                Log.Information(messageTemplate, messageValues);
            }

            Log.CloseAndFlush();
        }

        private static string ConvertToJson(object val)
        {
            return JsonConvert.SerializeObject(val, JsonSerializerSettings());
        }

        private static JsonSerializerSettings JsonSerializerSettings()
        {
            return new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
        }
    }
}
