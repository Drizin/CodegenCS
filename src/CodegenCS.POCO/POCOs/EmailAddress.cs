using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    public partial class EmailAddress
    {
        #region Members
        [Key]
        public int BusinessEntityId { get; set; }
        [Key]
        public int EmailAddressId { get; set; }
        [Column("EmailAddress")]
        public string EmailAddress1 { get; set; }
        public DateTime ModifiedDate { get; set; }
        public Guid Rowguid { get; set; }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (BusinessEntityId == default(int) && EmailAddressId == default(int))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [EmailAddress]
                (
                    [BusinessEntityID],
                    [EmailAddress],
                    [ModifiedDate]
                )
                VALUES
                (
                    @BusinessEntityId,
                    @EmailAddress1,
                    @ModifiedDate
                )";

                conn.Execute(cmd, this);
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [EmailAddress] SET
                    [BusinessEntityID] = @BusinessEntityId,
                    [EmailAddress] = @EmailAddress1,
                    [ModifiedDate] = @ModifiedDate
                WHERE
                    [BusinessEntityID] = @BusinessEntityId AND 
                    [EmailAddressID] = @EmailAddressId";
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
            EmailAddress other = obj as EmailAddress;
            if (other == null) return false;

            if (BusinessEntityId != other.BusinessEntityId)
                return false;
            if (EmailAddress1 != other.EmailAddress1)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (Rowguid != other.Rowguid)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (BusinessEntityId == default(int) ? 0 : BusinessEntityId.GetHashCode());
                hash = hash * 23 + (EmailAddress1 == null ? 0 : EmailAddress1.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (Rowguid == default(Guid) ? 0 : Rowguid.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(EmailAddress left, EmailAddress right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EmailAddress left, EmailAddress right)
        {
            return !Equals(left, right);
        }

        #endregion Equals/GetHashCode
    }
}
