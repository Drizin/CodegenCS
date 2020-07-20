using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("PersonPhone", Schema = "Person")]
    public partial class PersonPhone
    {
        #region Members
        [Key]
        public int BusinessEntityId { get; set; }
        [Key]
        public string PhoneNumber { get; set; }
        [Key]
        public int PhoneNumberTypeId { get; set; }
        public DateTime ModifiedDate { get; set; }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (BusinessEntityId == default(int) && PhoneNumber == null && PhoneNumberTypeId == default(int))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [Person].[PersonPhone]
                (
                    [BusinessEntityID],
                    [ModifiedDate],
                    [PhoneNumber],
                    [PhoneNumberTypeID]
                )
                VALUES
                (
                    @BusinessEntityId,
                    @ModifiedDate,
                    @PhoneNumber,
                    @PhoneNumberTypeId
                )";

                conn.Execute(cmd, this);
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [Person].[PersonPhone] SET
                    [BusinessEntityID] = @BusinessEntityId,
                    [ModifiedDate] = @ModifiedDate,
                    [PhoneNumber] = @PhoneNumber,
                    [PhoneNumberTypeID] = @PhoneNumberTypeId
                WHERE
                    [BusinessEntityID] = @BusinessEntityId AND 
                    [PhoneNumber] = @PhoneNumber AND 
                    [PhoneNumberTypeID] = @PhoneNumberTypeId";
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
            PersonPhone other = obj as PersonPhone;
            if (other == null) return false;

            if (BusinessEntityId != other.BusinessEntityId)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (PhoneNumber != other.PhoneNumber)
                return false;
            if (PhoneNumberTypeId != other.PhoneNumberTypeId)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (BusinessEntityId == default(int) ? 0 : BusinessEntityId.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (PhoneNumber == null ? 0 : PhoneNumber.GetHashCode());
                hash = hash * 23 + (PhoneNumberTypeId == default(int) ? 0 : PhoneNumberTypeId.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(PersonPhone left, PersonPhone right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PersonPhone left, PersonPhone right)
        {
            return !Equals(left, right);
        }

        #endregion Equals/GetHashCode
    }
}
