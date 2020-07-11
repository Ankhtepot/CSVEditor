using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace CSVEditor.ViewModel
{
    public class GitVM : INotifyPropertyChanged
    {
        private IRepository currentRepository;

        public IRepository CurrentRepository
        {
            get { return currentRepository; }
            set 
            {
                currentRepository = value;
                OnPropertyChanged();
            }
        }

        public void SetRepository(string path)
        {
            try
            {
                CurrentRepository = new Repository(path);
            }
            catch (RepositoryNotFoundException e)
            {
                Console.WriteLine($"Path \"{path}\" doesn't contain valid repository. Error: {e.Message}");
            }
        }

        public bool IsUnstaged()
        {
            return CurrentRepository?.RetrieveStatus(new StatusOptions()).Count() > 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
