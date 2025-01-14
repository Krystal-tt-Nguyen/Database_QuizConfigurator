
using MongoDB.Bson;
using System.Collections.ObjectModel;

namespace Laboration_3.Model;

enum Difficulty { Easy, Medium, Hard }

internal class QuestionPack
{
    public ObjectId Id { get; set; }
    public string Name { get; set; }
    public Difficulty Difficulty { get; set; }
    public int TimeLimitInSeconds { get; set; }
    public ObservableCollection<Question> Questions { get; set; }
    public string? Category { get; set; }

    public QuestionPack(string name = "<PackName>", string? category = null, Difficulty difficulty = Difficulty.Medium, int timeLimitInSeconds = 30, ObjectId id = new ObjectId())
    {
        Id = id = ObjectId.GenerateNewId();
        Name = name;
        Category = category;
        Difficulty = difficulty;
        TimeLimitInSeconds = timeLimitInSeconds;
        Questions = new ObservableCollection<Question>();
    }
}
