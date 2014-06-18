﻿using System.Web.Security;
using System.Configuration.Provider;
using System.Collections.Specialized;
using System;
using System.Data;
using System.Data.Common;
using NuoDb.Data.Client;
using System.Configuration;
using System.Diagnostics;
using System.Web;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Web.Configuration;

//
// This file contains a basic implementation of a .NET Membership provider for NuoDB.
//

namespace NuoDb.Web.Security
{

    public sealed class NuoDbMembershipProvider : MembershipProvider
    {

        //
        // Global connection string, generated password length, generic exception message, event log info.
        //

        private int newPasswordLength = 8;
        private string eventSource = "NuoDbMembershipProvider";
        private string eventLog = "Application";
        private string exceptionMessage = "An exception occurred. Please check the Event Log.";
        private string connectionString;

        //
        // Used when determining encryption key values.
        //

        private MachineKeySection machineKey;

        //
        // If false, exceptions are thrown to the caller. If true,
        // exceptions are written to the event log.
        //

        private bool pWriteExceptionsToEventLog;

        public bool WriteExceptionsToEventLog
        {
            get { return pWriteExceptionsToEventLog; }
            set { pWriteExceptionsToEventLog = value; }
        }


        //
        // System.Configuration.Provider.ProviderBase.Initialize Method
        //

        public override void Initialize(string name, NameValueCollection config)
        {
            //
            // Initialize values from web.config.
            //

            if (config == null)
                throw new ArgumentNullException("config");

            if (name == null || name.Length == 0)
                name = "NuoDbMembershipProvider";

            if (String.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "NuoDb Membership provider");
            }

            // Initialize the abstract base class.
            base.Initialize(name, config);

            pApplicationName = GetConfigValue(config["applicationName"],
                                            System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
            pMaxInvalidPasswordAttempts = Convert.ToInt32(GetConfigValue(config["maxInvalidPasswordAttempts"], "5"));
            pPasswordAttemptWindow = Convert.ToInt32(GetConfigValue(config["passwordAttemptWindow"], "10"));
            pMinRequiredNonAlphanumericCharacters = Convert.ToInt32(GetConfigValue(config["minRequiredNonAlphanumericCharacters"], "1"));
            pMinRequiredPasswordLength = Convert.ToInt32(GetConfigValue(config["minRequiredPasswordLength"], "7"));
            pPasswordStrengthRegularExpression = Convert.ToString(GetConfigValue(config["passwordStrengthRegularExpression"], ""));
            pEnablePasswordReset = Convert.ToBoolean(GetConfigValue(config["enablePasswordReset"], "true"));
            pEnablePasswordRetrieval = Convert.ToBoolean(GetConfigValue(config["enablePasswordRetrieval"], "true"));
            pRequiresQuestionAndAnswer = Convert.ToBoolean(GetConfigValue(config["requiresQuestionAndAnswer"], "false"));
            pRequiresUniqueEmail = Convert.ToBoolean(GetConfigValue(config["requiresUniqueEmail"], "true"));
            pWriteExceptionsToEventLog = Convert.ToBoolean(GetConfigValue(config["writeExceptionsToEventLog"], "true"));

            string temp_format = config["passwordFormat"];
            if (temp_format == null)
            {
                temp_format = "Hashed";
            }

            switch (temp_format)
            {
                case "Hashed":
                    pPasswordFormat = MembershipPasswordFormat.Hashed;
                    break;
                case "Encrypted":
                    pPasswordFormat = MembershipPasswordFormat.Encrypted;
                    break;
                case "Clear":
                    pPasswordFormat = MembershipPasswordFormat.Clear;
                    break;
                default:
                    throw new ProviderException("Password format not supported.");
            }

            //
            // Initialize NuoDbConnection.
            //

            ConnectionStringSettings ConnectionStringSettings =
              ConfigurationManager.ConnectionStrings[config["connectionStringName"]];

            if (ConnectionStringSettings == null || ConnectionStringSettings.ConnectionString.Trim() == "")
            {
                throw new ProviderException("Connection string cannot be blank.");
            }

            connectionString = ConnectionStringSettings.ConnectionString;


            // Get encryption and decryption key information from the configuration.
            Configuration cfg =
              WebConfigurationManager.OpenWebConfiguration(System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
            machineKey = (MachineKeySection)cfg.GetSection("system.web/machineKey");

            if (machineKey.ValidationKey.Contains("AutoGenerate"))
                if (PasswordFormat != MembershipPasswordFormat.Clear)
                    throw new ProviderException("Hashed or Encrypted passwords " +
                                                "are not supported with auto-generated keys.");
        }


        //
        // A helper function to retrieve config values from the configuration file.
        //

        private string GetConfigValue(string configValue, string defaultValue)
        {
            if (String.IsNullOrEmpty(configValue))
                return defaultValue;

            return configValue;
        }


        //
        // System.Web.Security.MembershipProvider properties.
        //


        private string pApplicationName;
        private bool pEnablePasswordReset;
        private bool pEnablePasswordRetrieval;
        private bool pRequiresQuestionAndAnswer;
        private bool pRequiresUniqueEmail;
        private int pMaxInvalidPasswordAttempts;
        private int pPasswordAttemptWindow;
        private MembershipPasswordFormat pPasswordFormat;

        public override string ApplicationName
        {
            get { return pApplicationName; }
            set { pApplicationName = value; }
        }

        public override bool EnablePasswordReset
        {
            get { return pEnablePasswordReset; }
        }


        public override bool EnablePasswordRetrieval
        {
            get { return pEnablePasswordRetrieval; }
        }


        public override bool RequiresQuestionAndAnswer
        {
            get { return pRequiresQuestionAndAnswer; }
        }


        public override bool RequiresUniqueEmail
        {
            get { return pRequiresUniqueEmail; }
        }


        public override int MaxInvalidPasswordAttempts
        {
            get { return pMaxInvalidPasswordAttempts; }
        }


        public override int PasswordAttemptWindow
        {
            get { return pPasswordAttemptWindow; }
        }


        public override MembershipPasswordFormat PasswordFormat
        {
            get { return pPasswordFormat; }
        }

        private int pMinRequiredNonAlphanumericCharacters;

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return pMinRequiredNonAlphanumericCharacters; }
        }

        private int pMinRequiredPasswordLength;

        public override int MinRequiredPasswordLength
        {
            get { return pMinRequiredPasswordLength; }
        }

        private string pPasswordStrengthRegularExpression;

        public override string PasswordStrengthRegularExpression
        {
            get { return pPasswordStrengthRegularExpression; }
        }

        //
        // System.Web.Security.MembershipProvider methods.
        //

