using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("UnitMeasure", Schema = "Production")]
    public partial class UnitMeasure
    {
        #region Members
        [Key]
        public string UnitMeasureCode { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string Name { get; set; }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (UnitMeasureCode == null)
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [Production].[UnitMeasure]
                (
                    [ModifiedDate],
                    [Name],
                    [UnitMeasureCode]
                )
                VALUES
                (
                    @ModifiedDate,
                    @Name,
                    @UnitMeasureCode
                )";

                conn.Execute(cmd, this);
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [Production].[UnitMeasure] SET
                    [ModifiedDate] = @ModifiedDate,
                    [Name] = @Name,
                    [UnitMeasureCode] = @UnitMeasureCode
                WHERE
                    [UnitMeasureCode] = @UnitMeasureCode";
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
            UnitMeasure other = obj as UnitMeasure;
            if (other == null) return false;

            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (Name != other.Name)
                return false;
            if (UnitMeasureCode != other.UnitMeasureCode)
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
                hash = hash * 23 + (UnitMeasureCode == null ? 0 : UnitMeasureCode.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(UnitMeasure left, UnitMeasure right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(UnitMeasure left, UnitMeasure right)
        {
            return !Equals(left, right);
        }

        #endregion Equals/GetHashCode
    }
}
