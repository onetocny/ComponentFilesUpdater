// See https://aka.ms/new-console-template for more information

using System.Xml.Linq;

const string root = "D:\\ado\\src";
const string module = "module://Vssf.Sdk/Microsoft.TeamFoundation.Framework.Server.dll";

var nugets = new List<string>
{
    "nuget://Autofac/lib/net45/Autofac.dll",
    "nuget://Autofac.WebApi2/lib/net45/Autofac.Integration.WebApi.dll"
};

var xmls = Directory.GetFiles(root, "*.xml", SearchOption.AllDirectories).Select(f => new
{
    Path = f,
    Document = Load(f)
}).Where(x => x.Document != null).ToList();

var filesCount = 0;
var dirCount = 0;

foreach (var xml in xmls)
{
    var document = xml.Document;
    var component = document.Element("Component");
    if (component == null)
    {
        continue;
    }

    var directories = component
        .Descendants("Directory")
        .Where(d => d.Attributes().Any(a => a.Name == "Path" && !a.Value.Contains("Plugins")))
        .Where(d => d.Descendants("File").Any(d => d.Attributes().Any(a => a.Name == "Origin" && a.Value == module)))
        .ToList();

    foreach (var directory in directories)
    {
        dirCount++;
        foreach (var nuget in nugets)
        {
            var element = new XElement("File");
            element.SetAttributeValue("Origin", nuget);
            directory.Add(element);
        }
    }


    if (directories.Any())
    {
        Console.WriteLine($"Saving {xml.Path}");
        document.Save(xml.Path);
        filesCount++;
    }
}

Console.WriteLine($"Updated {filesCount} files at {dirCount} places");


Console.ReadLine();


XDocument Load(string path)
{
    try
    {
        return XDocument.Load(path);
    }
    catch
    {
        Console.WriteLine($"Error while parsing {path}");
    }
    return null;
}