using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    public partial class BusinessEntityContact
    {
        #region Members
        [Key]
        public int BusinessEntityId { get; set; }
        [Key]
        public int PersonId { get; set; }
        [Key]
        public int ContactTypeId { get; set; }
        public DateTime ModifiedDate { get; set; }
        public Guid Rowguid { get; set; }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (BusinessEntityId == default(int) && PersonId == default(int) && ContactTypeId == default(int))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [BusinessEntityContact]
                (
                    [BusinessEntityID],
                    [ContactTypeID],
                    [ModifiedDate],
                    [PersonID]
                )
                VALUES
                (
                    @BusinessEntityId,
                    @ContactTypeId,
                    @ModifiedDate,
                    @PersonId
                )";

                conn.Execute(cmd, this);
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [BusinessEntityContact] SET
                    [BusinessEntityID] = @BusinessEntityId,
                    [ContactTypeID] = @ContactTypeId,
                    [ModifiedDate] = @ModifiedDate,
                    [PersonID] = @PersonId
                WHERE
                    [BusinessEntityID] = @BusinessEntityId AND 
                    [PersonID] = @PersonId AND 
                    [ContactTypeID] = @ContactTypeId";
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
            BusinessEntityContact other = obj as BusinessEntityContact;
            if (other == null) return false;

            if (BusinessEntityId != other.BusinessEntityId)
                return false;
            if (ContactTypeId != other.ContactTypeId)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (PersonId != other.PersonId)
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
                hash = hash * 23 + (ContactTypeId == default(int) ? 0 : ContactTypeId.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (PersonId == default(int) ? 0 : PersonId.GetHashCode());
                hash = hash * 23 + (Rowguid == default(Guid) ? 0 : Rowguid.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(BusinessEntityContact left, BusinessEntityContact right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(BusinessEntityContact left, BusinessEntityContact right)
        {
            return !Equals(left, right);
        }

        #endregion Equals/GetHashCode
    }
}
