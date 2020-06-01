using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Controls;
using System.Windows.Documents;
using TelephoneCallsBTK.Model;

namespace TelephoneCallsBTK.ViewModel
{
    public class ApplicationViewModel : BaseVieModel
    {

        IFileService fileService;
        IDialogService dialogService;
        public bool StFilter { get; set; }

        private IEnumerable<ReportNumber> _reportNumbers;

        public IEnumerable<ReportNumber> ReportNumbers
        {
            get => _reportNumbers;
            set
            {
                _reportNumbers = value;
                OnPropertyChanged(nameof(ReportNumbers));
            }
        }

        private int _countNumbers;
        public int CountNumbers
        {
            get => _countNumbers;
            set
            {
                _countNumbers = value;
                OnPropertyChanged(nameof(CountNumbers));
            }
        }

        private IEnumerable<StoryNumber> _storyNumbersFirst;
        public IEnumerable<StoryNumber> StoryNumbersFirst
        {
            get => _storyNumbersFirst;
            set
            {
                _storyNumbersFirst = value;
                OnPropertyChanged(nameof(StoryNumbers));
            }
        }

        private IEnumerable<StoryNumber> _storyNumbers;
        public IEnumerable<StoryNumber> StoryNumbers
        {
            get => _storyNumbers;
            set
            {
                _storyNumbers = value;
                OnPropertyChanged(nameof(StoryNumbers));
            }
        }

        private IEnumerable<string> _names;
        public IEnumerable<string> Names
        {
            get => _names;
            set
            {
                _names = value;
                OnPropertyChanged(nameof(Names));
            }
        }

        private IEnumerable<string> _listPhone;
        public IEnumerable<string> ListPhone
        {
            get => _listPhone;
            set
            {
                _listPhone = value;
                OnPropertyChanged(nameof(ListPhone));
            }
        }

        public ApplicationViewModel(IDialogService dialogService, IFileService fileService)
        {
            this.dialogService = dialogService;
            this.fileService = fileService;

            Names = new List<string>();
            List<string> listPhone = new List<string>();
            StFilter = true;
            try
            {
                using var sr = new StreamReader("phone.txt");
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    listPhone.Add(line);
                }
            }
            catch (Exception)
            {
                // ignored
            }
            ListPhone = listPhone;
            ReportNumbers = new List<ReportNumber>();
            StoryNumbers = new List<StoryNumber>();
            StoryNumbersFirst = new List<StoryNumber>();
        }

