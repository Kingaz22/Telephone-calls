using System;
using System.Collections.Generic;
using System.Linq;
using TelephoneCallsBTK.Model;
using Microsoft.Office.Interop.Excel;

namespace TelephoneCallsBTK.ViewModel
{
    static class MyFunc
    {
        /// <summary>
        /// Наименование направления вызова
        /// </summary>
        /// <param name="d">Наименование</param>
        /// <returns></returns>
        public static string Direction(string d)
        {
            return d switch
            {
                "Гомельская" => "Гомельская обл.",
                "Брестская" => "Брестская обл.",
                "Витебская" => "Витебская обл.",
                "Гродненская" => "Гродненская обл.",
                "Минская" => "Минская обл.",
                "Могилевская" => "Могилевская обл.",
                _ => d,
            };
        }

        /// <summary>
        /// Фильтр телефонного номера
        /// </summary>
        /// <param name="p">Номер телефона</param>
        /// <returns></returns>
        public static string Phone(string p)
        {
            return $"{Convert.ToInt32(p.Remove(0, 3)):##-##-##}";
        }

        /// <summary>
        /// Формирование строки дат
        /// </summary>
        /// <param name="aList"></param>
        /// <returns></returns>
        public static string LineDate(List<IGrouping<int, StoryNumber>> aList)
        {
            string str = "";
            foreach (var a in aList)
            {
                if (a.Key < 10)
                {
                    str = str + "0" + a.Key + ", ";
                }
                else
                {
                    str = str + a.Key + ", ";
                }
            }
            return str.Length >= 2 ? str.Remove(str.Length - 2) : str;
        }

        /// <summary>
        /// Фильтр даты
        /// </summary>
        /// <param name="m">Месяц</param>
        /// <param name="y">Год</param>
        /// <returns></returns>
        public static string MonthYear(int m, int y)
        {
            DateTime date = new DateTime(y, m, 01);
            return date.ToString("MMMM yyyy");
        }

        /// <summary>
        /// Экспорт в Excel
        /// </summary>
        /// <param name="reportNumbers">Список отчетов</param>
        public static void ExportExcel(IEnumerable<ReportNumber> reportNumbers)
        {
            Application excelApp = new Application();
            excelApp.Application.Workbooks.Add(Type.Missing);
            excelApp.Rows.RowHeight = 15.5;
            excelApp.Columns.ColumnWidth = 10;
            excelApp.Columns[2].ColumnWidth = 25;
            excelApp.Columns[3].ColumnWidth = 50;
            (excelApp.Cells as Range).Font.Name = "Times New Roman";
            (excelApp.Cells as Range).Font.Size = 12;
            (excelApp.Cells as Range).WrapText = true; 
            (excelApp.Cells as Range).HorizontalAlignment = XlHAlign.xlHAlignCenter;
            (excelApp.Cells as Range).VerticalAlignment = XlVAlign.xlVAlignCenter;

            int i = 1, j = 1;

            foreach (var reportNumber in reportNumbers)
            {
                ((Range)excelApp.get_Range("A" + i.ToString(), "C" + i.ToString()).Cells).Merge(Type.Missing);
                excelApp.Cells[i, 1] = reportNumber.MonthYear;
                excelApp.Cells[i, 1].WrapText = false;
                (excelApp.Cells[i, 1] as Range).Font.Bold = "True";
                excelApp.Cells[i, 1].Font.Size = 14;
                i++;
                excelApp.Rows[i].RowHeight = 31.5;
                excelApp.Cells[i, 1] = "№ телефона";
                excelApp.Cells[i, 2] = "Направление (наименование)";
                excelApp.Cells[i, 3] = "Дата";

                foreach (var phone in reportNumber.Phones)
                {
                    j = i + 1;
                    foreach (var nameList in phone.NameList)
                    {
                        i++;
                        excelApp.Cells[i, 2] = nameList.Name;
                        excelApp.Cells[i, 3] = nameList.Dates;
                    }
                    ((Range)excelApp.get_Range("A" + j.ToString(), "A" + i.ToString()).Cells).Merge(Type.Missing);
                    excelApp.Cells[j, 1] = phone.NamePhone;
                }
                ((Range)excelApp.get_Range("A2", "C" + i.ToString())).Cells.Borders.LineStyle = XlLineStyle.xlContinuous;
                i++;
            }
            excelApp.Visible = true;
        }
       
        /// <summary>
        /// Начальная фильтрация
        /// </summary>
        /// <param name="listPhone">Список номеров</param>
        /// <param name="StoryNumbersFirst">Первоначальный список</param>
        /// <returns></returns>
        public static IEnumerable<StoryNumber> FirstSort(IEnumerable<string> listPhone, IEnumerable<StoryNumber> StoryNumbersFirst)
        {
            var phone = listPhone.ToList();
            if (!phone.Any()) return StoryNumbersFirst.ToList();
            var storyList = new List<StoryNumber>();
            return phone.Aggregate(storyList, (current, i) => StoryNumbersFirst.Where(x => x.Phone == i).Concat(current).ToList());
        }

        /// <summary>
        /// Формирование отчёта
        /// </summary>
        /// <param name="ListPhone">Список номеров</param>
        /// <param name="StoryNumbersFirst">Первоначальный список</param>
        /// <returns></returns>
        public static List<ReportNumber> ReportNumbers(IEnumerable<string> ListPhone, IEnumerable<StoryNumber> StoryNumbersFirst)
        {
            List<ReportNumber> report = new List<ReportNumber>();
            var yearList = StoryNumbersFirst
                    .GroupBy(x => Convert.ToDateTime(x.DateStartTime).Year)
                    .ToList();
            foreach (var year in yearList)
            {
                var monthList = StoryNumbersFirst
                    .Where(x => Convert.ToDateTime(x.DateStartTime).Year == year.Key)
                    .GroupBy(x => Convert.ToDateTime(x.DateStartTime).Month)
                    .ToList();
                foreach (var month in monthList)
                {
                    List<Phone> xPhones = new List<Phone>();
                    foreach (var phone in ListPhone)
                    {
                        var directionList = StoryNumbersFirst
                            .Where(x => x.Phone == phone)
                            .Where(x => x.Direction != "")
                            .Where(x => Convert.ToDateTime(x.DateStartTime).Year == year.Key && Convert.ToDateTime(x.DateStartTime).Month == month.Key)
                            .GroupBy(x => x.Direction)
                            .ToList();
                        List<NameList> nameList = new List<NameList>();
                        foreach (var direction in directionList)
                        {
                            var dayList = StoryNumbersFirst
                                .Where(x => x.Phone == phone)
                                .Where(x => Convert.ToDateTime(x.DateStartTime).Year == year.Key && Convert.ToDateTime(x.DateStartTime).Month == month.Key)
                                .Where(x => x.Direction == direction.Key && x.Direction != "")
                                .GroupBy(x => Convert.ToDateTime(x.DateStartTime).Day)
                                .ToList();
                            NameList xNameList = new NameList
                            {
                                Name = MyFunc.Direction(direction.Key),
                                Dates = MyFunc.LineDate(dayList)
                            };
                            nameList.Add(xNameList);
                        }
                        Phone xPhone = new Phone
                        {
                            NamePhone = MyFunc.Phone(phone),
                            NameList = nameList
                        };
                        xPhones.Add(xPhone);
                    }
                    ReportNumber xReportNumber = new ReportNumber
                    {
                        MonthYear = MyFunc.MonthYear(month.Key, year.Key),
                        Phones = xPhones
                    };
                    report.Add(xReportNumber);
                }
            }
            return report;
        }


    }
}