        //
        // MembershipProvider.ChangePassword
        //

        public override bool ChangePassword(string username, string oldPwd, string newPwd)
        {
            if (!ValidateUser(username, oldPwd))
                return false;


            ValidatePasswordEventArgs args =
              new ValidatePasswordEventArgs(username, newPwd, true);

            OnValidatingPassword(args);

            if (args.Cancel)
                if (args.FailureInformation != null)
                    throw args.FailureInformation;
                else
                    throw new MembershipPasswordException("Change password canceled due to new password validation failure.");

            int rowsAffected = 0;

            NuoDbConnection conn = null;
            try
            {
                conn = new NuoDbConnection(connectionString);
                conn.Open();

                using (DbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "UPDATE Users " +
                    " SET Password = ?, LastPasswordChangedDate = ? " +
                    " WHERE Username = ? AND ApplicationName = ?";
                    IDbDataParameter passwordParameter = cmd.CreateParameter();
                    passwordParameter.Value = EncodePassword(newPwd);
                    passwordParameter.DbType = DbType.String;
                    cmd.Parameters.Add(passwordParameter);
                    IDbDataParameter lastPasswordChangedDataParameter = cmd.CreateParameter();
                    lastPasswordChangedDataParameter.Value = DateTime.Now;
                    lastPasswordChangedDataParameter.DbType = DbType.DateTime;
                    cmd.Parameters.Add(lastPasswordChangedDataParameter);
                    IDbDataParameter usernameParameter = cmd.CreateParameter();
                    usernameParameter.Value = username;
                    usernameParameter.DbType = DbType.String;
                    cmd.Parameters.Add(usernameParameter);
                    IDbDataParameter applicationNameParameter = cmd.CreateParameter();
                    applicationNameParameter.Value = pApplicationName;
                    applicationNameParameter.DbType = DbType.String;
                    cmd.Parameters.Add(applicationNameParameter);

                    rowsAffected = cmd.ExecuteNonQuery();
                }
            }
            catch (NuoDbSqlException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "ChangePassword");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                conn.Close();
            }

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }



        //
        // MembershipProvider.ChangePasswordQuestionAndAnswer
        //

        public override bool ChangePasswordQuestionAndAnswer(string username,
                      string password,
                      string newPwdQuestion,
                      string newPwdAnswer)
        {
            if (!ValidateUser(username, password))
                return false;

            int rowsAffected = 0;
            NuoDbConnection conn = null;
            try
            {
                conn = new NuoDbConnection(connectionString);
                conn.Open();

                using (DbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "UPDATE Users " +
                    " SET PasswordQuestion = ?, PasswordAnswer = ?" +
                    " WHERE Username = ? AND ApplicationName = ?";
                    IDbDataParameter questionParameter = cmd.CreateParameter();
                    questionParameter.Value = newPwdQuestion;
                    questionParameter.DbType = DbType.String;
                    cmd.Parameters.Add(questionParameter);
                    IDbDataParameter passwordParameter = cmd.CreateParameter();
                    passwordParameter.Value = EncodePassword(newPwdAnswer);
                    passwordParameter.DbType = DbType.String;
                    cmd.Parameters.Add(passwordParameter);
                    IDbDataParameter usernameParameter = cmd.CreateParameter();
                    usernameParameter.Value = username;
                    usernameParameter.DbType = DbType.String;
                    cmd.Parameters.Add(usernameParameter);
                    IDbDataParameter applicationNameParameter = cmd.CreateParameter();
                    applicationNameParameter.Value = pApplicationName;
                    applicationNameParameter.DbType = DbType.String;
                    cmd.Parameters.Add(applicationNameParameter);

                    rowsAffected = cmd.ExecuteNonQuery();
                }
            }
            catch (NuoDbSqlException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "ChangePasswordQuestionAndAnswer");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                conn.Close();
            }

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }



        //
        // MembershipProvider.CreateUser
        //

        public override MembershipUser CreateUser(string username,
                 string password,
                 string email,
                 string passwordQuestion,
                 string passwordAnswer,
                 bool isApproved,
                 object providerUserKey,
                 out MembershipCreateStatus status)
        {
            ValidatePasswordEventArgs args =
              new ValidatePasswordEventArgs(username, password, true);

            OnValidatingPassword(args);

            if (args.Cancel)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }



            if (RequiresUniqueEmail && GetUserNameByEmail(email) != "")
            {
                status = MembershipCreateStatus.DuplicateEmail;
                return null;
            }

            MembershipUser u = GetUser(username, false);

            if (u == null)
            {
                DateTime createDate = DateTime.Now;

                if (providerUserKey == null)
                {
                    providerUserKey = Guid.NewGuid();
                }
                else
                {
                    if (!(providerUserKey is Guid))
                    {
                        status = MembershipCreateStatus.InvalidProviderUserKey;
                        return null;
                    }
                }

                NuoDbConnection conn = null;
                try
                {
                    conn = new NuoDbConnection(connectionString);
                    conn.Open();

                    using (DbCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "INSERT INTO Users " +
                            " (PKID, Username, Password, Email, PasswordQuestion, " +
                            " PasswordAnswer, IsApproved," +
                            " Comment, CreationDate, LastPasswordChangedDate, LastActivityDate," +
                            " ApplicationName, IsLockedOut, LastLockedOutDate," +
                            " FailedPasswordAttemptCount, FailedPasswordAttemptWindowStart, " +
                            " FailedPasswordAnswerAttemptCount, FailedPasswordAnswerAttemptWindowStart)" +
                            " Values(?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

                        IDbDataParameter pkidParameter = cmd.CreateParameter();
                        pkidParameter.Value = providerUserKey;
                        pkidParameter.DbType = DbType.String;
                        cmd.Parameters.Add(pkidParameter);
                        IDbDataParameter usernameParameter = cmd.CreateParameter();
                        usernameParameter.Value = username;
                        usernameParameter.DbType = DbType.String;
                        cmd.Parameters.Add(usernameParameter);
                        IDbDataParameter passwordParameter = cmd.CreateParameter();
                        passwordParameter.Value = EncodePassword(password);
                        passwordParameter.DbType = DbType.String;
                        cmd.Parameters.Add(passwordParameter);
                        IDbDataParameter emailParameter = cmd.CreateParameter();
                        emailParameter.Value = email;
                        emailParameter.DbType = DbType.String;
                        cmd.Parameters.Add(emailParameter);
                        IDbDataParameter passwordQuestionParameter = cmd.CreateParameter();
                        passwordQuestionParameter.Value = passwordQuestion;
                        passwordQuestionParameter.DbType = DbType.String;
                        cmd.Parameters.Add(passwordQuestionParameter);
                        IDbDataParameter passwordAnswerParameter = cmd.CreateParameter();
                        passwordAnswerParameter.Value = EncodePassword(passwordAnswer);
                        passwordAnswerParameter.DbType = DbType.String;
                        cmd.Parameters.Add(passwordAnswerParameter);
                        IDbDataParameter isApprovedParameter = cmd.CreateParameter();
                        isApprovedParameter.Value = isApproved;
                        isApprovedParameter.DbType = DbType.Boolean;
                        cmd.Parameters.Add(isApprovedParameter);
                        IDbDataParameter commentParameter = cmd.CreateParameter();
                        commentParameter.Value = "";
                        commentParameter.DbType = DbType.String;
                        cmd.Parameters.Add(commentParameter);
                        IDbDataParameter creationDateParameter = cmd.CreateParameter();
                        creationDateParameter.Value = createDate;
                        creationDateParameter.DbType = DbType.DateTime;
                        cmd.Parameters.Add(creationDateParameter);
                        IDbDataParameter lastPasswordChangedDateParameter = cmd.CreateParameter();
                        lastPasswordChangedDateParameter.Value = createDate;
                        lastPasswordChangedDateParameter.DbType = DbType.DateTime;
                        cmd.Parameters.Add(lastPasswordChangedDateParameter);
                        IDbDataParameter lastActivityDateParameter = cmd.CreateParameter();
                        lastActivityDateParameter.Value = createDate;
                        lastActivityDateParameter.DbType = DbType.DateTime;
                        cmd.Parameters.Add(lastActivityDateParameter);
                        IDbDataParameter applicationNameParameter = cmd.CreateParameter();
                        applicationNameParameter.Value = pApplicationName;
                        applicationNameParameter.DbType = DbType.String;
                        cmd.Parameters.Add(applicationNameParameter);
                        IDbDataParameter isLockedOutParameter = cmd.CreateParameter();
                        isLockedOutParameter.Value = false;
                        isLockedOutParameter.DbType = DbType.Boolean;
                        cmd.Parameters.Add(isLockedOutParameter);
                        IDbDataParameter lastLockedOutDateParameter = cmd.CreateParameter();
                        lastLockedOutDateParameter.Value = createDate;
                        lastLockedOutDateParameter.DbType = DbType.DateTime;
                        cmd.Parameters.Add(lastLockedOutDateParameter);
                        IDbDataParameter failedPasswordAttemptCountParameter = cmd.CreateParameter();
                        failedPasswordAttemptCountParameter.Value = 0;
                        failedPasswordAttemptCountParameter.DbType = DbType.Int32;
                        cmd.Parameters.Add(failedPasswordAttemptCountParameter);
                        IDbDataParameter failedPasswordAttemptWindowStartParameter = cmd.CreateParameter();
                        failedPasswordAttemptWindowStartParameter.Value = createDate;
                        failedPasswordAttemptWindowStartParameter.DbType = DbType.DateTime;
                        cmd.Parameters.Add(failedPasswordAttemptWindowStartParameter);
                        IDbDataParameter failedPasswordAnswerAttemptCountParameter = cmd.CreateParameter();
                        failedPasswordAnswerAttemptCountParameter.Value = 0;
                        failedPasswordAnswerAttemptCountParameter.DbType = DbType.Int32;
                        cmd.Parameters.Add(failedPasswordAnswerAttemptCountParameter);
                        IDbDataParameter failedPasswordAnswerAttemptWindowStartParameter = cmd.CreateParameter();
                        failedPasswordAnswerAttemptWindowStartParameter.Value = createDate;
                        failedPasswordAnswerAttemptWindowStartParameter.DbType = DbType.DateTime;
                        cmd.Parameters.Add(failedPasswordAnswerAttemptWindowStartParameter);

                        int recAdded = cmd.ExecuteNonQuery();

                        if (recAdded > 0)
                        {
                            status = MembershipCreateStatus.Success;
                        }
                        else
                        {
                            status = MembershipCreateStatus.UserRejected;
                        }
                    }
                }
                catch (NuoDbSqlException e)
                {
                    if (WriteExceptionsToEventLog)
                    {
                        WriteToEventLog(e, "CreateUser");
                    }

                    status = MembershipCreateStatus.ProviderError;
                }
                finally
                {
                    conn.Close();
                }


                return GetUser(username, false);
            }
            else
            {
                status = MembershipCreateStatus.DuplicateUserName;
            }


            return null;
        }



        //
        // MembershipProvider.DeleteUser
        //

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {

            NuoDbConnection conn = null;
            int rowsAffected = 0;

            try
            {
                conn = new NuoDbConnection(connectionString);
                conn.Open();

                using (DbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM Users " +
                    " WHERE Username = ? AND Applicationname = ?";

                    IDbDataParameter usernameParameter = cmd.CreateParameter();
                    usernameParameter.Value = username;
                    usernameParameter.DbType = DbType.String;
                    cmd.Parameters.Add(usernameParameter);
                    IDbDataParameter applicationNameParameter = cmd.CreateParameter();
                    applicationNameParameter.Value = pApplicationName;
                    applicationNameParameter.DbType = DbType.String;
                    cmd.Parameters.Add(applicationNameParameter);

                    rowsAffected = cmd.ExecuteNonQuery();

                    if (deleteAllRelatedData)
                    {
                        // Process commands to delete all data for the user in the database.
                    }
                }
            }
            catch (NuoDbSqlException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "DeleteUser");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                conn.Close();
            }

            if (rowsAffected > 0)
                return true;

            return false;
        }



        //
        // MembershipProvider.GetAllUsers
        //

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            NuoDbConnection conn = null;
            MembershipUserCollection users = new MembershipUserCollection();
            DbDataReader reader = null;
            totalRecords = 0;
            try
            {
                conn = new NuoDbConnection(connectionString);
                conn.Open();

                using (DbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Count(*) FROM Users " +
                                              "WHERE ApplicationName = ?";
                    IDbDataParameter applicationNameParameter = cmd.CreateParameter();
                    applicationNameParameter.Value = pApplicationName;
                    applicationNameParameter.DbType = DbType.String;
                    cmd.Parameters.Add(applicationNameParameter);

                    totalRecords = (int)cmd.ExecuteScalar();

                    if (totalRecords <= 0) { return users; }

                    cmd.CommandText = "SELECT PKID, Username, Email, PasswordQuestion," +
                         " Comment, IsApproved, IsLockedOut, CreationDate, LastLoginDate," +
                         " LastActivityDate, LastPasswordChangedDate, LastLockedOutDate " +
                         " FROM Users " +
                         " WHERE ApplicationName = ? " +
                         " ORDER BY Username Asc";

                    reader = cmd.ExecuteReader();

                    int counter = 0;
                    int startIndex = pageSize * pageIndex;
                    int endIndex = startIndex + pageSize - 1;

                    while (reader.Read())
                    {
                        if (counter >= startIndex)
                        {
                            MembershipUser u = GetUserFromReader(reader);
                            users.Add(u);
                        }

                        if (counter >= endIndex) { break; }

                        counter++;
                    }
                }
            }
            catch (NuoDbSqlException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "GetAllUsers ");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                if (reader != null) { reader.Close(); }
                conn.Close();
            }

            return users;
        }


        //
        // MembershipProvider.GetNumberOfUsersOnline
        //

        public override int GetNumberOfUsersOnline()
        {

            TimeSpan onlineSpan = new TimeSpan(0, System.Web.Security.Membership.UserIsOnlineTimeWindow, 0);
            DateTime compareTime = DateTime.Now.Subtract(onlineSpan);

            NuoDbConnection conn = null;
            int numOnline = 0;
            try
            {
                conn = new NuoDbConnection(connectionString);
                conn.Open();

                using (DbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Count(*) FROM Users " +
                    " WHERE LastActivityDate > ? AND ApplicationName = ?";

                    IDbDataParameter compareDateParameter = cmd.CreateParameter();
                    compareDateParameter.Value = compareTime;
                    compareDateParameter.DbType = DbType.DateTime;
                    cmd.Parameters.Add(compareDateParameter);
                    IDbDataParameter applicationNameParameter = cmd.CreateParameter();
                    applicationNameParameter.Value = pApplicationName;
                    applicationNameParameter.DbType = DbType.String;
                    cmd.Parameters.Add(applicationNameParameter);


                    numOnline = (int)cmd.ExecuteScalar();
                }
            }
            catch (NuoDbSqlException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "GetNumberOfUsersOnline");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                conn.Close();
            }

            return numOnline;
        }



        //
        // MembershipProvider.GetPassword
        //

        public override string GetPassword(string username, string answer)
        {
            if (!EnablePasswordRetrieval)
            {
                throw new ProviderException("Password Retrieval Not Enabled.");
            }

            if (PasswordFormat == MembershipPasswordFormat.Hashed)
            {
                throw new ProviderException("Cannot retrieve Hashed passwords.");
            }

            NuoDbConnection conn = null;
            string password = "";
            string passwordAnswer = "";
            DbDataReader reader = null;
            try
            {
                conn = new NuoDbConnection(connectionString);
                conn.Open();

                using (DbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Password, PasswordAnswer, IsLockedOut FROM Users " +
                        " WHERE Username = ? AND ApplicationName = ?";

                    IDbDataParameter usernameParameter = cmd.CreateParameter();
                    usernameParameter.Value = username;
                    usernameParameter.DbType = DbType.String;
                    cmd.Parameters.Add(usernameParameter);
                    IDbDataParameter applicationNameParameter = cmd.CreateParameter();
                    applicationNameParameter.Value = pApplicationName;
                    applicationNameParameter.DbType = DbType.String;
                    cmd.Parameters.Add(applicationNameParameter);

                    reader = cmd.ExecuteReader(CommandBehavior.SingleRow);

                    if (reader.Read())
                    {
                        if (reader.GetBoolean(2))
                            throw new MembershipPasswordException("The supplied user is locked out.");

                        password = reader.GetString(0);
                        passwordAnswer = reader.GetString(1);
                    }
                    else
                    {
                        throw new MembershipPasswordException("The supplied user name is not found.");
                    }
                }
            }
            catch (NuoDbSqlException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "GetPassword");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                if (reader != null) { reader.Close(); }
                conn.Close();
            }


            if (RequiresQuestionAndAnswer && !CheckPassword(answer, passwordAnswer))
            {
                UpdateFailureCount(username, "passwordAnswer");

                throw new MembershipPasswordException("Incorrect password answer.");
            }


            if (PasswordFormat == MembershipPasswordFormat.Encrypted)
            {
                password = UnEncodePassword(password);
            }

            return password;
        }



        //
        // MembershipProvider.GetUser(string, bool)
        //

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {

            NuoDbConnection conn = null;
            MembershipUser u = null;
            DbDataReader reader = null;
            try
            {
                conn = new NuoDbConnection(connectionString);
                conn.Open();

                using (DbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT PKID, Username, Email, PasswordQuestion," +
                        " Comment, IsApproved, IsLockedOut, CreationDate, LastLoginDate," +
                        " LastActivityDate, LastPasswordChangedDate, LastLockedOutDate" +
                        " FROM Users WHERE Username = ? AND ApplicationName = ?";

                    IDbDataParameter usernameParameter = cmd.CreateParameter();
                    usernameParameter.Value = username;
                    usernameParameter.DbType = DbType.String;
                    cmd.Parameters.Add(usernameParameter);
                    IDbDataParameter applicationNameParameter = cmd.CreateParameter();
                    applicationNameParameter.Value = pApplicationName;
                    applicationNameParameter.DbType = DbType.String;
                    cmd.Parameters.Add(applicationNameParameter);


                    reader = cmd.ExecuteReader();
                    
                    if (reader.Read())
                    {
                        u = GetUserFromReader(reader);

                        if (userIsOnline)
                        {
                            using (DbCommand updateCmd = conn.CreateCommand())
                            {
                                updateCmd.CommandText = "UPDATE Users " +
                                      "SET LastActivityDate = ? " +
                                      "WHERE Username = ? AND Applicationname = ?";

                                IDbDataParameter lastActivityDateParameter = updateCmd.CreateParameter();
                                lastActivityDateParameter.Value = DateTime.Now;
                                lastActivityDateParameter.DbType = DbType.DateTime;
                                updateCmd.Parameters.Add(lastActivityDateParameter);
                                updateCmd.Parameters.Add(usernameParameter);
                                updateCmd.Parameters.Add(applicationNameParameter);
                                updateCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }

            }
            catch (NuoDbSqlException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "GetUser(String, Boolean)");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                if (reader != null) { reader.Close(); }

                conn.Close();
            }

            return u;
        }


        //
        // MembershipProvider.GetUser(object, bool)
        //

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            NuoDbConnection conn = null;
            MembershipUser u = null;
            DbDataReader reader = null;
            try
            {
                conn = new NuoDbConnection(connectionString);
                conn.Open();

                using (DbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT PKID, Username, Email, PasswordQuestion," +
                        " Comment, IsApproved, IsLockedOut, CreationDate, LastLoginDate," +
                        " LastActivityDate, LastPasswordChangedDate, LastLockedOutDate" +
                        " FROM Users WHERE PKID = ?";

                    IDbDataParameter pkidParameter = cmd.CreateParameter();
                    pkidParameter.Value = providerUserKey;
                    pkidParameter.DbType = DbType.String;
                    cmd.Parameters.Add(pkidParameter);

                    reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        u = GetUserFromReader(reader);

                        if (userIsOnline)
                        {
                            using (DbCommand updateCmd = conn.CreateCommand())
                            {
                                updateCmd.CommandText = "UPDATE Users " +
                                  "SET LastActivityDate = ? " +
                                  "WHERE PKID = ?";

                                IDbDataParameter lastActivityDateParameter = updateCmd.CreateParameter();
                                lastActivityDateParameter.Value = DateTime.Now;
                                lastActivityDateParameter.DbType = DbType.DateTime;
                                updateCmd.Parameters.Add(lastActivityDateParameter);

                                updateCmd.Parameters.Add(pkidParameter);

                                updateCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }

            }
            catch (NuoDbSqlException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "GetUser(Object, Boolean)");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                if (reader != null) { reader.Close(); }

                conn.Close();
            }

            return u;
        }


        //
        // GetUserFromReader
        //    A helper function that takes the current row from the DbDataReader
        // and hydrates a MembershiUser from the values. Called by the 
        // MembershipUser.GetUser implementation.
        //

        private MembershipUser GetUserFromReader(DbDataReader reader)
        {
            object providerUserKey = reader.GetValue(0);
            string username = reader.GetString(1);
            string email = reader.GetString(2);

            string passwordQuestion = "";
            if (reader.GetValue(3) != DBNull.Value)
                passwordQuestion = reader.GetString(3);

            string comment = "";
            if (reader.GetValue(4) != DBNull.Value)
                comment = reader.GetString(4);

            bool isApproved = reader.GetBoolean(5);
            bool isLockedOut = reader.GetBoolean(6);
            DateTime creationDate = reader.GetDateTime(7);

            DateTime lastLoginDate = new DateTime();
            if (reader.GetValue(8) != DBNull.Value)
                lastLoginDate = reader.GetDateTime(8);

            DateTime lastActivityDate = reader.GetDateTime(9);
            DateTime lastPasswordChangedDate = reader.GetDateTime(10);

            DateTime lastLockedOutDate = new DateTime();
            if (reader.GetValue(11) != DBNull.Value)
                lastLockedOutDate = reader.GetDateTime(11);

            MembershipUser u = new MembershipUser(this.Name,
                                                  username,
                                                  providerUserKey,
                                                  email,
                                                  passwordQuestion,
                                                  comment,
                                                  isApproved,
                                                  isLockedOut,
                                                  creationDate,
                                                  lastLoginDate,
                                                  lastActivityDate,
                                                  lastPasswordChangedDate,
                                                  lastLockedOutDate);

            return u;
        }


        //
        // MembershipProvider.UnlockUser
        //

        public override bool UnlockUser(string username)
        {
            NuoDbConnection conn = null;
            int rowsAffected = 0;
            try
            {
                conn = new NuoDbConnection(connectionString);
                conn.Open();

                using (DbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "UPDATE Users " +
                      " SET IsLockedOut = False, LastLockedOutDate = ? " +
                      " WHERE Username = ? AND ApplicationName = ?";

                    IDbDataParameter lastLockedOutDateParameter = cmd.CreateParameter();
                    lastLockedOutDateParameter.Value = DateTime.Now;
                    lastLockedOutDateParameter.DbType = DbType.DateTime;
                    cmd.Parameters.Add(lastLockedOutDateParameter);
                    IDbDataParameter usernameParameter = cmd.CreateParameter();
                    usernameParameter.Value = username;
                    usernameParameter.DbType = DbType.String;
                    cmd.Parameters.Add(usernameParameter);
                    IDbDataParameter applicationNameParameter = cmd.CreateParameter();
                    applicationNameParameter.Value = pApplicationName;
                    applicationNameParameter.DbType = DbType.String;
                    cmd.Parameters.Add(applicationNameParameter);

                    rowsAffected = cmd.ExecuteNonQuery();
                }
            }
            catch (NuoDbSqlException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "UnlockUser");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                conn.Close();
            }

            if (rowsAffected > 0)
                return true;

            return false;
        }


        //
        // MembershipProvider.GetUserNameByEmail
        //

        public override string GetUserNameByEmail(string email)
        {
            NuoDbConnection conn = null;
            string username = "";

            try
            {
                conn = new NuoDbConnection(connectionString);
                conn.Open();

                using (DbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Username" +
                        " FROM Users WHERE Email = ? AND ApplicationName = ?";

                    IDbDataParameter emailParameter = cmd.CreateParameter();
                    emailParameter.Value = email;
                    emailParameter.DbType = DbType.String;
                    cmd.Parameters.Add(emailParameter);

                    IDbDataParameter applicationNameParameter = cmd.CreateParameter();
                    applicationNameParameter.Value = pApplicationName;
                    applicationNameParameter.DbType = DbType.String;
                    cmd.Parameters.Add(applicationNameParameter);

                    username = (string)cmd.ExecuteScalar();
                }
            }
            catch (NuoDbSqlException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "GetUserNameByEmail");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                conn.Close();
            }

            if (username == null)
                username = "";

            return username;
        }




        //
        // MembershipProvider.ResetPassword
        //

        public override string ResetPassword(string username, string answer)
        {
            if (!EnablePasswordReset)
            {
                throw new NotSupportedException("Password reset is not enabled.");
            }

            if (answer == null && RequiresQuestionAndAnswer)
            {
                UpdateFailureCount(username, "passwordAnswer");

                throw new ProviderException("Password answer required for password reset.");
            }

            string newPassword =
              System.Web.Security.Membership.GeneratePassword(newPasswordLength, MinRequiredNonAlphanumericCharacters);


            ValidatePasswordEventArgs args =
              new ValidatePasswordEventArgs(username, newPassword, true);

            OnValidatingPassword(args);

            if (args.Cancel)
                if (args.FailureInformation != null)
                    throw args.FailureInformation;
                else
                    throw new MembershipPasswordException("Reset password canceled due to password validation failure.");


            NuoDbConnection conn = null;
            int rowsAffected = 0;
            string passwordAnswer = "";
            DbDataReader reader = null;

            try
            {
                conn = new NuoDbConnection(connectionString);
                conn.Open();

                using (DbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT PasswordAnswer, IsLockedOut FROM Users " +
                        " WHERE Username = ? AND ApplicationName = ?";

                    IDbDataParameter usernameParameter = cmd.CreateParameter();
                    usernameParameter.Value = username;
                    usernameParameter.DbType = DbType.String;
                    cmd.Parameters.Add(usernameParameter);
                    IDbDataParameter applicationNameParameter = cmd.CreateParameter();
                    applicationNameParameter.Value = pApplicationName;
                    applicationNameParameter.DbType = DbType.String;
                    cmd.Parameters.Add(applicationNameParameter);



                    reader = cmd.ExecuteReader(CommandBehavior.SingleRow);

                    if (reader.Read())
                    {
                        if (reader.GetBoolean(1))
                            throw new MembershipPasswordException("The supplied user is locked out.");

                        passwordAnswer = reader.GetString(0);
                    }
                    else
                    {
                        throw new MembershipPasswordException("The supplied user name is not found.");
                    }

                    if (RequiresQuestionAndAnswer && !CheckPassword(answer, passwordAnswer))
                    {
                        UpdateFailureCount(username, "passwordAnswer");

                        throw new MembershipPasswordException("Incorrect password answer.");
                    }

                    using (DbCommand updateCmd = conn.CreateCommand())
                    {
                        updateCmd.CommandText = "UPDATE Users " +
                        " SET Password = ?, LastPasswordChangedDate = ?" +
                        " WHERE Username = ? AND ApplicationName = ? AND IsLockedOut = False";

                        IDbDataParameter passwordParameter = updateCmd.CreateParameter();
                        passwordParameter.Value = EncodePassword(newPassword);
                        passwordParameter.DbType = DbType.String;
                        updateCmd.Parameters.Add(passwordParameter);

                        IDbDataParameter lastPasswordChangedDateParameter = updateCmd.CreateParameter();
                        lastPasswordChangedDateParameter.Value = DateTime.Now;
                        lastPasswordChangedDateParameter.DbType = DbType.DateTime;
                        updateCmd.Parameters.Add(lastPasswordChangedDateParameter);

                        updateCmd.Parameters.Add(usernameParameter);

                        updateCmd.Parameters.Add(applicationNameParameter);

                        rowsAffected = updateCmd.ExecuteNonQuery();
                    }
                }
            }
            catch (NuoDbSqlException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "ResetPassword");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                if (reader != null) { reader.Close(); }
                conn.Close();
            }

            if (rowsAffected > 0)
            {
                return newPassword;
            }
            else
            {
                throw new MembershipPasswordException("User not found, or user is locked out. Password not Reset.");
            }
        }


        //
        // MembershipProvider.UpdateUser
        //

        public override void UpdateUser(MembershipUser user)
        {
            NuoDbConnection conn = null;

            try
            {
                conn = new NuoDbConnection(connectionString);
                conn.Open();

                using (DbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "UPDATE Users " +
                    " SET Email = ?, Comment = ?," +
                    " IsApproved = ?" +
                    " WHERE Username = ? AND ApplicationName = ?";

                    IDbDataParameter emailParameter = cmd.CreateParameter();
                    emailParameter.Value = user.Email;
                    emailParameter.DbType = DbType.String;
                    cmd.Parameters.Add(emailParameter);
                    IDbDataParameter commentParameter = cmd.CreateParameter();
                    commentParameter.Value = user.Comment;
                    commentParameter.DbType = DbType.String;
                    cmd.Parameters.Add(commentParameter);
                    IDbDataParameter isApprovedParameter = cmd.CreateParameter();
                    isApprovedParameter.Value = user.IsApproved;
                    isApprovedParameter.DbType = DbType.Boolean;
                    cmd.Parameters.Add(isApprovedParameter);
                    IDbDataParameter usernameParameter = cmd.CreateParameter();
                    usernameParameter.Value = user.UserName;
                    usernameParameter.DbType = DbType.String;
                    cmd.Parameters.Add(usernameParameter);
                    IDbDataParameter applicationNameParameter = cmd.CreateParameter();
                    applicationNameParameter.Value = pApplicationName;
                    applicationNameParameter.DbType = DbType.String;
                    cmd.Parameters.Add(applicationNameParameter);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (NuoDbSqlException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "UpdateUser");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                conn.Close();
            }
        }


        //
        // MembershipProvider.ValidateUser
        //

        public override bool ValidateUser(string username, string password)
        {
            bool isValid = false;


            NuoDbConnection conn = null;
            DbDataReader reader = null;
            bool isApproved = false;
            string pwd = "";

            try
            {
                conn = new NuoDbConnection(connectionString);
                conn.Open();

                using (DbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Password, IsApproved FROM Users " +
                    " WHERE Username = ? AND ApplicationName = ? AND IsLockedOut = False";

                    IDbDataParameter usernameParameter = cmd.CreateParameter();
                    usernameParameter.Value = username;
                    usernameParameter.DbType = DbType.String;
                    cmd.Parameters.Add(usernameParameter);
                    IDbDataParameter applicationNameParameter = cmd.CreateParameter();
                    applicationNameParameter.Value = pApplicationName;
                    applicationNameParameter.DbType = DbType.String;
                    cmd.Parameters.Add(applicationNameParameter);

                    reader = cmd.ExecuteReader(CommandBehavior.SingleRow);

                    if (reader.Read())
                    {
                        pwd = reader.GetString(0);
                        isApproved = reader.GetBoolean(1);
                    }
                    else
                    {
                        return false;
                    }

                    reader.Close();

                    if (CheckPassword(password, pwd))
                    {
                        if (isApproved)
                        {
                            isValid = true;

                            using (DbCommand updateCmd = conn.CreateCommand())
                            {
                                updateCmd.CommandText = "UPDATE Users SET LastLoginDate = ?" +
                                                                            " WHERE Username = ? AND ApplicationName = ?";

                                IDbDataParameter lastLoginDateParameter = updateCmd.CreateParameter();
                                lastLoginDateParameter.Value = DateTime.Now;
                                lastLoginDateParameter.DbType = DbType.DateTime;
                                updateCmd.Parameters.Add(lastLoginDateParameter);

                                updateCmd.Parameters.Add(usernameParameter);

                                updateCmd.Parameters.Add(applicationNameParameter);

                                updateCmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            conn.Close();

                            UpdateFailureCount(username, "password");
                        }
                    }
                }
            }
            catch (NuoDbSqlException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "ValidateUser");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                if (reader != null) { reader.Close(); }
                conn.Close();
            }

            return isValid;
        }


        //
        // UpdateFailureCount
        //   A helper method that performs the checks and updates associated with
        // password failure tracking.
        //

        private void UpdateFailureCount(string username, string failureType)
        {
            NuoDbConnection conn = null;
            DbDataReader reader = null;
            DateTime windowStart = new DateTime();
            int failureCount = 0;

            try
            {
                conn = new NuoDbConnection(connectionString);
                conn.Open();

                using (DbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT FailedPasswordAttemptCount, " +
                                              "  FailedPasswordAttemptWindowStart, " +
                                              "  FailedPasswordAnswerAttemptCount, " +
                                              "  FailedPasswordAnswerAttemptWindowStart " +
                                              "  FROM Users " +
                                              "  WHERE Username = ? AND ApplicationName = ?";

                    IDbDataParameter usernameParameter = cmd.CreateParameter();
                    usernameParameter.Value = username;
                    usernameParameter.DbType = DbType.String;
                    cmd.Parameters.Add(usernameParameter);
                    IDbDataParameter applicationNameParameter = cmd.CreateParameter();
                    applicationNameParameter.Value = pApplicationName;
                    applicationNameParameter.DbType = DbType.String;
                    cmd.Parameters.Add(applicationNameParameter);

                    reader = cmd.ExecuteReader(CommandBehavior.SingleRow);

                    if (reader.HasRows)
                    {
                        reader.Read();

                        if (failureType == "password")
                        {
                            failureCount = reader.GetInt32(0);
                            windowStart = reader.GetDateTime(1);
                        }

                        if (failureType == "passwordAnswer")
                        {
                            failureCount = reader.GetInt32(2);
                            windowStart = reader.GetDateTime(3);
                        }
                    }

                    reader.Close();

                    DateTime windowEnd = windowStart.AddMinutes(PasswordAttemptWindow);

                    if (failureCount == 0 || DateTime.Now > windowEnd)
                    {
                        // First password failure or outside of PasswordAttemptWindow. 
                        // Start a new password failure count from 1 and a new window starting now.

                        using (DbCommand updateCmd = conn.CreateCommand())
                        {
                            if (failureType == "password")
                                updateCmd.CommandText = "UPDATE Users " +
                                                  "  SET FailedPasswordAttemptCount = ?, " +
                                                  "      FailedPasswordAttemptWindowStart = ? " +
                                                  "  WHERE Username = ? AND ApplicationName = ?";

                            if (failureType == "passwordAnswer")
                                updateCmd.CommandText = "UPDATE Users " +
                                                  "  SET FailedPasswordAnswerAttemptCount = ?, " +
                                                  "      FailedPasswordAnswerAttemptWindowStart = ? " +
                                                  "  WHERE Username = ? AND ApplicationName = ?";

                            updateCmd.Parameters.Clear();

                            IDbDataParameter countParameter = updateCmd.CreateParameter();
                            countParameter.Value = 1;
                            countParameter.DbType = DbType.Int32;
                            updateCmd.Parameters.Add(countParameter);
                            IDbDataParameter windowStartParameter = updateCmd.CreateParameter();
                            windowStartParameter.Value = DateTime.Now;
                            windowStartParameter.DbType = DbType.DateTime;
                            updateCmd.Parameters.Add(windowStartParameter);
                            IDbDataParameter updateUsernameParameter = updateCmd.CreateParameter();
                            updateUsernameParameter.Value = username;
                            updateUsernameParameter.DbType = DbType.String;
                            updateCmd.Parameters.Add(updateUsernameParameter);

                            updateCmd.Parameters.Add(applicationNameParameter);

                            if (cmd.ExecuteNonQuery() < 0)
                                throw new ProviderException("Unable to update failure count and window start.");
                        }
                    }
                    else
                    {
                        if (failureCount++ >= MaxInvalidPasswordAttempts)
                        {
                            // Password attempts have exceeded the failure threshold. Lock out
                            // the user.
                            using (DbCommand updateCmd = conn.CreateCommand())
                            {
                                updateCmd.CommandText = "UPDATE Users " +
                                              "  SET IsLockedOut = ?, LastLockedOutDate = ? " +
                                              "  WHERE Username = ? AND ApplicationName = ?";

                                updateCmd.Parameters.Clear();

                                IDbDataParameter isLockedOutParameter = updateCmd.CreateParameter();
                                isLockedOutParameter.Value = true;
                                isLockedOutParameter.DbType = DbType.Boolean;
                                updateCmd.Parameters.Add(isLockedOutParameter);
                                IDbDataParameter lastLockedOutDateParameter = updateCmd.CreateParameter();
                                lastLockedOutDateParameter.Value = DateTime.Now;
                                lastLockedOutDateParameter.DbType = DbType.DateTime;
                                updateCmd.Parameters.Add(lastLockedOutDateParameter);

                                IDbDataParameter updateUsernameParameter = updateCmd.CreateParameter();
                                updateUsernameParameter.Value = username;
                                updateUsernameParameter.DbType = DbType.String;
                                updateCmd.Parameters.Add(updateUsernameParameter);

                                updateCmd.Parameters.Add(applicationNameParameter);


                                if (cmd.ExecuteNonQuery() < 0)
                                    throw new ProviderException("Unable to lock out user.");
                            }
                        }
                        else
                        {
                            // Password attempts have not exceeded the failure threshold. Update
                            // the failure counts. Leave the window the same.
                            using (DbCommand updateCmd = conn.CreateCommand())
                            {
                                if (failureType == "password")
                                    updateCmd.CommandText = "UPDATE Users " +
                                                      "  SET FailedPasswordAttemptCount = ?" +
                                                      "  WHERE Username = ? AND ApplicationName = ?";

                                if (failureType == "passwordAnswer")
                                    updateCmd.CommandText = "UPDATE Users " +
                                                      "  SET FailedPasswordAnswerAttemptCount = ?" +
                                                      "  WHERE Username = ? AND ApplicationName = ?";

                                updateCmd.Parameters.Clear();
                                IDbDataParameter countParameter = updateCmd.CreateParameter();
                                countParameter.Value = failureCount;
                                countParameter.DbType = DbType.Int32;
                                updateCmd.Parameters.Add(countParameter);
                                IDbDataParameter updateUsernameParameter = updateCmd.CreateParameter();
                                updateUsernameParameter.Value = username;
                                updateUsernameParameter.DbType = DbType.String;
                                updateCmd.Parameters.Add(updateUsernameParameter);

                                updateCmd.Parameters.Add(applicationNameParameter);

                                if (cmd.ExecuteNonQuery() < 0)
                                    throw new ProviderException("Unable to update failure count.");
                            }
                        }
                    }
                }
            }
            catch (NuoDbSqlException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "UpdateFailureCount");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                if (reader != null) { reader.Close(); }
                conn.Close();
            }
        }


        //
        // CheckPassword
        //   Compares password values based on the MembershipPasswordFormat.
        //

        private bool CheckPassword(string password, string dbpassword)
        {
            string pass1 = password;
            string pass2 = dbpassword;

            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Encrypted:
                    pass2 = UnEncodePassword(dbpassword);
                    break;
                case MembershipPasswordFormat.Hashed:
                    pass1 = EncodePassword(password);
                    break;
                default:
                    break;
            }

            if (pass1 == pass2)
            {
                return true;
            }

            return false;
        }


        //
        // EncodePassword
        //   Encrypts, Hashes, or leaves the password clear based on the PasswordFormat.
        //

        private string EncodePassword(string password)
        {
            string encodedPassword = password;

            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    break;
                case MembershipPasswordFormat.Encrypted:
                    encodedPassword =
                      Convert.ToBase64String(EncryptPassword(Encoding.Unicode.GetBytes(password)));
                    break;
                case MembershipPasswordFormat.Hashed:
                    HMACSHA1 hash = new HMACSHA1();
                    hash.Key = HexToByte(machineKey.ValidationKey);
                    encodedPassword =
                      Convert.ToBase64String(hash.ComputeHash(Encoding.Unicode.GetBytes(password)));
                    break;
                default:
                    throw new ProviderException("Unsupported password format.");
            }

            return encodedPassword;
        }


        //
        // UnEncodePassword
        //   Decrypts or leaves the password clear based on the PasswordFormat.
        //

        private string UnEncodePassword(string encodedPassword)
        {
            string password = encodedPassword;

            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    break;
                case MembershipPasswordFormat.Encrypted:
                    password =
                      Encoding.Unicode.GetString(DecryptPassword(Convert.FromBase64String(password)));
                    break;
                case MembershipPasswordFormat.Hashed:
                    throw new ProviderException("Cannot unencode a hashed password.");
                default:
                    throw new ProviderException("Unsupported password format.");
            }

            return password;
        }

        //
        // HexToByte
        //   Converts a hexadecimal string to a byte array. Used to convert encryption
        // key values from the configuration.
        //

        private byte[] HexToByte(string hexString)
        {
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }


        //
        // MembershipProvider.FindUsersByName
        //

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            NuoDbConnection conn = null;
            MembershipUserCollection users = new MembershipUserCollection();
            DbDataReader reader = null;

            try
            {
                conn = new NuoDbConnection(connectionString);
                conn.Open();

                using (DbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Count(*) FROM Users " +
                      "WHERE Username LIKE ? AND ApplicationName = ?";

                    IDbDataParameter usernameSearchParameter = cmd.CreateParameter();
                    usernameSearchParameter.Value = usernameToMatch;
                    usernameSearchParameter.DbType = DbType.String;
                    cmd.Parameters.Add(usernameSearchParameter);
                    IDbDataParameter applicationNameParameter = cmd.CreateParameter();
                    applicationNameParameter.Value = pApplicationName;
                    applicationNameParameter.DbType = DbType.String;
                    cmd.Parameters.Add(applicationNameParameter);

                    totalRecords = (int)cmd.ExecuteScalar();

                    if (totalRecords <= 0) { return users; }

                    cmd.CommandText = "SELECT PKID, Username, Email, PasswordQuestion," +
                      " Comment, IsApproved, IsLockedOut, CreationDate, LastLoginDate," +
                      " LastActivityDate, LastPasswordChangedDate, LastLockedOutDate " +
                      " FROM Users " +
                      " WHERE Username LIKE ? AND ApplicationName = ? " +
                      " ORDER BY Username Asc";

                    reader = cmd.ExecuteReader();

                    int counter = 0;
                    int startIndex = pageSize * pageIndex;
                    int endIndex = startIndex + pageSize - 1;

                    while (reader.Read())
                    {
                        if (counter >= startIndex)
                        {
                            MembershipUser u = GetUserFromReader(reader);
                            users.Add(u);
                        }

                        if (counter >= endIndex) { break; }

                        counter++;
                    }
                }
            }
            catch (NuoDbSqlException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "FindUsersByName");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                if (reader != null) { reader.Close(); }

                conn.Close();
            }

            return users;
        }

        //
        // MembershipProvider.FindUsersByEmail
        //

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            NuoDbConnection conn = null;
            MembershipUserCollection users = new MembershipUserCollection();
            DbDataReader reader = null;
            totalRecords = 0;

            try
            {
                conn = new NuoDbConnection(connectionString);
                conn.Open();

                using (DbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Count(*) FROM Users " +
                                              "WHERE Email LIKE ? AND ApplicationName = ?";

                    IDbDataParameter emailSearchParameter = cmd.CreateParameter();
                    emailSearchParameter.Value = emailToMatch;
                    emailSearchParameter.DbType = DbType.String;
                    cmd.Parameters.Add(emailSearchParameter);
                    IDbDataParameter applicationNameParameter = cmd.CreateParameter();
                    applicationNameParameter.Value = pApplicationName;
                    applicationNameParameter.DbType = DbType.String;
                    cmd.Parameters.Add(applicationNameParameter);

                    totalRecords = (int)cmd.ExecuteScalar();

                    if (totalRecords <= 0) { return users; }

                    cmd.CommandText = "SELECT PKID, Username, Email, PasswordQuestion," +
                             " Comment, IsApproved, IsLockedOut, CreationDate, LastLoginDate," +
                             " LastActivityDate, LastPasswordChangedDate, LastLockedOutDate " +
                             " FROM Users " +
                             " WHERE Email LIKE ? AND ApplicationName = ? " +
                             " ORDER BY Username Asc";

                    reader = cmd.ExecuteReader();

                    int counter = 0;
                    int startIndex = pageSize * pageIndex;
                    int endIndex = startIndex + pageSize - 1;

                    while (reader.Read())
                    {
                        if (counter >= startIndex)
                        {
                            MembershipUser u = GetUserFromReader(reader);
                            users.Add(u);
                        }

                        if (counter >= endIndex) { break; }

                        counter++;
                    }
                }
            }
            catch (NuoDbSqlException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "FindUsersByEmail");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                if (reader != null) { reader.Close(); }

                conn.Close();
            }

            return users;
        }


        //
        // WriteToEventLog
        //   A helper function that writes exception detail to the event log. Exceptions
        // are written to the event log as a security measure to avoid private database
        // details from being returned to the browser. If a method does not return a status
        // or boolean indicating the action succeeded or failed, a generic exception is also 
        // thrown by the caller.
        //

        private void WriteToEventLog(Exception e, string action)
        {
            EventLog log = new EventLog();
            log.Source = eventSource;
            log.Log = eventLog;

            string message = "An exception occurred communicating with the data source.\n\n";
            message += "Action: " + action + "\n\n";
            message += "Exception: " + e.ToString();

            log.WriteEntry(message);
        }

    }
}
