public class GraphQlSubSelection
{
    public string Name { get; set; }

    public GraphQlSubSelection[] SubSelections { get; set; }

    public KeyValuePair<string, string>[] Params { get; set; }
}
