
using System.Collections.Generic;

public class Period
{
    public int Index { get; set; }
    public string Value { get; set; }
}

public class Measure
{
    public string Metric { get; set; }
    public string Value { get; set; }
    public List<Period> Periods { get; set; }
}

public class Component
{
    public string Dd { get; set; }
    public string Key { get; set; }
    public string Name { get; set; }
    public string Qualifier { get; set; }
    public List<Measure> Measures { get; set; }
}

public class SonarRatingJson
{
    public Component Component { get; set; }
}