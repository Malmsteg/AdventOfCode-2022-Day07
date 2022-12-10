const int PART1LIMIT = 100000;
const int PART2SIZE = 30000000;
const int PART2TOTALSIZE = 70000000;

{
    string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
    string file = Path.Combine(currentDirectory, "../../../input.txt");
    string path = Path.GetFullPath(file);
    string[] text = File.ReadAllText(path).Split("\n");

    FileStructure root = new() { files = new(), name = "/", size = 0, isDir = true, relativeName = "/" };
    FileStructure currentDir = root;
    FileStructure parentDir = root;

    for (int i = 0; i < text.Length; i++)
    {
        string[] line = text[i].Split();

        switch (line[0])
        {
            case "$":
                // new command
                switch (line[1])
                {
                    case "ls":
                        // list current directory
                        for (int j = i + 1; j < text.Length; j++)
                        {
                            string[] jLine = text[j].Split();
                            if (jLine[0].Equals("$"))
                            {
                                i = j - 1;
                                break; // new command, exit loop
                            }
                            else if (jLine[0].Equals("dir")) //directory
                            {
                                FileStructure newDir = new() { files = new(), name = jLine[1], size = 0, isDir = true, relativeName = currentDir.relativeName + jLine[1] };
                                currentDir.files.Add(newDir);
                            }
                            else // file
                            {
                                int size = Convert.ToInt32(jLine[0]);
                                currentDir.files.Add(new() { name = jLine[1], size = size, isDir = false, relativeName = currentDir.relativeName + jLine[1] });
                                currentDir.size += size;
                            }
                        }
                        break;
                    case "cd":
                        // change directory
                        switch (line[2])
                        {
                            case "..":
                                // move up one directory
                                currentDir = parentDir;
                                if (!parentDir.name.Equals(root.name))
                                {
                                    parentDir = FindParentFileStructure(root, parentDir)[0];
                                }
                                break;
                            case "/":
                                break;
                            default:
                                parentDir = currentDir;
                                currentDir = currentDir.files.First(x => x.name.Equals(line[2]));
                                break;
                        }
                        break;
                }
                break;
            default:
                // output from last command
                break;
        }
    }
    root = SumDirectorySizes(root);
    int result = CalculateAssignmentSizes(root, PART1LIMIT);
    int part2 = PART2SIZE - (PART2TOTALSIZE - root.size);
    var resultPart2 = CalculateAssignmentSizesPart2(root, part2).Min();
    Console.WriteLine($"Result is {result}");
    Console.WriteLine($"Part 2 result is {resultPart2}");
}

static List<FileStructure> FindParentFileStructure(FileStructure root, FileStructure current)
{
    List<FileStructure> result = new();
    var temp = root.files.Find(x =>
    x.relativeName.Equals(current.relativeName)
    && x.files.Count == current.files.Count);
    if (temp.isDir)
    {
        return new List<FileStructure>() { root };
    }
    else
    {
        foreach (var file in root.files)
        {
            if (file.isDir)
            {
                result.AddRange(FindParentFileStructure(file, current));
            }
        }
    }
    return result;
}

static FileStructure SumDirectorySizes(FileStructure root)
{
    if (root.files is null)
    {
        return root;
    }
    for (int i = 0; i < root.files.Count; i++)
    {
        if (root.files[i].files is not null)
        {
            root.files[i] = SumDirectorySizes(root.files[i]);
        }
        root.size += root.files[i].size;
    }
    return root;
}

static int CalculateAssignmentSizes(FileStructure root, int limit)
{
    int result = 0;
    foreach (var file in root.files)
    {
        if (file.files is not null)
        {
            if (file.size <= limit)
            { result += file.size; }
            result += CalculateAssignmentSizes(file, limit);
        }
    }
    return result;
}

static List<int> CalculateAssignmentSizesPart2(FileStructure root, int limit)
{
    List<int> resultArray = new();
    if (root.size >= limit)
    {
        resultArray.Add(root.size);
    }
    foreach (var file in root.files)
    {
        if (file.files is not null)
        {
            if (file.size >= limit)
            {
                resultArray.Add(file.size);
            }
            resultArray.AddRange(CalculateAssignmentSizesPart2(file, limit));
        }
    }
    return resultArray;
}

struct FileStructure
{
    public string name;
    public int size;
    public List<FileStructure> files;
    public bool isDir;
    public string relativeName;
};