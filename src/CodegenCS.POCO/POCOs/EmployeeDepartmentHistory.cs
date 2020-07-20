using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("EmployeeDepartmentHistory", Schema = "HumanResources")]
    public partial class EmployeeDepartmentHistory
    {
        #region Members
        [Key]
        public int BusinessEntityId { get; set; }
        [Key]
        public short DepartmentId { get; set; }
        [Key]
        public byte ShiftId { get; set; }
        [Key]
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (BusinessEntityId == default(int) && DepartmentId == default(short) && ShiftId == default(byte) && StartDate == default(DateTime))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [HumanResources].[EmployeeDepartmentHistory]
                (
                    [BusinessEntityID],
                    [DepartmentID],
                    [EndDate],
                    [ModifiedDate],
                    [ShiftID],
                    [StartDate]
                )
                VALUES
                (
                    @BusinessEntityId,
                    @DepartmentId,
                    @EndDate,
                    @ModifiedDate,
                    @ShiftId,
                    @StartDate
                )";

                conn.Execute(cmd, this);
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [HumanResources].[EmployeeDepartmentHistory] SET
                    [BusinessEntityID] = @BusinessEntityId,
                    [DepartmentID] = @DepartmentId,
                    [EndDate] = @EndDate,
                    [ModifiedDate] = @ModifiedDate,
                    [ShiftID] = @ShiftId,
                    [StartDate] = @StartDate
                WHERE
                    [BusinessEntityID] = @BusinessEntityId AND 
                    [DepartmentID] = @DepartmentId AND 
                    [ShiftID] = @ShiftId AND 
                    [StartDate] = @StartDate";
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
            EmployeeDepartmentHistory other = obj as EmployeeDepartmentHistory;
            if (other == null) return false;

            if (BusinessEntityId != other.BusinessEntityId)
                return false;
            if (DepartmentId != other.DepartmentId)
                return false;
            if (EndDate != other.EndDate)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (ShiftId != other.ShiftId)
                return false;
            if (StartDate != other.StartDate)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (BusinessEntityId == default(int) ? 0 : BusinessEntityId.GetHashCode());
                hash = hash * 23 + (DepartmentId == default(short) ? 0 : DepartmentId.GetHashCode());
                hash = hash * 23 + (EndDate == null ? 0 : EndDate.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (ShiftId == default(byte) ? 0 : ShiftId.GetHashCode());
                hash = hash * 23 + (StartDate == default(DateTime) ? 0 : StartDate.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(EmployeeDepartmentHistory left, EmployeeDepartmentHistory right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EmployeeDepartmentHistory left, EmployeeDepartmentHistory right)
        {
            return !Equals(left, right);
        }

        #endregion Equals/GetHashCode
    }
}
