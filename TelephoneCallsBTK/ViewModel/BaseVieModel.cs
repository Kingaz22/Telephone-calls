using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TelephoneCallsBTK.ViewModel
{
    public abstract class BaseVieModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



    }

}
