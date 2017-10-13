using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Data.Entity.Core;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using System.Web;

namespace OSD_Auto_Unlock
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "MBAMKeyService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select MBAMKeyService.svc or MBAMKeyService.svc.cs at the Solution Explorer and start debugging.
#if DEBUG
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
#endif
    public class MBAMKeyService : MBAMKeyInterface
    {
        public string GetMBAMkey(string keyID)
        {
#if !DEBUG
            string GenericError = "ERROR: Could find any information for supplied KeyID";
#endif
            //Verify incoming keyID is of appropriate length and composition before moving forward.
            try
            {
                if ((keyID.Length != 8) || !Regex.IsMatch(keyID, @"^[A-Z0-9]+$"))
                {
                    throw new System.ArgumentException("KeyID can only contain numbers and Uppercase letters");
                }
            }
            catch   //Error messages are split into debug / release types to control amount of detailed information leakage in the dev version vs. prod version.
            {
                string message = "WARN: a strange partal KeyID was provided and rejected. Given KeyID: " + keyID;
                ErrorLogToFile(message, HttpContext.Current.Request.LogonUserIdentity.ToString(), HttpContext.Current.Request.UserHostAddress.ToString());
#if DEBUG
                return "ERROR: Supplied KeyID failed length and composition check";
#endif
#if !DEBUG
                return GenericError;
#endif
            }
            //Create instance for communicating with MBAM DB instance
            MBAM_Recovery_and_HardwareEntities Result = new MBAM_Recovery_and_HardwareEntities();

            string UnlockKey = "";
            List<string> keyIDs;
            //request full KeyID from user OSD provided partial KeyID
            try 
            {
                keyIDs = Result.GetRecoveryKeyIds(keyID, "OSD_AutoUnlock Request").ToList<string>();
                string message = "AUDIT: A full recovery KeyID was found from the supplied partal KeyID: " + keyID;
                AuditLogToFile(message, HttpContext.Current.Request.LogonUserIdentity.ToString(), HttpContext.Current.Request.UserHostAddress.ToString());
            }
            catch
            {
                string message = "WARN: Could not locate full drive ID for Key ID: " + keyID;
                ErrorLogToFile(message, HttpContext.Current.Request.LogonUserIdentity.ToString(), HttpContext.Current.Request.UserHostAddress.ToString());
#if DEBUG
                return "ERROR: Could not locate full drive ID for Key ID: " + keyID;
#endif
#if !DEBUG
                return GenericError;
#endif
            }

            //request unlock code from MBAM using the first found full KeyID
            string fullKeyID = keyIDs[0];
            try
            {
                List<GetRecoveryKey_Result> passwordValue = Result.GetRecoveryKey(fullKeyID, "OSD_AutoUnlock Request").ToList<GetRecoveryKey_Result>();

                foreach (GetRecoveryKey_Result passwordResult in passwordValue)
                {
                    UnlockKey = passwordResult.RecoveryKey;
                }

                string message = "AUDIT: A full recovery password was provided for the supplied KeyID: " + keyID;
                AuditLogToFile(message, HttpContext.Current.Request.LogonUserIdentity.ToString(), HttpContext.Current.Request.UserHostAddress.ToString());

                return UnlockKey;
            }
            catch
            {
                string message = "WARN: Could not locate recovery password for Key ID: " + keyID;
                ErrorLogToFile(message, HttpContext.Current.Request.LogonUserIdentity.ToString(), HttpContext.Current.Request.UserHostAddress.ToString());
#if DEBUG
                return "ERROR: Could not locate recovery password for Key ID: " + keyID;
#endif
#if !DEBUG
                return GenericError;
#endif
            }
        }

        private void ErrorLogToFile(string message, string Request_UserID, string Request_IP)
        {
            string logPath = WebConfigurationManager.AppSettings["LogFilePath"].ToString();
            System.IO.StreamWriter logWriter = System.IO.File.AppendText(logPath);
            try
            {
                string logLine = String.Format("{0:G}: {1}.", DateTime.Now, message, Request_UserID, Request_IP);
                logWriter.WriteLine(logLine);
            }
            finally
            {
                logWriter.Close();
            }
        }
        private void AuditLogToFile(string message, string Request_UserID, string Request_IP)
        {
            string logPath = WebConfigurationManager.AppSettings["KeyLogFilePath"].ToString();
            System.IO.StreamWriter logWriter = System.IO.File.AppendText(logPath);
            try
            {
                string logLine = String.Format("{0:G}: {1}.", DateTime.Now, message, Request_UserID, Request_IP);
                logWriter.WriteLine(logLine);
            }
            finally
            {
                logWriter.Close();
            }
        }
    }
}
