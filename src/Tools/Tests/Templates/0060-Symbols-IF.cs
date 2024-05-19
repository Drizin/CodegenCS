class MyTemplate
{
    void Main(IModelFactory factory, ICodegenOutputFile writer)
    {
        RenderMyApiClient(writer, true);
    }
    void RenderMyApiClient(ICodegenOutputFile w, bool injectHttpClient)
    {
        w.WriteLine($$"""
          public class MyApiClient
          {
              public MyApiClient({{IF(injectHttpClient)}}HttpClient httpClient{{ENDIF}})
              { {{IF(injectHttpClient)}}
                  _httpClient = httpClient; {{ENDIF}}
              }
          }
          """);
    }
}
