
using System.Collections.Generic;

public class Col
{
    public string metric { get; set; }
}

public class Cell
{
    public string d { get; set; }
    public List<double> v { get; set; }
}

public class SonarMetricsJson
{
    public List<Col> cols { get; set; }
    public List<Cell> cells { get; set; }
}