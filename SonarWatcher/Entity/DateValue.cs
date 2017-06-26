using System;

public class DateValue
{

    public DateValue(string date, double value)
    {
        if (String.IsNullOrEmpty(date))
        {
            this.Date = DateTime.MinValue;
        }
        else
        {
            this.Date = DateTime.Parse(date).Date;
        }

        this.Value = value;
    }

    public DateTime Date { get; private set; }

    public double Value { get; private set; }

    public bool HasNoValue()
    {
        return this.Date == DateTime.MinValue && this.Value == 0;
    }
}