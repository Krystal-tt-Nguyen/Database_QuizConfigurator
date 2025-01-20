using Database_QuizConfigurator.Model;
using Laboration_3.Command;
using Laboration_3.Model;
using MongoDB.Driver;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Laboration_3.ViewModel;

internal class ConfigurationViewModel : ViewModelBase
{
    private readonly MainWindowViewModel? mainWindowViewModel;
    public QuestionPackViewModel? ActivePack { get => mainWindowViewModel.ActivePack; }
    private IMongoCollection<Category> CategoryCollection { get; set; }

    public ObservableCollection<Category> Categories { get; set; }

    //private Category category;

    public Category? Category
    {
        get => Categories.FirstOrDefault(c => c.Id == ActivePack?.Category?.Id);
        set
        {
            ActivePack.Category = value;
            RaisePropertyChanged();
        }
    }

    private Category selectedCategory;

    public Category SelectedCategory
    {
        get => selectedCategory; 
        set 
        { 
            selectedCategory = value;
            DeleteCategoryCommand.RaiseCanExecuteChanged();
            RaisePropertyChanged();
        }
    }
    private string categoryName;

    public string CategoryName
    {
        get => categoryName; 
        set 
        {
            categoryName = value;
            RaisePropertyChanged();
        }
    }

    private bool _deleteQuestionIsEnable;
    public bool DeleteQuestionIsEnable
    {
        get => _deleteQuestionIsEnable;
        set 
        {
            _deleteQuestionIsEnable = value; 
            RaisePropertyChanged();
        }
    }

    private bool _isConfigurationModeVisible;
    public bool IsConfigurationModeVisible
    {
        get => _isConfigurationModeVisible;
        set
        {
            _isConfigurationModeVisible = value;
            RaisePropertyChanged();
        }
    }

    private Question? _selectedQuestion;
    public Question? SelectedQuestion
    {
        get => _selectedQuestion;
        set
        {
            _selectedQuestion = value;
            DeleteQuestionCommand.RaiseCanExecuteChanged();
            RaisePropertyChanged();
            ChangeTextVisibility();
        }
    }

    private bool _textVisibility;
    public bool TextVisibility
    {
        get => _textVisibility;
        set 
        { 
            _textVisibility = value;
            RaisePropertyChanged();
        }
    }


    public event EventHandler EditPackOptionsRequested;
    public event EventHandler EditCategoriesRequested;
    
    public DelegateCommand AddQuestionCommand { get; }
    public DelegateCommand DeleteQuestionCommand { get; }
    public DelegateCommand EditPackOptionsCommand { get; }
    public DelegateCommand SwitchToConfigurationModeCommand { get; }
    public DelegateCommand AddCategoryCommand { get; }
    public DelegateCommand DeleteCategoryCommand { get; }
    public DelegateCommand EditCategoriesCommand { get; }

    public ConfigurationViewModel(MainWindowViewModel? mainWindowViewModel)
    {
        this.mainWindowViewModel = mainWindowViewModel;

        DeleteQuestionIsEnable = false;
        IsConfigurationModeVisible = true;
        
        AddQuestionCommand = new DelegateCommand(AddQuestion, IsAddQuestionEnable); 
        DeleteQuestionCommand = new DelegateCommand(DeleteQuestion, IsDeleteQuestionEnable);
        EditPackOptionsCommand = new DelegateCommand(EditPackOptions, IsEditPackOptionsEnable);
        SwitchToConfigurationModeCommand = new DelegateCommand(StartConfigurationMode, IsStartConfigurationModeEnable);
        AddCategoryCommand = new DelegateCommand(AddCategory);
        DeleteCategoryCommand = new DelegateCommand(DeleteCategory, DeleteCategoryActive);
        EditCategoriesCommand = new DelegateCommand(EditCategories);

        SelectedQuestion = ActivePack?.Questions.FirstOrDefault();
        TextVisibility = ActivePack?.Questions.Count > 0;

        Categories = new ObservableCollection<Category>(); 
        ConnectForCategory();

        var allCategories = CategoryCollection.Find(c => true).ToList();
        foreach (var category in allCategories)
        {
            Categories.Add(category);
        }
    }

    private void EditCategories(object obj)
    {
        EditCategoriesRequested.Invoke(this, EventArgs.Empty);
    }

    private bool DeleteCategoryActive(object? arg)
    {
        if (selectedCategory is not null) return true;
        return false;
    }

    private void DeleteCategory(object obj)
    {
        try
        {
            var filter = Builders<Category>.Filter.Eq(c => c.Id, SelectedCategory.Id);
            CategoryCollection.DeleteOne(filter);
            Categories.Remove(selectedCategory);
        }
        catch (Exception e)
        {
            Debug.WriteLine($"{e}");
        }
    }

    private void AddCategory(object obj)
    {
        var newCategory = new Category(CategoryName);
        Categories.Add(newCategory);
        CategoryName = string.Empty;
        CategoryCollection.InsertOne(newCategory);
    }

    private void AddQuestion(object? obj) 
    { 
        ActivePack?.Questions.Add(new Question("New Question", string.Empty, string.Empty, string.Empty, string.Empty));

        SelectedQuestion = (ActivePack?.Questions.Count > 0) 
            ? ActivePack?.Questions.Last() 
            : ActivePack?.Questions.FirstOrDefault();

        UpdateCommandStates();
        ChangeTextVisibility();
        mainWindowViewModel.SaveToMongoDbAsync();
    }

    private bool IsAddQuestionEnable(object? obj) => IsConfigurationModeVisible;

    private void DeleteQuestion(object? obj)
    { 
        ActivePack?.Questions.Remove(SelectedQuestion);
        UpdateCommandStates();
        ChangeTextVisibility();
        mainWindowViewModel.SaveToMongoDbAsync();
    }

    private bool IsDeleteQuestionEnable(object? obj)
        => IsConfigurationModeVisible && SelectedQuestion != null && (DeleteQuestionIsEnable = (ActivePack != null && ActivePack?.Questions.Count > 0));

    private void EditPackOptions(object? obj)  
    {
        EditPackOptionsRequested.Invoke(this, EventArgs.Empty);
        mainWindowViewModel.SaveToMongoDbAsync();
    } 

    private bool IsEditPackOptionsEnable(object? obj) => IsConfigurationModeVisible;

    private void ChangeTextVisibility() 
        => TextVisibility = ActivePack?.Questions.Count > 0 && SelectedQuestion != null;

    private void StartConfigurationMode(object? obj)
    {
        mainWindowViewModel.PlayerViewModel._timer.Stop();

        IsConfigurationModeVisible = true;
        mainWindowViewModel.PlayerViewModel.IsPlayerModeVisible = false;
        mainWindowViewModel.PlayerViewModel.IsResultModeVisible = false;
        UpdateCommandStates();
    }

    private bool IsStartConfigurationModeEnable(object? obj) => IsConfigurationModeVisible ? false : true;

    private void UpdateCommandStates()
    {
        AddQuestionCommand.RaiseCanExecuteChanged();
        DeleteQuestionCommand.RaiseCanExecuteChanged();
        EditPackOptionsCommand.RaiseCanExecuteChanged();
        mainWindowViewModel.PlayerViewModel.SwitchToPlayModeCommand.RaiseCanExecuteChanged();
    }

    private void ConnectForCategory()
    {
        var connectionString = "mongodb://localhost:27017/";

        var client = new MongoClient(connectionString);

        CategoryCollection = client.GetDatabase("Krystal_Lovisa").GetCollection<Category>("Categories");
    }

}