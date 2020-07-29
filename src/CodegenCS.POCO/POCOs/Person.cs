using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;
using System.ComponentModel;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("Person", Schema = "Person")]
    public partial class Person : INotifyPropertyChanged
    {
        #region Members
        private int _businessEntityId;
        [Key]
        public int BusinessEntityId 
        { 
            get { return _businessEntityId; } 
            set { SetField(ref _businessEntityId, value, nameof(BusinessEntityId)); } 
        }
        private string _additionalContactInfo;
        public string AdditionalContactInfo 
        { 
            get { return _additionalContactInfo; } 
            set { SetField(ref _additionalContactInfo, value, nameof(AdditionalContactInfo)); } 
        }
        private string _demographics;
        public string Demographics 
        { 
            get { return _demographics; } 
            set { SetField(ref _demographics, value, nameof(Demographics)); } 
        }
        private int _emailPromotion;
        public int EmailPromotion 
        { 
            get { return _emailPromotion; } 
            set { SetField(ref _emailPromotion, value, nameof(EmailPromotion)); } 
        }
        private string _firstName;
        public string FirstName 
        { 
            get { return _firstName; } 
            set { SetField(ref _firstName, value, nameof(FirstName)); } 
        }
        private string _lastName;
        public string LastName 
        { 
            get { return _lastName; } 
            set { SetField(ref _lastName, value, nameof(LastName)); } 
        }
        private string _middleName;
        public string MiddleName 
        { 
            get { return _middleName; } 
            set { SetField(ref _middleName, value, nameof(MiddleName)); } 
        }
        private DateTime _modifiedDate;
        public DateTime ModifiedDate 
        { 
            get { return _modifiedDate; } 
            set { SetField(ref _modifiedDate, value, nameof(ModifiedDate)); } 
        }
        private bool _nameStyle;
        public bool NameStyle 
        { 
            get { return _nameStyle; } 
            set { SetField(ref _nameStyle, value, nameof(NameStyle)); } 
        }
        private string _personType;
        public string PersonType 
        { 
            get { return _personType; } 
            set { SetField(ref _personType, value, nameof(PersonType)); } 
        }
        private Guid _rowguid;
        public Guid Rowguid 
        { 
            get { return _rowguid; } 
            set { SetField(ref _rowguid, value, nameof(Rowguid)); } 
        }
        private string _suffix;
        public string Suffix 
        { 
            get { return _suffix; } 
            set { SetField(ref _suffix, value, nameof(Suffix)); } 
        }
        private string _title;
        public string Title 
        { 
            get { return _title; } 
            set { SetField(ref _title, value, nameof(Title)); } 
        }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (BusinessEntityId == default(int))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [Person].[Person]
                (
                    [AdditionalContactInfo],
                    [BusinessEntityID],
                    [Demographics],
                    [EmailPromotion],
                    [FirstName],
                    [LastName],
                    [MiddleName],
                    [ModifiedDate],
                    [NameStyle],
                    [PersonType],
                    [Suffix],
                    [Title]
                )
                VALUES
                (
                    @AdditionalContactInfo,
                    @BusinessEntityId,
                    @Demographics,
                    @EmailPromotion,
                    @FirstName,
                    @LastName,
                    @MiddleName,
                    @ModifiedDate,
                    @NameStyle,
                    @PersonType,
                    @Suffix,
                    @Title
                )";

                conn.Execute(cmd, this);
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [Person].[Person] SET
                    [AdditionalContactInfo] = @AdditionalContactInfo,
                    [BusinessEntityID] = @BusinessEntityId,
                    [Demographics] = @Demographics,
                    [EmailPromotion] = @EmailPromotion,
                    [FirstName] = @FirstName,
                    [LastName] = @LastName,
                    [MiddleName] = @MiddleName,
                    [ModifiedDate] = @ModifiedDate,
                    [NameStyle] = @NameStyle,
                    [PersonType] = @PersonType,
                    [Suffix] = @Suffix,
                    [Title] = @Title
                WHERE
                    [BusinessEntityID] = @BusinessEntityId";
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
            Person other = obj as Person;
            if (other == null) return false;

            if (AdditionalContactInfo != other.AdditionalContactInfo)
                return false;
            if (BusinessEntityId != other.BusinessEntityId)
                return false;
            if (Demographics != other.Demographics)
                return false;
            if (EmailPromotion != other.EmailPromotion)
                return false;
            if (FirstName != other.FirstName)
                return false;
            if (LastName != other.LastName)
                return false;
            if (MiddleName != other.MiddleName)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (NameStyle != other.NameStyle)
                return false;
            if (PersonType != other.PersonType)
                return false;
            if (Rowguid != other.Rowguid)
                return false;
            if (Suffix != other.Suffix)
                return false;
            if (Title != other.Title)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (AdditionalContactInfo == null ? 0 : AdditionalContactInfo.GetHashCode());
                hash = hash * 23 + (BusinessEntityId == default(int) ? 0 : BusinessEntityId.GetHashCode());
                hash = hash * 23 + (Demographics == null ? 0 : Demographics.GetHashCode());
                hash = hash * 23 + (EmailPromotion == default(int) ? 0 : EmailPromotion.GetHashCode());
                hash = hash * 23 + (FirstName == null ? 0 : FirstName.GetHashCode());
                hash = hash * 23 + (LastName == null ? 0 : LastName.GetHashCode());
                hash = hash * 23 + (MiddleName == null ? 0 : MiddleName.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (NameStyle == default(bool) ? 0 : NameStyle.GetHashCode());
                hash = hash * 23 + (PersonType == null ? 0 : PersonType.GetHashCode());
                hash = hash * 23 + (Rowguid == default(Guid) ? 0 : Rowguid.GetHashCode());
                hash = hash * 23 + (Suffix == null ? 0 : Suffix.GetHashCode());
                hash = hash * 23 + (Title == null ? 0 : Title.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(Person left, Person right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Person left, Person right)
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
