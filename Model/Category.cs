using MongoDB.Bson;

namespace Database_QuizConfigurator.Model;

class Category
{
    public ObjectId Id { get; set; }

    public string Name { get; set; }

    public Category(string name)
    {
        Name = name;
        Id = ObjectId.GenerateNewId();
    }
}
