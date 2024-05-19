class MyTemplate
{
    void Main(ICodegenOutputFile writer)
    {
        RenderMyApiClient(writer, true);
    }
    void RenderMyApiClient(ICodegenOutputFile w, bool isVisibilityPublic)
    {
        w.WriteLine($$"""
        public class User
        {
            {{IIF(isVisibilityPublic, $"public ")}}string FirstName { get; set; }
            {{IIF(isVisibilityPublic, $"public ", $"protected ")}}string FirstName { get; set; }
        }
        """);
    }
}