        #region Формирование отчёта
        private RelayCommand _reportCommand;
        public RelayCommand ReportCommand => _reportCommand ??= new RelayCommand(obj =>
        {
            try
            {
                if (ListPhone.Count() == 0) throw new Exception("Нету номеров");
                else
                {
                    List<ReportNumber> report = new List<ReportNumber>();

                    var yearList = StoryNumbersFirst
                            .Where(x => x.Name != "Исходящее местное соединение")
                            .GroupBy(x => Convert.ToDateTime(x.DateStartTime).Year)
                            .ToList();
                    foreach (var year in yearList)
                    {
                        var monthList = StoryNumbersFirst
                            .Where(x => x.Name != "Исходящее местное соединение")
                            .Where(x => Convert.ToDateTime(x.DateStartTime).Year == year.Key)
                            .GroupBy(x => Convert.ToDateTime(x.DateStartTime).Month)
                            .ToList();
                        foreach (var month in monthList)
                        {


                            List<Phone> xPhones = new List<Phone>();
                            foreach (var phone in ListPhone)
                            {
                                var directionList = StoryNumbersFirst
                                    .Where(x => x.Phone == phone && x.Name != "Исходящее местное соединение")
                                    .Where(x => x.Direction != "")
                                    .Where(x => Convert.ToDateTime(x.DateStartTime).Year == year.Key && Convert.ToDateTime(x.DateStartTime).Month == month.Key)
                                    .GroupBy(x => x.Direction)
                                    .ToList();

                                List<NameList> nameList = new List<NameList>();
                                foreach (var direction in directionList)
                                {
                                    var dayList = StoryNumbersFirst
                                        .Where(x => x.Phone == phone && x.Name != "Исходящее местное соединение")
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
                    ReportNumbers = report;
                }
            }
            catch (Exception ex)
            {
                dialogService.ShowMessage("Ошибка: " + ex.Message);
            }
        });
        #endregion

        #region Экспорт в Excel
        private RelayCommand _exportData;
        public RelayCommand ExportData => _exportData ??= new RelayCommand(obj =>
        {
            try
            {
                MyFunc.ExportExcel(ReportNumbers);
            }
            catch (Exception ex)
            {
                dialogService.ShowMessage("Ошибка: " + ex.Message);
            }
        });
        #endregion

        #region Загрузка данных из файла и приминение начальных настроек
        private RelayCommand _openCommand;
        public RelayCommand OpenCommand => _openCommand ??= new RelayCommand(obj =>
        {
            try
            {
                if (dialogService.OpenFileDialog() == true)
                {
                    //var storyNumbers = StoryNumbersFirst.ToList();
                    StoryNumbers = StoryNumbersFirst = fileService.Open(dialogService.FilePath)
                        .Union(StoryNumbersFirst, new StoryNumberClassComparer())
                        .Where(x => x.Phone != "Телефон")
                        .Where(x => x.Name != "Итого сумма начислений по абонентскому номеру:");


                    //var storyNumbers = StoryNumbersFirst.ToList();

                    //StoryNumbers = storyNumbers;
                    if (ListPhone.Count() != 0)
                    {
                        List<StoryNumber> storyList = new List<StoryNumber>();
                        foreach (var i in ListPhone)
                        {
                            storyList = StoryNumbersFirst.Where(x => x.Phone == i).Concat(storyList).ToList();
                        }
                        StoryNumbers = storyList;
                    }

                    if (StFilter)
                    {
                        StoryNumbers = StoryNumbers.Where(x =>
                                x.Name == "Исходящее соединение на мобильную сеть" ||
                                x.Name == "Исходящее междугородное соединение в пределах области" ||
                                x.Name == "Исходящее междугородное соединение в пределах республики").ToList();
                    }
                    else
                    {



                    }

                    CountNumbers = StoryNumbers.Count();

                    #region Формирование списка наименований услуг
                    var nameList = Names.ToList();
                    foreach (var x in StoryNumbersFirst.GroupBy(x => x.Name))
                    {
                        if (nameList.Count(a => a == x.Key) == 0)
                            nameList.Add(x.Key);
                    }
                    Names = nameList;
                    #endregion

                    dialogService.ShowMessage("Файл загружен");
                }
            }
            catch (Exception ex)
            {
                dialogService.ShowMessage("Ошибка: " + ex.Message);
            }
        });
        #endregion


        #region Открыть справку
        private RelayCommand _openHelp;
        public RelayCommand OpenHelp => _openHelp ??= new RelayCommand(obj =>
        {
            Process.Start((Environment.CurrentDirectory + "\\123.chm").Replace("\\", "/"));
        });
        #endregion


        #region Очиcтка данных
        private RelayCommand _clearData;
        public RelayCommand ClearData => _clearData ??= new RelayCommand(obj =>
        {
            try
            {
                StoryNumbers = new List<StoryNumber>();
                Names = new List<string>();
                ReportNumbers = new List<ReportNumber>();
                CountNumbers = 0;
            }
            catch (Exception ex)
            {
                dialogService.ShowMessage("Ошибка: " + ex.Message);
            }
        });
        #endregion

        #region Фильтрация
        private RelayCommand _sort;
        public RelayCommand Sort => _sort ??= new RelayCommand(obj =>
        {
            try
            {
                var item = obj as ListBox;
                foreach (var i in item.SelectedItems)
                {
                    StoryNumbers = StoryNumbers.Where(x => x.Name != i.ToString());
                }
            }
            catch (Exception ex)
            {
                dialogService.ShowMessage("Ошибка: " + ex.Message);
            }
        });
        #endregion

        #region Добавление номера телефона
        private RelayCommand _addPhone;
        public RelayCommand AddPhone => _addPhone ??= new RelayCommand(obj =>
        {
            try
            {
                var listPhone = ListPhone.ToList();
                var item = (obj as TextBox)?.Text;

                if (int.TryParse(item, out var d) && (item.Length == 9))
                {
                    if (!listPhone.Any(x => x.StartsWith(item)))
                    {
                        listPhone.Add(item);
                        ListPhone = listPhone;
                        using var sw = new StreamWriter("phone.txt", false, System.Text.Encoding.Default);
                        foreach (var i in ListPhone)
                            sw.WriteLine(i);
                    }
                    else throw new Exception("Такой номер уже есть в списке");
                }
                else throw new Exception("Не верный формат номера. \nТребуемый формат номера: 232123456");
            }
            catch (Exception ex)
            {
                dialogService.ShowMessage("Ошибка: " + ex.Message);
            }
        });
        #endregion

        #region Удаление номера телефона
        private RelayCommand _deletePhone;
        public RelayCommand DeletePhone => _deletePhone ??= new RelayCommand(obj =>
        {
            try
            {
                var listPhone = ListPhone.ToList();
                var item = (obj as ListBox)?.SelectedItems;
                foreach (var i in item)
                    listPhone.Remove(i.ToString());
                ListPhone = listPhone;
                using var sw = new StreamWriter("phone.txt", false, System.Text.Encoding.Default);
                foreach (var i in ListPhone)
                    sw.WriteLine(i);
            }
            catch (Exception ex)
            {
                dialogService.ShowMessage("Ошибка: " + ex.Message);
            }
        });
        #endregion

        #region Установка стандартных настроек фильтрации
        private RelayCommand _radioButton;
        public RelayCommand RadioButton => _radioButton ??= new RelayCommand(obj =>
        {
            try
            {
                StFilter = (bool)((obj as StackPanel).Children[0] as RadioButton).IsChecked;
            }
            catch (Exception ex)
            {
                dialogService.ShowMessage("Ошибка: " + ex.Message);
            }
        });
        #endregion
    }

    #region Класс сравнения StoryNumber
    public class StoryNumberClassComparer : IEqualityComparer<StoryNumber>
    {
        public bool Equals(StoryNumber x, StoryNumber y)
        {
            if (Object.ReferenceEquals(x, y)) return true;
            return x != null && y != null
                             && x.Phone.Equals(y.Phone)
                             && x.Name.Equals(y.Name)
                             && x.Direction.Equals(y.Direction)
                             && x.CalledCallerNumber.Equals(y.CalledCallerNumber)
                             && x.DateStartTime.Equals(y.DateStartTime)
                             && x.Duration.Equals(y.Duration)
                             && x.Coast.Equals(y.Coast);
        }
        public int GetHashCode(StoryNumber obj)
        {
            int hashPhone = obj.Phone == null ? 0 : obj.Phone.GetHashCode();
            int hashName = obj.Name == null ? 0 : obj.Name.GetHashCode();
            int hashDirection = obj.Direction == null ? 0 : obj.Direction.GetHashCode();
            int hashCalledCallerNumber = obj.CalledCallerNumber == null ? 0 : obj.CalledCallerNumber.GetHashCode();
            int hashDateStartTime = obj.DateStartTime == null ? 0 : obj.DateStartTime.GetHashCode();
            int hashDuration = obj.Duration == null ? 0 : obj.Duration.GetHashCode();
            int hashCoast = obj.Coast == null ? 0 : obj.Coast.GetHashCode();
            return hashPhone ^ hashName ^ hashDirection ^ hashCalledCallerNumber ^ hashDateStartTime ^ hashDuration ^ hashCoast;
        }
    }
    #endregion

}
