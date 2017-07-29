using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Labirynth.Annotations;

namespace Labirynth
{
    public class Square : INotifyPropertyChanged
    {
        private bool _starting;
        private bool _ending;
        public bool Checked { get; set; }

        public bool Wall;

        public bool Starting
        {
            get { return _starting; }
            set
            {
                _starting = value;
                OnPropertyChanged();
            }
        }

        public bool Ending
        {
            get { return _ending; }
            set
            {
                _ending = value;
                OnPropertyChanged();
            }
        }

        public string Tag { get; set; }

        private SolidColorBrush _filling;

        public SolidColorBrush Filling
        {
            get { return _filling; }
            set
            {
                _filling = value;
                OnPropertyChanged();
            }
        }

        public Square(bool starting, bool ending, bool wall, string tag)
        {
            _starting = starting;
            _ending = ending;
            Wall = wall;
            Tag = tag;
            Filling = new SolidColorBrush(Colors.CadetBlue);
        }

        public Square(bool wall, string tag)
        {
            if (wall)
            {
                Wall = true;
                Filling = new SolidColorBrush(Colors.Black);
            }
            Tag = tag;
        }

        public Square(string tag)
        {
            Filling = new SolidColorBrush(Colors.CadetBlue);
            Tag = tag;
        }

        public Square()
        {
            
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}