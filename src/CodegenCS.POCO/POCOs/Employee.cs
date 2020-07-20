using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    public partial class Employee
    {
        #region Members
        [Key]
        public int BusinessEntityId { get; set; }
        public DateTime BirthDate { get; set; }
        public bool CurrentFlag { get; set; }
        public string Gender { get; set; }
        public DateTime HireDate { get; set; }
        public string JobTitle { get; set; }
        public string LoginId { get; set; }
        public string MaritalStatus { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string NationalIdNumber { get; set; }
        public short? OrganizationLevel { get; set; }
        public Guid Rowguid { get; set; }
        public bool SalariedFlag { get; set; }
        public short SickLeaveHours { get; set; }
        public short VacationHours { get; set; }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (BusinessEntityId == default(int))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [Employee]
                (
                    [BirthDate],
                    [BusinessEntityID],
                    [CurrentFlag],
                    [Gender],
                    [HireDate],
                    [JobTitle],
                    [LoginID],
                    [MaritalStatus],
                    [ModifiedDate],
                    [NationalIDNumber],
                    [SalariedFlag],
                    [SickLeaveHours],
                    [VacationHours]
                )
                VALUES
                (
                    @BirthDate,
                    @BusinessEntityId,
                    @CurrentFlag,
                    @Gender,
                    @HireDate,
                    @JobTitle,
                    @LoginId,
                    @MaritalStatus,
                    @ModifiedDate,
                    @NationalIdNumber,
                    @SalariedFlag,
                    @SickLeaveHours,
                    @VacationHours
                )";

                conn.Execute(cmd, this);
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [Employee] SET
                    [BirthDate] = @BirthDate,
                    [BusinessEntityID] = @BusinessEntityId,
                    [CurrentFlag] = @CurrentFlag,
                    [Gender] = @Gender,
                    [HireDate] = @HireDate,
                    [JobTitle] = @JobTitle,
                    [LoginID] = @LoginId,
                    [MaritalStatus] = @MaritalStatus,
                    [ModifiedDate] = @ModifiedDate,
                    [NationalIDNumber] = @NationalIdNumber,
                    [SalariedFlag] = @SalariedFlag,
                    [SickLeaveHours] = @SickLeaveHours,
                    [VacationHours] = @VacationHours
                WHERE
                    [BusinessEntityID] = @BusinessEntityId";
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
            Employee other = obj as Employee;
            if (other == null) return false;

            if (BirthDate != other.BirthDate)
                return false;
            if (BusinessEntityId != other.BusinessEntityId)
                return false;
            if (CurrentFlag != other.CurrentFlag)
                return false;
            if (Gender != other.Gender)
                return false;
            if (HireDate != other.HireDate)
                return false;
            if (JobTitle != other.JobTitle)
                return false;
            if (LoginId != other.LoginId)
                return false;
            if (MaritalStatus != other.MaritalStatus)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (NationalIdNumber != other.NationalIdNumber)
                return false;
            if (OrganizationLevel != other.OrganizationLevel)
                return false;
            if (Rowguid != other.Rowguid)
                return false;
            if (SalariedFlag != other.SalariedFlag)
                return false;
            if (SickLeaveHours != other.SickLeaveHours)
                return false;
            if (VacationHours != other.VacationHours)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (BirthDate == default(DateTime) ? 0 : BirthDate.GetHashCode());
                hash = hash * 23 + (BusinessEntityId == default(int) ? 0 : BusinessEntityId.GetHashCode());
                hash = hash * 23 + (CurrentFlag == default(bool) ? 0 : CurrentFlag.GetHashCode());
                hash = hash * 23 + (Gender == null ? 0 : Gender.GetHashCode());
                hash = hash * 23 + (HireDate == default(DateTime) ? 0 : HireDate.GetHashCode());
                hash = hash * 23 + (JobTitle == null ? 0 : JobTitle.GetHashCode());
                hash = hash * 23 + (LoginId == null ? 0 : LoginId.GetHashCode());
                hash = hash * 23 + (MaritalStatus == null ? 0 : MaritalStatus.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (NationalIdNumber == null ? 0 : NationalIdNumber.GetHashCode());
                hash = hash * 23 + (OrganizationLevel == null ? 0 : OrganizationLevel.GetHashCode());
                hash = hash * 23 + (Rowguid == default(Guid) ? 0 : Rowguid.GetHashCode());
                hash = hash * 23 + (SalariedFlag == default(bool) ? 0 : SalariedFlag.GetHashCode());
                hash = hash * 23 + (SickLeaveHours == default(short) ? 0 : SickLeaveHours.GetHashCode());
                hash = hash * 23 + (VacationHours == default(short) ? 0 : VacationHours.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(Employee left, Employee right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Employee left, Employee right)
        {
            return !Equals(left, right);
        }

        #endregion Equals/GetHashCode
    }
}
