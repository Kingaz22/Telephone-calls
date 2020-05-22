using System;
using Microsoft.VisualBasic.FileIO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelephoneCallsBTK.Model;

namespace TelephoneCallsBTK.ViewModel
{
    public interface IFileService
    {
        List<StoryNumber> Open(string filename);
    }
    public class CsvFileService : IFileService
    {
        public List<StoryNumber> Open(string filename)
        {
            List<StoryNumber> storyNumbers = new List<StoryNumber>();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            using (TextFieldParser tfy = new TextFieldParser(filename, Encoding.GetEncoding("Windows-1251")))
            {
                tfy.TextFieldType = FieldType.Delimited;
                tfy.SetDelimiters(";");
                while (!tfy.EndOfData)
                {
                    string[] fields = tfy.ReadFields();
                    storyNumbers.Add(new StoryNumber()
                    {
                        Phone = fields[0],
                        Name = fields[1],
                        Direction = fields[2],
                        CalledCallerNumber = fields[3],
                        DateStartTime = fields[4],
                        Duration = fields[5],
                        Coast = fields[6]
                    });
                }
            }
            return storyNumbers;
        }
    }

}
