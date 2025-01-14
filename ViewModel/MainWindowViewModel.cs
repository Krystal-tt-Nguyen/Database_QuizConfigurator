using Laboration_3.Command;
using Laboration_3.Model;
using MongoDB.Driver;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Laboration_3.ViewModel
{
    internal class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<QuestionPackViewModel> Packs { get; set; }
        public ConfigurationViewModel ConfigurationViewModel { get; }
        public PlayerViewModel PlayerViewModel { get; }
        public QuestionPackViewModel ActivePacksCopy { get; set; }
        public IMongoCollection<QuestionPackViewModel> QuestionCollection { get; set; }


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

            QuestionCollection = ConnectToLocalHost();
            InitializeData();

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
                SaveToMongoDbAsync();
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
            SaveToMongoDbAsync();
        }

        private bool IsDeletePackEnable(object? obj) => Packs != null && Packs.Count > 1;

        private void SelectActivePack(object? obj)
        {
            if (obj is QuestionPackViewModel selectedPack)
            {
                SelectedPack = selectedPack;
                ActivePack = SelectedPack;
                SaveToMongoDbAsync();
            }
        }

        private void ToggleWindowFullScreen(object? obj)
        {
            IsFullscreen = !IsFullscreen;
            ToggleFullScreenRequested?.Invoke(this, _isFullscreen);
        }

        private void ExitGame(object? obj)
        {
            SaveToMongoDbAsync();

            CanExit = true;
            ExitGameRequested?.Invoke(this, CanExit);
        }

        private IMongoCollection<QuestionPackViewModel> ConnectToLocalHost()
        {
            var connectionString = "mongodb://localhost:27017/";

            var client = new MongoClient(connectionString);

            return client.GetDatabase("Krystal_Lovisa").GetCollection<QuestionPackViewModel>("QuestionPacks");
        }

        private void InitializeData()
        {
            Packs = new ObservableCollection<QuestionPackViewModel>();

            var filter = Builders<QuestionPackViewModel>.Filter.Exists("_id", true);
            
            var questionPacksExist = QuestionCollection.Find(filter).FirstOrDefault();


            try
            {
                if (questionPacksExist is not null)
                {
                    GetCollection();
                    ActivePack = Packs?.FirstOrDefault();
                }
                else
                {
                    ActivePack = new QuestionPackViewModel(new QuestionPack("Default Question Pack"));
                    Packs.Add(ActivePack);
                }
                QuestionCollection.InsertMany(Packs);
                ActivePacksCopy = MakeQuestionPackViewModelCopy(ActivePack);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e.Message}");
            }
        }

        public QuestionPackViewModel MakeQuestionPackViewModelCopy(QuestionPackViewModel model)
        {
            return new QuestionPackViewModel(new QuestionPack(model.Name, model.Category, model.Difficulty, model.TimeLimitInSeconds, model.Id));
        }

        public void SaveToMongoDbAsync()
        {
            //if (!ActivePacksCopy.Equals(ActivePack))
            //{
            //    foreach (var question in ActivePack.Questions)
            //    {
            //        var filter = Builders<QuestionPackViewModel>.Filter.Eq("_id", question.Id);
            //        var replacement = Builders<QuestionPackViewModel>.SetFields

            //        QuestionCollection.FindOneAndReplaceAsync(filter, );

            //        var toRemove = ActivePacksCopy.Questions.FirstOrDefault(q => q.Id == question.Id);
            //        ActivePacksCopy.Questions.Remove(toRemove);

            //        var toAdd = ActivePack.Questions.FirstOrDefault(q => q.Id == question.Id);
            //        ActivePacksCopy.Questions.Add(question);

            //    }
            //}

            if (ActivePack is not null && !ActivePacksCopy.Equals(ActivePack))
            {
                var filter = Builders<QuestionPackViewModel>.Filter.Eq("_id", ActivePack.Id);

                QuestionCollection.ReplaceOne(filter, ActivePack);

                ActivePacksCopy = MakeQuestionPackViewModelCopy(ActivePack);
            }
        }

        private void GetCollection()
        {
            var allQuestionPacks = QuestionCollection.Find(q => true).ToList();

            foreach (var pack in allQuestionPacks)
            {
                Packs.Add(pack);
            }            
        }

        private void SaveOnShortcut(object? obj) => SaveToMongoDbAsync();

    }
}