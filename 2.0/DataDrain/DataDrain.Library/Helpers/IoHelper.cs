using System.Security;
using System.Security.Permissions;

namespace DataDrain.Library.Helpers
{
    public static class IoHelper
    {
        public static bool VerifyVermission(FileIOPermissionAccess permission, string path)
        {
            var ioPermission = new FileIOPermission(permission, path);

            try
            {
                ioPermission.Demand();
                return true;
            }
            catch (SecurityException s)
            {
                return false;
            }
        }
    }
}
