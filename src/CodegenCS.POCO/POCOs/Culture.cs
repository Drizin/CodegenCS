using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    public partial class Culture
    {
        #region Members
        [Key]
        public string CultureId { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string Name { get; set; }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (CultureId == null)
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [Culture]
                (
                    [CultureID],
                    [ModifiedDate],
                    [Name]
                )
                VALUES
                (
                    @CultureId,
                    @ModifiedDate,
                    @Name
                )";

                conn.Execute(cmd, this);
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [Culture]
                    SET [CultureID] = @CultureId,
                    SET [ModifiedDate] = @ModifiedDate,
                    SET [Name] = @Name
                WHERE
                    [CultureID] = @CultureId";
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
            Culture other = obj as Culture;
            if (other == null) return false;

            if (CultureId != other.CultureId)
                return false;
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
                hash = hash * 23 + (CultureId == null ? 0 : CultureId.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (Name == null ? 0 : Name.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(Culture left, Culture right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Culture left, Culture right)
        {
            return !Equals(left, right);
        }

        #endregion Equals/GetHashCode
    }
}
