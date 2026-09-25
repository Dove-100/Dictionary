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
    }
}
