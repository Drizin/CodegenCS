class MyTemplate
{
    void Main(ICodegenOutputFile writer)
    {
        RenderMyApiClient(writer, true, true);
    }
    void RenderMyApiClient(ICodegenOutputFile w, bool generateConstructor, bool injectHttpClient)
    {
        w.WriteLine($$"""
        {{IF(generateConstructor)}}public class MyApiClient
        {
            public MyApiClient({{IF(injectHttpClient)}}HttpClient httpClient{{ENDIF}})
            { {{IF(injectHttpClient)}} 
                _httpClient = httpClient; {{ENDIF}}
            }
        } {{ENDIF}}
        """);
    }
}