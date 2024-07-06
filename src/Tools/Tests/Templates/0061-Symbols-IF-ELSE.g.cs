public class MyApiClient
{
    public void InvokeApi()
    {
        try
        {
            restApi.Invoke();
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }
}
