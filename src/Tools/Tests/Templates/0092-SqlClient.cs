#r "System.Data.dll"
#r "System.Data.SqlClient.dll"
#r "System.Data.Common.dll"
using System.Data.SqlClient;

class MyTemplate
{
    private readonly string CONNECTION_STRING = "Data Source=<yourserver>;Initial Catalog='yourdb';Persist Security Info=True;Encrypt=false;User ID=<username>;Password='<password>';";
    async Task<FormattableString> Main()
    {
        using (SqlConnection sqlConnection = new SqlConnection(CONNECTION_STRING))
        {
            sqlConnection.Open();
            return $"My template worked";
        }
        return $"My template failed";
    }
}
