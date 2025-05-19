using ESG.ViewModels;

namespace ESG.Utilities
{
    public static class PermissionChecker
    {
        public static bool HasPermission(string permissionName)
        {
            var currentUser = UsersViewModel.CurrentUser;
            return currentUser?.Permissions?.Any(p => p.PermissionName == permissionName) ?? false;
        }

        public static bool CanManageUsers() => HasPermission("ManageAll");
        public static bool CanPerformCrud() => HasPermission("ManageAll") || HasPermission("UploadData");
        public static bool CanViewOnly() => HasPermission("ViewOnly");
    }
}