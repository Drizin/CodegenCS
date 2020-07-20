using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    public partial class ErrorLog
    {
        #region Members
        [Key]
        public int ErrorLogId { get; set; }
        public int? ErrorLine { get; set; }
        public string ErrorMessage { get; set; }
        public int ErrorNumber { get; set; }
        public string ErrorProcedure { get; set; }
        public int? ErrorSeverity { get; set; }
        public int? ErrorState { get; set; }
        public DateTime ErrorTime { get; set; }
        public string UserName { get; set; }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (ErrorLogId == default(int))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [ErrorLog]
                (
                    [ErrorLine],
                    [ErrorMessage],
                    [ErrorNumber],
                    [ErrorProcedure],
                    [ErrorSeverity],
                    [ErrorState],
                    [ErrorTime],
                    [UserName]
                )
                VALUES
                (
                    @ErrorLine,
                    @ErrorMessage,
                    @ErrorNumber,
                    @ErrorProcedure,
                    @ErrorSeverity,
                    @ErrorState,
                    @ErrorTime,
                    @UserName
                )";

                this.ErrorLogId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", this).Single();
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [ErrorLog]
                    SET [ErrorLine] = @ErrorLine,
                    SET [ErrorMessage] = @ErrorMessage,
                    SET [ErrorNumber] = @ErrorNumber,
                    SET [ErrorProcedure] = @ErrorProcedure,
                    SET [ErrorSeverity] = @ErrorSeverity,
                    SET [ErrorState] = @ErrorState,
                    SET [ErrorTime] = @ErrorTime,
                    SET [UserName] = @UserName
                WHERE
                    [ErrorLogID] = @ErrorLogId";
                conn.Execute(cmd, this);
            }
        }
        #endregion ActiveRecord

        #region Equals/GetHashCode
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            ErrorLog other = obj as ErrorLog;
            if (other == null) return false;

            if (ErrorLine != other.ErrorLine)
                return false;
            if (ErrorMessage != other.ErrorMessage)
                return false;
            if (ErrorNumber != other.ErrorNumber)
                return false;
            if (ErrorProcedure != other.ErrorProcedure)
                return false;
            if (ErrorSeverity != other.ErrorSeverity)
                return false;
            if (ErrorState != other.ErrorState)
                return false;
            if (ErrorTime != other.ErrorTime)
                return false;
            if (UserName != other.UserName)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (ErrorLine == null ? 0 : ErrorLine.GetHashCode());
                hash = hash * 23 + (ErrorMessage == null ? 0 : ErrorMessage.GetHashCode());
                hash = hash * 23 + (ErrorNumber == default(int) ? 0 : ErrorNumber.GetHashCode());
                hash = hash * 23 + (ErrorProcedure == null ? 0 : ErrorProcedure.GetHashCode());
                hash = hash * 23 + (ErrorSeverity == null ? 0 : ErrorSeverity.GetHashCode());
                hash = hash * 23 + (ErrorState == null ? 0 : ErrorState.GetHashCode());
                hash = hash * 23 + (ErrorTime == default(DateTime) ? 0 : ErrorTime.GetHashCode());
                hash = hash * 23 + (UserName == null ? 0 : UserName.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(ErrorLog left, ErrorLog right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ErrorLog left, ErrorLog right)
        {
            return !Equals(left, right);
        }

        #endregion Equals/GetHashCode
    }
}
