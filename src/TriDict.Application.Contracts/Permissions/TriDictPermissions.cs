namespace TriDict.Permissions;

public static class TriDictPermissions
{
    public const string GroupName = "TriDict";
    public const string Concepts = GroupName + ".Concepts";
    public const string ConceptsCreate = Concepts + ".Create";
    public const string ConceptsEdit = Concepts + ".Edit";
    public const string ConceptsReview = Concepts + ".Review";
    public const string ConceptsPublish = Concepts + ".Publish";
    public const string Sources = GroupName + ".Sources";
    public const string SourcesManage = Sources + ".Manage";
    public const string Imports = GroupName + ".Imports";
    public const string ImportsExecute = Imports + ".Execute";
    public const string ImportsView = Imports + ".View";
}
