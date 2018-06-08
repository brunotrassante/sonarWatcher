
using System.Collections.Generic;

public class Col
{
    public string Metric { get; set; }
}

public class Cell
{
    public string D { get; set; }
    public List<double> V { get; set; }
}

public class SonarMetricsJson
{
    public List<Col> Cols { get; set; }
    public List<Cell> Cells { get; set; }
}