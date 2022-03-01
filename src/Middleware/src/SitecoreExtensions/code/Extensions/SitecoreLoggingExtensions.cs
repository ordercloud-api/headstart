namespace Sitecore.Diagnostics
{
    using Sitecore.Foundation.SitecoreExtensions.Extensions;

    public static class LogExt
    {
        /// <summary>
        /// Common re-usable LogException method used for logging exception sepertated log errors throughout the entire application into Sitecore Log files
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="message"></param>
        /// <param name="tryCatchMessage"></param>
        /// <param name="errorTraceCert"></param>
        /// <param name="obj"></param>
        public static void LogException(string appLogFileKey, string methodName, string message, string tryCatchMessage, string errorTraceCert, object obj, bool createCustomLogFile = false)
        {
            message = (message.Contains("[methodName]")) ? message.Replace("[methodName]", methodName) : message;
            if (createCustomLogFile)
            {
                LoggingNotifications.WriteToCustomLogFile(appLogFileKey, methodName, message, true, tryCatchMessage, errorTraceCert);
            }
            else
            {
                string exception = LoggingNotifications.GenerateLogExeptionMessage(methodName, string.Empty, tryCatchMessage, errorTraceCert);
                Log.Error($@"{message} - {LoggingNotifications.GetExceptionMessagePrefixKey()}: {exception}.", obj);
            }            
        }

        /// <summary>
        /// Common re-usable LogApiResponseMessages method used for api response message logging throughout the entire application
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="message"></param>
        /// <param name="messageKeyValue"></param>
        public static void LogApiResponseMessages(string methodName, string message, string messageKeyValue, bool isError = false)
        {
            if (!string.IsNullOrEmpty(message))
            {
                messageKeyValue = (!string.IsNullOrEmpty(messageKeyValue)) ? messageKeyValue : LoggingNotifications.GetGeneralLogMessagePrefixKey();
                message = LoggingNotifications.GetLogExeptionMessage(methodName, $@"{messageKeyValue}: {message}", string.Empty, string.Empty, LoggingNotifications.GetApiResponseMessagePrefixKey());
                if (isError)
                {
                    Log.Error(message, new LoggingOwnerObject());
                }
                else
                {
                    Log.Info(message, new LoggingOwnerObject());
                }
            }
        }
    }
}

