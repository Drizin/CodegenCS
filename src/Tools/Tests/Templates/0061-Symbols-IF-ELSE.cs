class MyTemplate
{
    void Main(ICodegenOutputFile writer)
    {
        var settings = new MySettings() { SwallowExceptions = true };
        RenderMyApiClient(writer, settings);
    }
    class MySettings
    {
        public bool SwallowExceptions;
    }
    void RenderMyApiClient(ICodegenOutputFile w, MySettings settings)
    {
        w.WriteLine($$"""
        public class MyApiClient
        {
            public void InvokeApi()
            {
                try
                {
                    restApi.Invoke();
                }
                catch (Exception ex)
                { {{IF(settings.SwallowExceptions)}}
                    Log.Error(ex); {{ELSE}}
                    throw; {{ENDIF}}
                }
            }
        }
        """);
    }
}