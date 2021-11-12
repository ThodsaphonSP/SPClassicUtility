using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;

namespace Flogging.Core
{
    static public class Flogger
    {
        private static readonly ILogger _perfLogger;
        private static readonly ILogger _usageLogger;
        private static readonly ILogger _errorLogger;
        private static readonly ILogger _diagnosticLogger;

        static Flogger()
        {

            var connectionString = ConfigurationManager.AppSettings["logConnectionString"];

            int batchPostingLimit = Convert.ToInt32(ConfigurationManager.AppSettings["batchPostingLimit"]);



           var performLogger = new LoggerConfiguration();
                // .WriteTo.File(path: "C:\\repo\\edahl\\Source\\perf.txt");
              

           var tempUsageLogger = new LoggerConfiguration();
               // .WriteTo.File(path: "C:\\repo\\edahl\\Source\\usage.txt");


           var tempErrorLogger = new LoggerConfiguration();
               // .WriteTo.File(path: "C:\\repo\\edahl\\Source\\error.txt");


           var tempDiagnostic = new LoggerConfiguration();
                // .WriteTo.File(path: "C:\\repo\\edahl\\Source\\diagnostic.txt");





            if (!string.IsNullOrEmpty(connectionString))
            {
                performLogger.WriteTo.MSSqlServer(connectionString, "performs", autoCreateSqlTable: true,
                    columnOptions: GetSqlColumnOptions(), batchPostingLimit: batchPostingLimit);
            }
            
            if (!string.IsNullOrEmpty(connectionString))
            {
                tempUsageLogger.WriteTo.MSSqlServer(connectionString, "Usage", autoCreateSqlTable: true,
                    columnOptions: GetSqlColumnOptions(), batchPostingLimit: batchPostingLimit);
            }


            if (!string.IsNullOrEmpty(connectionString))
            {
                tempErrorLogger.WriteTo.MSSqlServer(connectionString, "error", autoCreateSqlTable: true,
                    columnOptions: GetSqlColumnOptions(), batchPostingLimit: batchPostingLimit);
            }




            if (!string.IsNullOrEmpty(connectionString))
            {
                tempDiagnostic.WriteTo.MSSqlServer(connectionString, "DiagnosticLogs", autoCreateSqlTable: true,
                    columnOptions: GetSqlColumnOptions(), batchPostingLimit: batchPostingLimit);
            }

            _perfLogger = performLogger.CreateLogger();

            _usageLogger = tempUsageLogger.CreateLogger();

            _errorLogger = tempErrorLogger.CreateLogger();

            Flogger._diagnosticLogger = tempDiagnostic.CreateLogger();
        }


        public static ColumnOptions GetSqlColumnOptions()
        {
            var colOptions = new ColumnOptions();
            colOptions.Store.Remove(StandardColumn.Properties);
            colOptions.Store.Remove(StandardColumn.MessageTemplate);
            colOptions.Store.Remove(StandardColumn.Message);
            colOptions.Store.Remove(StandardColumn.Exception);
            colOptions.Store.Remove(StandardColumn.TimeStamp);
            colOptions.Store.Remove(StandardColumn.Level);

            colOptions.AdditionalDataColumns = new Collection<DataColumn>
            {
                new DataColumn {DataType = typeof(DateTime), ColumnName = "Timestamp"},
                new DataColumn {DataType = typeof(string), ColumnName = "Product"},
                new DataColumn {DataType = typeof(string), ColumnName = "Layer"},
                new DataColumn {DataType = typeof(string), ColumnName = "Location"},
                new DataColumn {DataType = typeof(string), ColumnName = "Message"},
                new DataColumn {DataType = typeof(string), ColumnName = "Hostname"},
                new DataColumn {DataType = typeof(string), ColumnName = "UserId"},
                new DataColumn {DataType = typeof(string), ColumnName = "UserName"},
                new DataColumn {DataType = typeof(string), ColumnName = "Exception"},
                new DataColumn {DataType = typeof(int), ColumnName = "ElapsedMilliseconds"},
                new DataColumn {DataType = typeof(string), ColumnName = "CorrelationId"},
                new DataColumn {DataType = typeof(string), ColumnName = "CustomException"},
                new DataColumn {DataType = typeof(string), ColumnName = "AdditionalInfo"},
            };

            return colOptions;
        }


