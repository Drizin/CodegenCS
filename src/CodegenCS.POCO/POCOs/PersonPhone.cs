using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;
using System.ComponentModel;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("PersonPhone", Schema = "Person")]
    public partial class PersonPhone : INotifyPropertyChanged
    {
        #region Members
        private int _businessEntityId;
        [Key]
        public int BusinessEntityId 
        { 
            get { return _businessEntityId; } 
            set { SetField(ref _businessEntityId, value, nameof(BusinessEntityId)); } 
        }
        private string _phoneNumber;
        [Key]
        public string PhoneNumber 
        { 
            get { return _phoneNumber; } 
            set { SetField(ref _phoneNumber, value, nameof(PhoneNumber)); } 
        }
        private int _phoneNumberTypeId;
        [Key]
        public int PhoneNumberTypeId 
        { 
            get { return _phoneNumberTypeId; } 
            set { SetField(ref _phoneNumberTypeId, value, nameof(PhoneNumberTypeId)); } 
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
