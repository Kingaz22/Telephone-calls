using System.Collections.Generic;

namespace TelephoneCallsBTK.Model
{
    public class ReportNumber
    {
        public string MonthYear { get; set; }
        public List<Phone> Phones { get; set; }
        public ReportNumber()
        {
            Phones = new List<Phone>();
        }
    }
    public class Phone
    {
        public string NamePhone { get; set; }
        public List<NameList> NameList { get; set; }

        public Phone()
        {
            NameList = new List<NameList>();
        }
    }
    public class NameList
    {
        public string Name { get; set; }
        public string Dates { get; set; }
    }

}