        public static void WritePerf(FlogDetail infoToLog)
        {
            // _perfLogger.Write(LogEventLevel.Information, "{@FlogDetail}", infoToLog);

            _perfLogger.Write(LogEventLevel.Information,
                "{Timestamp}{Message}{Layer}{Location}{Product}" +
                "{CustomException}{ElapsedMilliseconds}{Exception}{Hostname}" +
                "{UserId}{UserName}{CorrelationId}{AdditionalInfo}",
                infoToLog.Timestamp, infoToLog.Message,
                infoToLog.Layer, infoToLog.Location,
                infoToLog.Product, infoToLog.CustomException,
                infoToLog.ElapsedMilliseconds, infoToLog.Exception?.ToBetterString(),
                infoToLog.Hostname, infoToLog.UserId,
                infoToLog.UserName, infoToLog.CorrelationId,
                infoToLog.AdditionalInfo
            );
        }
        public static void WriteUsage(FlogDetail infoToLog)
        {
            // _usageLogger.Write(LogEventLevel.Information, "{@FlogDetail}", infoToLog);

            _usageLogger.Write(LogEventLevel.Information,
                "{Timestamp}{Message}{Layer}{Location}{Product}" +
                "{CustomException}{ElapsedMilliseconds}{Exception}{Hostname}" +
                "{UserId}{UserName}{CorrelationId}{AdditionalInfo}",
                infoToLog.Timestamp, infoToLog.Message,
                infoToLog.Layer, infoToLog.Location,
                infoToLog.Product, infoToLog.CustomException,
                infoToLog.ElapsedMilliseconds, infoToLog.Exception?.ToBetterString(),
                infoToLog.Hostname, infoToLog.UserId,
                infoToLog.UserName, infoToLog.CorrelationId,
                infoToLog.AdditionalInfo
            );
        }
        public static void WriteError(FlogDetail infoToLog)
        {
            if (infoToLog.Exception != null)
            {
                var procName = FindProcName(infoToLog.Exception);
                infoToLog.Location = string.IsNullOrEmpty(procName) ? infoToLog.Location : procName;
                infoToLog.Message = GetMessageFromException(infoToLog.Exception);
            }



            // _errorLogger.Write(LogEventLevel.Information, "{@FlogDetail}", infoToLog);

            _errorLogger.Write(LogEventLevel.Information,
                "{Timestamp}{Message}{Layer}{Location}{Product}" +
                "{CustomException}{ElapsedMilliseconds}{Exception}{Hostname}" +
                "{UserId}{UserName}{CorrelationId}{AdditionalInfo}",
                infoToLog.Timestamp, infoToLog.Message,
                infoToLog.Layer, infoToLog.Location,
                infoToLog.Product, infoToLog.CustomException,
                infoToLog.ElapsedMilliseconds, infoToLog.Exception?.ToBetterString(),
                infoToLog.Hostname, infoToLog.UserId,
                infoToLog.UserName, infoToLog.CorrelationId,
                infoToLog.AdditionalInfo
            );
        }
        public static void WriteDiagnostic(FlogDetail infoToLog)
        {
            var writeDiagnostics = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableDiagnostics"]);
            if (!writeDiagnostics)
                return;

            _diagnosticLogger.Write(LogEventLevel.Information,
                "{Timestamp}{Message}{Layer}{Location}{Product}" +
                "{CustomException}{ElapsedMilliseconds}{Exception}{Hostname}" +
                "{UserId}{UserName}{CorrelationId}{AdditionalInfo}",
                infoToLog.Timestamp, infoToLog.Message,
                infoToLog.Layer, infoToLog.Location,
                infoToLog.Product, infoToLog.CustomException,
                infoToLog.ElapsedMilliseconds, infoToLog.Exception?.ToBetterString(),
                infoToLog.Hostname, infoToLog.UserId,
                infoToLog.UserName, infoToLog.CorrelationId,
                infoToLog.AdditionalInfo
            );

            // _diagnosticLogger.Write(LogEventLevel.Information, "{@FlogDetail}", infoToLog);
        }



        private static string FindProcName(Exception ex)
        {
            var sqlEx = ex as SqlException;
            if (sqlEx != null)
            {
                var procName = sqlEx.Procedure;
                if (!string.IsNullOrEmpty(procName))
                    return procName;
            }

            if (!string.IsNullOrEmpty((string)ex.Data["Procedure"]))
            {
                return (string)ex.Data["Procedure"];
            }

            if (ex.InnerException != null)
                return FindProcName(ex.InnerException);

            return null;
        }

        private static string GetMessageFromException(Exception ex)
        {
            if (ex.InnerException != null)
                return GetMessageFromException(ex.InnerException);

            return ex.Message;
        }
    }



}