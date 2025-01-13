using Laboration_3.Command;
using Laboration_3.Model;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using Path = System.IO.Path;

namespace Laboration_3.ViewModel
{
    internal class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<QuestionPackViewModel> Packs { get; set; }
        public ConfigurationViewModel ConfigurationViewModel { get; }
        public PlayerViewModel PlayerViewModel { get; }
        public string FilePath { get; set; }


        private bool _canExit;
        public bool CanExit
        {
            get => _canExit;
            set
            {
                _canExit = value;
                RaisePropertyChanged();

            }
        }

        private bool _deletePackIsEnable;
        public bool DeletePackIsEnable
        {
            get => _deletePackIsEnable;
            set
            {
                _deletePackIsEnable = value;
                RaisePropertyChanged();
            }
        }

        private bool _isFullscreen;
        public bool IsFullscreen
        {
            get => _isFullscreen;
            set
            {
                _isFullscreen = value;
                RaisePropertyChanged();
            }
        }


        private QuestionPackViewModel? _activePack;
        public QuestionPackViewModel? ActivePack
        {
            get => _activePack;
            set
            {
                _activePack = value;
                RaisePropertyChanged();
                ConfigurationViewModel?.RaisePropertyChanged();
            }
        }

        private QuestionPackViewModel? _newPack;
        public QuestionPackViewModel? NewPack
        {
            get => _newPack;
            set
            {
                _newPack = value;
                RaisePropertyChanged();
            }
        }

        private QuestionPackViewModel? _selectedPack;
        public QuestionPackViewModel? SelectedPack
        {
            get => _selectedPack;
            set
            {
                _selectedPack = value;
                RaisePropertyChanged();
            }
        }


        public event EventHandler CloseDialogRequested; 
        public event EventHandler DeletePackRequested;
        public event EventHandler<bool> ExitGameRequested;
        public event EventHandler OpenNewPackDialogRequested; 
        public event EventHandler<bool> ToggleFullScreenRequested;

        public DelegateCommand CloseDialogCommand { get; }
        public DelegateCommand CreateNewPackCommand { get; } 
        public DelegateCommand DeletePackCommand { get; }
        public DelegateCommand ExitGameCommand { get; }
        public DelegateCommand OpenDialogCommand { get; } 
        public DelegateCommand SaveOnShortcutCommand { get; }
        public DelegateCommand SelectActivePackCommand { get; }
        public DelegateCommand ToggleWindowFullScreenCommand { get; }


        public MainWindowViewModel()
        {
            CanExit = false;
            DeletePackIsEnable = true;
            IsFullscreen = false;

            FilePath = GetFilePath();
            InitializeDataAsync();

            ConfigurationViewModel = new ConfigurationViewModel(this);
            PlayerViewModel = new PlayerViewModel(this);

            CloseDialogCommand = new DelegateCommand(ClosePackDialog);
            OpenDialogCommand = new DelegateCommand(OpenPackDialog); 

            CreateNewPackCommand = new DelegateCommand(CreateNewPack);
            DeletePackCommand = new DelegateCommand(RequestDeletePack, IsDeletePackEnable);

            SaveOnShortcutCommand = new DelegateCommand(SaveOnShortcut);
            SelectActivePackCommand = new DelegateCommand(SelectActivePack);
            ToggleWindowFullScreenCommand = new DelegateCommand(ToggleWindowFullScreen);
            ExitGameCommand = new DelegateCommand(ExitGame);
        }

        private void OpenPackDialog(object? obj) 
        {
            NewPack = new QuestionPackViewModel(new QuestionPack());
            OpenNewPackDialogRequested.Invoke(this, EventArgs.Empty);
        }

        private void ClosePackDialog(object? obj) => CloseDialogRequested.Invoke(this, EventArgs.Empty); 

        private void CreateNewPack(object? obj)
        {
            if (NewPack != null)
            {
                Packs.Add(NewPack);
                ActivePack = NewPack;

                ConfigurationViewModel.DeleteQuestionCommand.RaiseCanExecuteChanged();
                DeletePackCommand.RaiseCanExecuteChanged();
                SaveToJsonAsync();
            }

            CloseDialogRequested.Invoke(this, EventArgs.Empty);
        }

        private void RequestDeletePack(object? obj) => DeletePackRequested?.Invoke(this, EventArgs.Empty);
    
        public void DeletePack()
        {
            Packs.Remove(ActivePack);
            DeletePackCommand.RaiseCanExecuteChanged();

            if (Packs.Count > 0)
            {
                ActivePack = Packs.FirstOrDefault();
            }
            SaveToJsonAsync();
        }

        private bool IsDeletePackEnable(object? obj) => Packs != null && Packs.Count > 1;

        private void SelectActivePack(object? obj)
        {
            if (obj is QuestionPackViewModel selectedPack)
            {
                SelectedPack = selectedPack;
                ActivePack = SelectedPack;
                SaveToJsonAsync();
            }
        }

        private void ToggleWindowFullScreen(object? obj)
        {
            IsFullscreen = !IsFullscreen;
            ToggleFullScreenRequested?.Invoke(this, _isFullscreen);
        }

        private async void ExitGame(object? obj)
        {
            await SaveToJsonAsync();

            CanExit = true;
            ExitGameRequested?.Invoke(this, CanExit);
        }

        private string GetFilePath()
        {
            string appDataFilePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string directoryFilePath = Path.Combine(appDataFilePath, "Laboration_3");

            if (!Directory.Exists(directoryFilePath))
            {
                Directory.CreateDirectory(directoryFilePath);
            }

            string filePath = Path.Combine(directoryFilePath, "Laboration_3.json");
            return filePath;
        }

        private async Task InitializeDataAsync()
        {
            Packs = new ObservableCollection<QuestionPackViewModel>();

            if (Path.Exists(FilePath))
            {
                await ReadFromJsonAsync();
                ActivePack = Packs?.FirstOrDefault();
            }
            else
            {
                ActivePack = new QuestionPackViewModel(new QuestionPack("Default Question Pack"));
                Packs.Add(ActivePack);
            }
        }

        public async Task SaveToJsonAsync()
        {
            var options = new JsonSerializerOptions()
            {
                IncludeFields = true,
                IgnoreReadOnlyProperties = false,
            };

            string jsonString = JsonSerializer.Serialize(Packs, options);
            await File.WriteAllTextAsync(FilePath, jsonString);
        }

        private async Task ReadFromJsonAsync()
        {
            string jsonString = await File.ReadAllTextAsync(FilePath);
            var questionPack = JsonSerializer.Deserialize<QuestionPack[]>(jsonString);

            foreach (var pack in questionPack)
            {
                Packs.Add(new QuestionPackViewModel(pack));
            }
        }

        private void SaveOnShortcut(object? obj) => SaveToJsonAsync();

    }
}