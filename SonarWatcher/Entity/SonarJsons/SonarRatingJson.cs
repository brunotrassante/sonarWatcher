
using System.Collections.Generic;

public class Period
{
    public int index { get; set; }
    public string value { get; set; }
}

public class Measure
{
    public string metric { get; set; }
    public string value { get; set; }
    public List<Period> periods { get; set; }
}

public class Component
{
    public string id { get; set; }
    public string key { get; set; }
    public string name { get; set; }
    public string qualifier { get; set; }
    public List<Measure> measures { get; set; }
}

public class SonarRatingJson
{
    public Component component { get; set; }
}