using System.Xml.Linq;

namespace Campfire.ArchitectureTests;

public sealed class DependencyTests
{
    [Fact]
    public void Domain_projects_have_no_package_or_project_dependencies()
    {
        var root = RepositoryRoot.Find();
        var projects = Directory.GetFiles(Path.Combine(root, "backend", "src", "Modules"), "*.Domain.csproj", SearchOption.AllDirectories);

        Assert.NotEmpty(projects);
        foreach (var project in projects)
        {
            var document = XDocument.Load(project);
            Assert.Empty(document.Descendants("PackageReference"));
            Assert.Empty(document.Descendants("ProjectReference"));
        }
    }

    [Fact]
    public void Application_projects_do_not_reference_infrastructure_or_presentation()
    {
        var root = RepositoryRoot.Find();
        var projects = Directory.GetFiles(Path.Combine(root, "backend", "src", "Modules"), "*.Application.csproj", SearchOption.AllDirectories);

        Assert.NotEmpty(projects);
        foreach (var project in projects)
        {
            var references = XDocument.Load(project).Descendants("ProjectReference")
                .Select(reference => reference.Attribute("Include")?.Value ?? string.Empty);
            Assert.DoesNotContain(references, reference =>
                reference.Contains("Infrastructure", StringComparison.Ordinal) ||
                reference.Contains("Presentation", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void Modules_only_reference_other_modules_through_contracts()
    {
        var root = RepositoryRoot.Find();
        var projects = Directory.GetFiles(Path.Combine(root, "backend", "src", "Modules"), "*.csproj", SearchOption.AllDirectories);

        foreach (var project in projects)
        {
            var projectDirectory = new FileInfo(project).Directory!;
            var module = projectDirectory.Parent!.Name;
            var references = XDocument.Load(project).Descendants("ProjectReference")
                .Select(reference => reference.Attribute("Include")?.Value ?? string.Empty)
                .Select(reference => new FileInfo(Path.GetFullPath(reference, projectDirectory.FullName)));

            Assert.DoesNotContain(references, reference =>
                reference.Directory?.Parent?.Name is { } referencedModule &&
                !string.Equals(referencedModule, module, StringComparison.OrdinalIgnoreCase) &&
                !reference.Name.EndsWith(".Contracts.csproj", StringComparison.OrdinalIgnoreCase) &&
                reference.FullName.Contains($"{Path.DirectorySeparatorChar}Modules{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public void Application_and_presentation_projects_reference_mediatr()
    {
        var root = RepositoryRoot.Find();
        var projects = Directory.GetFiles(
            Path.Combine(root, "backend", "src", "Modules"),
            "*.csproj",
            SearchOption.AllDirectories)
            .Where(project => project.EndsWith(".Application.csproj", StringComparison.OrdinalIgnoreCase) ||
                              project.EndsWith(".Presentation.csproj", StringComparison.OrdinalIgnoreCase));

        Assert.NotEmpty(projects);
        foreach (var project in projects)
        {
            var packages = XDocument.Load(project).Descendants("PackageReference")
                .Select(reference => reference.Attribute("Include")?.Value ?? string.Empty);
            Assert.Contains("MediatR", packages);
        }
    }
}
