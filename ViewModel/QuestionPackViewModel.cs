using Laboration_3.Model;
using MongoDB.Bson;
using System.Collections.ObjectModel;

namespace Laboration_3.ViewModel
{
    internal class QuestionPackViewModel : ViewModelBase
    {
        private readonly QuestionPack model;

        public string Name 
        { 
            get => model.Name;
            set
            {
                model.Name = value;
                RaisePropertyChanged();
            }
        }

        public Difficulty Difficulty
        {
            get => model.Difficulty;
            set
            {
                model.Difficulty = value;
                RaisePropertyChanged();
            }
        }

        public int TimeLimitInSeconds
        {
            get => model.TimeLimitInSeconds;
            set
            {
                model.TimeLimitInSeconds = value;
                RaisePropertyChanged();
            }
        }

        private string? category;

        public string? Category
        {
            get => model.Category;
            set 
            {
                model.Category = value;
                RaisePropertyChanged();
            }
        }

        private ObjectId id;

        public ObjectId Id
        {
            get => model.Id; 
            set
            {
                model.Id = value;
                RaisePropertyChanged();
            }
        }


        public ObservableCollection<Question> Questions  { get; }

        public QuestionPackViewModel(QuestionPack model)
        {
            this.model = model;
            this.Questions = new ObservableCollection<Question>(model.Questions);
        }
        public QuestionPackViewModel(QuestionPackViewModel pack)
        {
            Id = pack.Id;
            Name = pack.Name;
            Category = pack.Category;
            TimeLimitInSeconds = pack.TimeLimitInSeconds;
            Questions = pack.Questions;
        }
    }
}
