using System.Text.Json;

public class Todo
{
  public required string ID { get; set; }
  public required string Description { get; set; }
  public string Status { get; set; } = "todo";
  public DateTime UpdatedAt { get; set; }
  public DateTime CreatedAt { get; set; }
}

class Program
{
  public static Todo[] LoadTodos(string filePath)
  {
    try
    {
      if (File.Exists(filePath))
      {
        string json = File.ReadAllText(filePath);
        if (!string.IsNullOrWhiteSpace(json))
        {
          return JsonSerializer.Deserialize<Todo[]>(json) ?? [];
        }
      }
      return [];
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error loading todos: {ex.Message}");
      return [];
    }
  }

  public static void SaveTodos(Todo[] todos, string filePath)
  {
    try
    {
      string directory = Path.GetDirectoryName(filePath) ?? "";
      if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
      {
        Directory.CreateDirectory(directory);
      }

      var options = new JsonSerializerOptions { WriteIndented = true };
      string json = JsonSerializer.Serialize(todos, options);
      File.WriteAllText(filePath, json);
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error saving todos: {ex.Message}");
    }
  }

  private static void AddToDo(string subcommand, List<Todo> result)
  {
    if (subcommand == null)
    {
      Console.WriteLine("need the list");
      return;
    }
    else
    {
      var maxId = result.Count > 0 ? result.Max(t => int.Parse(t.ID)) : 0;
      var toDo = new Todo
      {
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        ID = (maxId + 1).ToString(),
        Description = subcommand,
      };
      result.Add(toDo);
      Console.WriteLine($"Task added successfully (ID:) {result.Count}");
    }
  }

  private static void UpdateToDo(string subcommand, string extra, List<Todo> result)
  {
    bool isMatch = false;
    foreach (Todo td in result)
    {
      if (td.ID == subcommand)
      {
        string? newToDo = extra;
        if (newToDo == null)
        {
          Console.WriteLine("need a new updated todo");
        }
        else
        {
          td.Description = newToDo;
          td.UpdatedAt = DateTime.UtcNow;
          Console.WriteLine($"Todo {td.ID} has been updated");
        }
        isMatch = true;
        break;
      }
    }
    if (!isMatch)
    {
      Console.WriteLine("no id match with the todo list");
    }
  }

  private static void DeleteToDo(string subcommand, List<Todo> result)
  {
    result.RemoveAll(td => td.ID == subcommand);
    Console.WriteLine("deleted");
  }

  private static void MarkTodo(string command, string subcommand, List<Todo> result)
  {
    bool isMatch = false;
    foreach (Todo td in result)
    {
      if (td.ID == subcommand)
      {
        td.Status = command == "mark-in-progress" ? "in-progress" : "done";
        td.UpdatedAt = DateTime.UtcNow;
        Console.WriteLine("status updated");
        isMatch = true;
        break;
      }
    }
    if (!isMatch)
    {
      Console.WriteLine("can not find that id");
    }
  }

  static void Main(string[] args)
  {
    List<Todo> result = [.. LoadTodos("file.json")];

    string command = args[0];
    string? subcommand = args.Length > 1 ? args[1] : null;
    string? extra = args.Length > 2 ? args[2] : null;

    if (subcommand != null)
    {
      if (command == "add") AddToDo(subcommand, result);
      else if (command == "update")
      {
        if (subcommand != null && extra != null) UpdateToDo(subcommand!, extra!, result);
      }
      else if (command == "delete") DeleteToDo(subcommand, result);
      else if (command == "mark-in-progress" || command == "mark-done") MarkTodo(command, subcommand, result);
      else if (command == "list")
      {
        foreach (Todo td in result)
        {
          if (td.Status == subcommand || subcommand == null)
          {
            Console.WriteLine($"todo {td.ID}, {td.Description}, status: {td.Status}, last updated at {td.UpdatedAt}");
          }
        }
      }
    }

    else
    {
      Console.WriteLine("not a valid command");
    }

    SaveTodos([.. result], "file.json");
  }
}