namespace Sitecore.Foundation.SitecoreExtensions.Extensions
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Collections.Generic;

    public class LoggingOwnerObject
    {
        public LoggingOwnerObject() { }
    }

    public static class LoggingNotifications
    {
        private static ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();

        /// <summary>
        /// Common re-usable GetGeneralLogMessagePrefixKey method used for returning the GeneralLogMessagePrefixKey value
        /// </summary>
        /// <returns>The GeneralLogMessagePrefixKey string value</returns>
        public static string GetGeneralLogMessagePrefixKey()
        {
            return @"General Log Message";
        }

        /// <summary>
        /// Common re-usable GetApiResponseMessagePrefixKey method used for returning the ApiResponseMessagePrefixKey value
        /// </summary>
        /// <returns>The ApiResponseMessagePrefixKey string value</returns>
        public static string GetApiResponseMessagePrefixKey()
        {
            return @"Api Response Message";
        }

        /// <summary>
        /// Common re-usable GetExceptionMessagePrefixKey method used for returning the ExceptionMessagePrefixKey value
        /// </summary>
        /// <returns>The ExceptionMessagePrefixKey string value</returns>
        public static string GetExceptionMessagePrefixKey()
        {
            return @"Exception Message";
        }

        /// <summary>
        /// Common re-usable GetNoMethodIdPassedInPrefixKey method used for returning the NoMethodIdPassedInPrefixKey value
        /// </summary>
        /// <returns>The NoMethodIdPassedInPrefixKey string value</returns>
        public static string GetNoMethodIdPassedInPrefixKey()
        {
            return @"NoMethodIdPassedIn";
        }

        /// <summary>
        /// Common re-usable GenerateLogExeptionMessage method used for returning exception sepertated log messages throughout the entire application
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="message"></param>
        /// <param name="tryCatchMessage"></param>
        /// <param name="errorTraceCert"></param>
        /// <returns>The generated and formatted LogExeptionMessage string value</returns>
        public static string GenerateLogExeptionMessage(string methodName, string message, string tryCatchMessage, string errorTraceCert)
        {
            return GetLogExeptionMessage(methodName, message, tryCatchMessage, errorTraceCert, string.Empty);
        }

        /// <summary>
        /// Common re-usable LogExeptionNotification method used for exception logging throughout the entire application
        /// </summary>
        /// <param name="appLogFileKey"></param>
        /// <param name="methodName"></param>
        /// <param name="message"></param>
        /// <param name="tryCatchMessage"></param>
        /// <param name="errorTraceCert"></param>
        /// <param name="notificationTypePrefix"></param>
        public static void LogExeptionNotification(string appLogFileKey, string methodName, string message, string tryCatchMessage, string errorTraceCert, string notificationTypePrefix)
        {
            LogNotificationMessages(appLogFileKey, methodName, message, true, tryCatchMessage, errorTraceCert, notificationTypePrefix);
        }

        /// <summary>
        /// Common re-usable LogNotificationMessages method used for notification message logging throughout the entire application
        /// </summary>
        /// <param name="appLogFileKey"></param>
        /// <param name="methodName"></param>
        /// <param name="message"></param>
        /// <param name="isExeptionMessage"></param>
        /// <param name="tryCatchMessage"></param>
        /// <param name="errorTraceCert"></param>
        /// <param name="notificationTypePrefix"></param>
        public static void LogNotificationMessages(string appLogFileKey, string methodName, string message, bool isExeptionMessage, string tryCatchMessage, string errorTraceCert, string notificationTypePrefix)
        {
            WriteToCustomLogFile(appLogFileKey, methodName, message, false);
        }

        /// <summary>
        /// Common re-usable LogApiResponseMessages method used for api response message logging throughout the entire application
        /// </summary>
        /// <param name="appLogFileKey"></param>
        /// <param name="methodName"></param>
        /// <param name="message"></param>
        /// <param name="messageKeyValue"></param>
        public static void LogApiResponseMessages(string appLogFileKey, string methodName, string message, string messageKeyValue, bool isError = false, string tryCatchMessage = "", string errorTraceCert = "")
        {
            if (!string.IsNullOrEmpty(message))
            {
                messageKeyValue = (!string.IsNullOrEmpty(messageKeyValue)) ? messageKeyValue : GetGeneralLogMessagePrefixKey();
                message = GetLogExeptionMessage(methodName, $@"{messageKeyValue}: {message}", string.Empty, string.Empty, GetApiResponseMessagePrefixKey());
                if (isError)
                {
                    WriteToCustomLogFile(appLogFileKey, methodName, message, true, tryCatchMessage, errorTraceCert);
                }
                else
                {
                    WriteToCustomLogFile(appLogFileKey, methodName, message, false);
                }
            }
        }

        /// <summary>
        /// Common re-usable for building formatting wrappers message logging for the 'LogExeptionNotification'; 'LogNotificationMessages' and 'LogApiResponseMessages' methods above
        /// to make the logging chucks easily readable in the log file/s
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="message"></param>
        /// <param name="tryCatchMessage"></param>
        /// <param name="errorTraceCert"></param>
        /// <param name="notificationTypePrefix"></param>
        /// <returns>The generated and formatted LogExeptionMessage string value</returns>
        public static string GetLogExeptionMessage(string methodName, string message, string tryCatchMessage, string errorTraceCert, string notificationTypePrefix)
        {
            string _Notification = string.Empty;
            notificationTypePrefix = (string.IsNullOrEmpty(notificationTypePrefix)) ? GetGeneralLogMessagePrefixKey() : notificationTypePrefix;
            methodName = (string.IsNullOrEmpty(methodName)) ? GetNoMethodIdPassedInPrefixKey() : methodName;

            StringBuilder sb = new StringBuilder();
            string dateStamp = DateTime.Now.ToString();
            sb.Append($@"------------------------------{methodName}:{dateStamp}------------------------------" + Environment.NewLine);
            string logMessage = string.Empty;

            if (!string.IsNullOrEmpty(message))
            {
                logMessage = $@"{methodName.Trim()} - {notificationTypePrefix}: {message.Trim()}.";
            }
            if ((!string.IsNullOrEmpty(tryCatchMessage)) || ((!string.IsNullOrEmpty(errorTraceCert))))
            {
                logMessage = $@"{methodName.Trim()} - {notificationTypePrefix}: {message.Trim()}. {GetExceptionMessagePrefixKey()}: {tryCatchMessage} - {errorTraceCert}.";
            }

            sb.Append($@"{logMessage}" + Environment.NewLine);
            sb.Append($@"------------------------------{methodName}:{dateStamp}------------------------------" + Environment.NewLine);
            _Notification = sb.ToString().Trim();

            return _Notification;
        }

        /// <summary>
        /// Common re-usable GetHtmlNotificationForView method to return server error messages with HTML wrapper 
        /// </summary>
        /// <param name="notificationMessage"></param>
        /// <param name="alertType"></param>
        /// <returns>The NotificationAlerts message as a HtmlString wrapped value</returns>
        public static string GetHtmlNotificationForView(string notificationMessage, string alertType)
        {
            return ServerSideNotificationWrappers.NotificationAlerts(notificationMessage, alertType);
        }

        /// <summary>
        /// Common re-usable WriteToCustomLogFile method to create log file/s (1 per day) and logging message/s daily:
        /// Thread-safe code that Locks the file while it is being written to and Ques other thread requests until file is released
        /// </summary>
        /// <param name="logMessage"></param>
        public static void WriteToCustomLogFile(string AppLogFileKey, string methodName, string logMessage, bool isExeptionMessage, string tryCatchMessage = "", string errorTraceCert = "", bool isInfoMessage = false)
        {
            _readWriteLock.EnterWriteLock();
            try
            {
                StringBuilder sb = new StringBuilder();
                string dateTimeStamp = DateTime.Now.ToString("MM_dd_yyyy-HH_mm_ss");
                string dateStamp = DateTime.Now.ToString("MM_dd_yyyy");
                string logsDirectory = System.Web.HttpContext.Current.Server.MapPath(@"~/App_Data/logs");

                sb.Append($@"------------------------------{methodName}:{dateTimeStamp}:{GetMessageTypeKey(isExeptionMessage, isInfoMessage)}------------------------------" + Environment.NewLine);
                sb.Append($@"{logMessage} {tryCatchMessage}. {errorTraceCert}." + Environment.NewLine);
                sb.Append($@"------------------------------{methodName}:{dateTimeStamp}:{GetMessageTypeKey(isExeptionMessage, isInfoMessage)}------------------------------" + Environment.NewLine);

                if (Directory.Exists(logsDirectory))
                {
                    string filePath = $@"{logsDirectory}\{AppLogFileKey}ApiApplictionNotifictions_{dateStamp}.txt";

                    // This text is always added, making the file longer over time
                    // if it is not deleted.
                    using (StreamWriter sw = File.AppendText(filePath))
                    {
                        sw.WriteLine(sb.ToString().Trim());
                        sw.Close();
                    }
                }
            }
            finally
            {
                _readWriteLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Common re-usable GetMessageTypeKey() method
        /// </summary>
        /// <param name="isExeptionMessage"></param>
        /// <param name="isInfoMessage"></param>
        /// <returns>The MessageTypeKey string value</returns>
        private static string GetMessageTypeKey(bool isExeptionMessage, bool isInfoMessage = false)
        {
            string messageType = string.Empty;
            if (isExeptionMessage)
            {
                messageType = "ExeptionMessage";
            }
            else
            {
                messageType = "InfoMessage";
            }
            return messageType;
        }
    }

    public static class ServerSideNotificationWrappers
    {
        /// <summary>
        /// Common re-usable NotificationAlerts method used for dynamically create an HTML div wrapper for notifications styled message arround server message 
        /// needed to be returned to the user - All this types and HTML div wrapper are as per Bootstrap resposive-design standards
        /// </summary>
        /// <param name="notificationMessage"></param>
        /// <param name="alertType"></param>
        /// <returns>The NotificationAlerts message as a HtmlString wrapped value</returns>
        public static string NotificationAlerts(string notificationMessage, string alertType)
        {
            string wrapperNotify = GetAlertType(notificationMessage, alertType);
            string notificationWrapperId = $@"<div id='notify-message-wrapper'>{wrapperNotify}<div>";
            return notificationWrapperId;
        }

        /// <summary>
        /// Common re-usable GetAlertType method to apply inner HTML repsonse styled wrappers for notifications styled message arround server message needed to be returned to the user
        /// (i.e. info; success; danger/error; warning; certificate; question styled messages) - All this types and HTML div wrapper are as per Bootstrap resposive-design standards
        /// </summary>
        /// <param name="alertType"></param>
        /// <returns>The GetAlertType string as a HtmlString wrapped value</returns>
        private static string GetAlertType(string notificationMessage, string alertType)
        {
            string notificationWrapperInfo = "<div id='notify-message' class='validation-summary-info alert-message alert-message alert-message-info'><i class='fas fa-info-circle'></i><span>{0}</span></div>";
            string notificationWrapperSuccess = "<div id='notify-message' class='validation-summary-success alert-message alert-message-success'><i class='fas fa-check'></i><span>{0}</span></div>";
            string notificationWrapperWarning = "<div id='notify-message' class='validation-summary-warning alert-message alert-message-warning'><i class='fas fa-exclamation-triangle'></i><span>{0}</span></div>";
            string notificationWrapperDanger = "<div id='notify-message' class='validation-summary-errors alert-message alert-message-danger'><i class='fas fa-exclamation-triangle'></i><span>{0}</span></div>";
            string notificationWrapperCertificate = "<div id='notify-message' class='validation-summary-certificate alert-message alert-message-certificate'><i class='fas fa-certificate'></i><span>{0}</span></div>";
            string notificationWrapperQuestion = "<div id='notify-message' class='validation-summary-question alert-message alert-message-question'><i class='fas fa-question-circle'></i><span>{0}</span></div>";

            var wrapperNotify = "";
            switch (alertType)
            {
                case "info":
                    wrapperNotify = string.Format(notificationWrapperInfo, notificationMessage);
                    break;
                case "success":
                    wrapperNotify = string.Format(notificationWrapperSuccess, notificationMessage);
                    break;
                case "warning":
                    wrapperNotify = string.Format(notificationWrapperWarning, notificationMessage);
                    break;
                case "danger":
                    wrapperNotify = string.Format(notificationWrapperDanger, notificationMessage);
                    break;
                case "certifcate":
                    wrapperNotify = string.Format(notificationWrapperCertificate, notificationMessage);
                    break;
                case "question":
                    wrapperNotify = string.Format(notificationWrapperQuestion, notificationMessage);
                    break;
                default:
                    wrapperNotify = string.Format(notificationWrapperDanger, notificationMessage);
                    break;
            }
            return wrapperNotify;
        }

        /// <summary>
        /// Common re-usable ModelWrappedErrors method for applying wrapping a list of model errors as bulleted text summary
        /// </summary>
        /// <param name="ModelErrors"></param>
        /// <returns>The ModelWrappedULErrorsSummary message as a HtmlString wrapped value</returns>
        public static string ModelWrappedULErrorsSummary(List<string> ModelErrors)
        {
            List<string> jsonModelErrors = new List<string>();

            if (ModelErrors.Count > 0)
            {
                string jsonModelError = string.Empty;
                foreach (var err in ModelErrors)
                {
                    jsonModelError = $@"<li>{err}</li>";
                    jsonModelErrors.Add(jsonModelError);
                }

                string notificationWrapperUL = $@"<ul id='notify-ul'>{string.Join(@"<br/>", jsonModelErrors)}<ul>";
                return notificationWrapperUL;
            }
            return string.Empty;
        }
    }
}