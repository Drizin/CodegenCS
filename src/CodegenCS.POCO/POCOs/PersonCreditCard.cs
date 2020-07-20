using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("PersonCreditCard", Schema = "Sales")]
    public partial class PersonCreditCard
    {
        #region Members
        [Key]
        public int BusinessEntityId { get; set; }
        [Key]
        public int CreditCardId { get; set; }
        public DateTime ModifiedDate { get; set; }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (BusinessEntityId == default(int) && CreditCardId == default(int))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [Sales].[PersonCreditCard]
                (
                    [BusinessEntityID],
                    [CreditCardID],
                    [ModifiedDate]
                )
                VALUES
                (
                    @BusinessEntityId,
                    @CreditCardId,
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
                UPDATE [Sales].[PersonCreditCard] SET
                    [BusinessEntityID] = @BusinessEntityId,
                    [CreditCardID] = @CreditCardId,
                    [ModifiedDate] = @ModifiedDate
                WHERE
                    [BusinessEntityID] = @BusinessEntityId AND 
                    [CreditCardID] = @CreditCardId";
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
            PersonCreditCard other = obj as PersonCreditCard;
            if (other == null) return false;

            if (BusinessEntityId != other.BusinessEntityId)
                return false;
            if (CreditCardId != other.CreditCardId)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (BusinessEntityId == default(int) ? 0 : BusinessEntityId.GetHashCode());
                hash = hash * 23 + (CreditCardId == default(int) ? 0 : CreditCardId.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(PersonCreditCard left, PersonCreditCard right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PersonCreditCard left, PersonCreditCard right)
        {
            return !Equals(left, right);
        }

        #endregion Equals/GetHashCode
    }
}
