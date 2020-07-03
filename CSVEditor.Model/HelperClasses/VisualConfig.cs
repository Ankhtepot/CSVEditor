using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CSVEditor.Model.HelperClasses
{
    public class VisualConfig : INotifyPropertyChanged
    {
        private const double MIN_MAIN_WINDOW_WIDTH = 600;
        private const double MIN_MAIN_WINDOW_HIGHT = 400;
        private const double DEFAULT_MAIN_WINDOW_WIDTH = 1000;
        private const double DEFAULT_MAIN_WINDOW_HEIGHT = 450;

        private double mainWindowWidth;
        public double MainWindowWidth
        {
            get { return mainWindowWidth; }
            set { mainWindowWidth = value; OnPropertyChanged(); }
        }

        private double mainWindowHeight;
        public double MainWindowHeight
        {
            get { return mainWindowHeight; }
            set { mainWindowHeight = value; OnPropertyChanged(); }
        }

        public VisualConfig() : this(DEFAULT_MAIN_WINDOW_WIDTH, DEFAULT_MAIN_WINDOW_HEIGHT) { }

        public VisualConfig(double mainWindowWidth, double mainWindowHeight)
        {
            MainWindowWidth = Math.Max(mainWindowWidth, MIN_MAIN_WINDOW_WIDTH);
            MainWindowHeight = Math.Max(mainWindowHeight, MIN_MAIN_WINDOW_HIGHT);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
