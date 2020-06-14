using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using TelephoneCallsBTK.Model;
using TelephoneCallsBTK.Window;

namespace TelephoneCallsBTK.ViewModel
{
    public class ApplicationViewModel : BaseVieModel
    {
        IFileService fileService;
        IDialogService dialogService;
        
        private IEnumerable<ReportNumber> _reportNumbers;
        /// <summary>
        /// Отчет
        /// </summary>
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
        /// <summary>
        /// Количество загруженных строк
        /// </summary>
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
        /// <summary>
        /// Первоначальные данные из файла
        /// </summary>
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
        /// <summary>
        /// Отфильтрованный список
        /// </summary>
        public IEnumerable<StoryNumber> StoryNumbers
        {
            get => _storyNumbers;
            set
            {
                _storyNumbers = value;
                OnPropertyChanged(nameof(StoryNumbers));
            }
        }
        
        private IEnumerable<string> _listPhone;
        /// <summary>
        /// Список телефонов
        /// </summary>
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
            List<string> listPhone = new List<string>();
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
                    ReportNumbers = MyFunc.ReportNumbers(ListPhone, StoryNumbersFirst);
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
                    StoryNumbersFirst = fileService.Open(dialogService.FilePath)
                        .Union(StoryNumbersFirst, new StoryNumberClassComparer())
                        .Where(x => x.Phone != "Телефон")
                        .Where(x => x.Name != "Итого сумма начислений по абонентскому номеру:");
                    
                    StoryNumbers = MyFunc.FirstSort(ListPhone, StoryNumbersFirst);

                    CountNumbers = StoryNumbers.Count();

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
            Process.Start((Environment.CurrentDirectory + "\\App_Data\\help.chm").Replace("\\", "/"));
        });
        #endregion

        #region Открыть окно "О программе"
        private RelayCommand _openAbout;
        public RelayCommand OpenAbout => _openAbout ??= new RelayCommand(obj =>
        {
            About about = new About();
            about.Show();
        });
        #endregion
        
        #region Очиcтка данных
        private RelayCommand _clearData;
        public RelayCommand ClearData => _clearData ??= new RelayCommand(obj =>
        {
            try
            {
                StoryNumbers = new List<StoryNumber>();
                ReportNumbers = new List<ReportNumber>();
                StoryNumbersFirst = new List<StoryNumber>();
                CountNumbers = 0;
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

                        StoryNumbers = MyFunc.FirstSort(ListPhone, StoryNumbersFirst);
                        CountNumbers = StoryNumbers.Count();
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
                StoryNumbers = MyFunc.FirstSort(ListPhone, StoryNumbersFirst);
                CountNumbers = StoryNumbers.Count();
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
