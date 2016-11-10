using System.Security;
using System.Security.Permissions;

namespace DataDrain.Library.Helpers
{
    public static class RegistryHelper
    {
        public static bool HavePermissionsOnKey(this RegistryPermission reg, RegistryPermissionAccess accessLevel, string key)
        {
            try
            {
                var r = new RegistryPermission(accessLevel, key);
                r.Demand();
                return true;
            }
            catch (SecurityException)
            {
                return false;
            }
        }

        public static bool CanWriteKey(this RegistryPermission reg, string key)
        {
            try
            {
                var r = new RegistryPermission(RegistryPermissionAccess.Write, key);
                r.Demand();
                return true;
            }
            catch (SecurityException)
            {
                return false;
            }
        }

        public static bool CanReadKey(this RegistryPermission reg, string key)
        {
            try
            {
                var r = new RegistryPermission(RegistryPermissionAccess.Read, key);
                r.Demand();
                return true;
            }
            catch (SecurityException)
            {
                return false;
            }
        }
    }
}
