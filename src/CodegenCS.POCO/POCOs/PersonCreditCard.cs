using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;
using System.ComponentModel;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("PersonCreditCard", Schema = "Sales")]
    public partial class PersonCreditCard : INotifyPropertyChanged
    {
        #region Members
        private int _businessEntityId;
        [Key]
        public int BusinessEntityId 
        { 
            get { return _businessEntityId; } 
            set { SetField(ref _businessEntityId, value, nameof(BusinessEntityId)); } 
        }
        private int _creditCardId;
        [Key]
        public int CreditCardId 
        { 
            get { return _creditCardId; } 
            set { SetField(ref _creditCardId, value, nameof(CreditCardId)); } 
        }
        private DateTime _modifiedDate;
        public DateTime ModifiedDate 
        { 
            get { return _modifiedDate; } 
            set { SetField(ref _modifiedDate, value, nameof(ModifiedDate)); } 
        }
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

        #region INotifyPropertyChanged/IsDirty
        public HashSet<string> ChangedProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public void MarkAsClean()
        {
            ChangedProperties.Clear();
        }
        public virtual bool IsDirty => ChangedProperties.Any();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void SetField<T>(ref T field, T value, string propertyName) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                ChangedProperties.Add(propertyName);
                OnPropertyChanged(propertyName);
            }
        }
        protected virtual void OnPropertyChanged(string propertyName) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion INotifyPropertyChanged/IsDirty
    }
}
