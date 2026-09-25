using Volo.Abp.Authorization.Permissions;

namespace TriDict.Permissions;

public sealed class TriDictPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(TriDictPermissions.GroupName);
        var concepts = group.AddPermission(TriDictPermissions.Concepts);
        concepts.AddChild(TriDictPermissions.ConceptsCreate);
        concepts.AddChild(TriDictPermissions.ConceptsEdit);
        concepts.AddChild(TriDictPermissions.ConceptsReview);
        concepts.AddChild(TriDictPermissions.ConceptsPublish);
        var sources = group.AddPermission(TriDictPermissions.Sources);
        sources.AddChild(TriDictPermissions.SourcesManage);
        var imports = group.AddPermission(TriDictPermissions.Imports);
        imports.AddChild(TriDictPermissions.ImportsExecute);
        imports.AddChild(TriDictPermissions.ImportsView);
    }
}
