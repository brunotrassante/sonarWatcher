using System;

class DateValue
{

    public DateValue(string date, double value)
    {
        this.Date = DateTime.Parse(date).Date;
        this.Value = value;
    }

    public DateTime Date { get; private set; }

    public double Value { get; private set; }
}