using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    public partial class ScrapReason
    {
        #region Members
        [Key]
        public short ScrapReasonId { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string Name { get; set; }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (ScrapReasonId == default(short))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [ScrapReason]
                (
                    [ModifiedDate],
                    [Name]
                )
                VALUES
                (
                    @ModifiedDate,
                    @Name
                )";

                this.ScrapReasonId = conn.Query<short>(cmd + "SELECT SCOPE_IDENTITY();", this).Single();
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [ScrapReason]
                    SET [ModifiedDate] = @ModifiedDate,
                    SET [Name] = @Name
                WHERE
                    [ScrapReasonID] = @ScrapReasonId";
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
            ScrapReason other = obj as ScrapReason;
            if (other == null) return false;

            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (Name != other.Name)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (Name == null ? 0 : Name.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(ScrapReason left, ScrapReason right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ScrapReason left, ScrapReason right)
        {
            return !Equals(left, right);
        }

        #endregion Equals/GetHashCode
    }
}
