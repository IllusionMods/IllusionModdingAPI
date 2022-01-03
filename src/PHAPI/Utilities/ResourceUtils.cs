using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace KKAPI.Utilities
{
    /// <summary>
    /// Utility methods for working with embedded resources.
    /// </summary>
    public static class ResourceUtils
    {
        /// <summary>
        /// Read all bytes starting at current position and ending at the end of the stream.
        /// </summary>
        public static byte[] ReadAllBytes(this Stream input)
        {
            var buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                    ms.Write(buffer, 0, read);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Get a file set as "Embedded Resource" from the assembly that is calling this code, or optionally from a specified assembly.
        /// The filename is matched to the end of the resource path, no need to give the full path.
        /// If 0 or more than 1 resources match the provided filename, an exception is thrown.
        /// For example if you have a file "ProjectRoot\Resources\icon.png" set as "Embedded Resource", you can use this to load it by
        /// doing <code>GetEmbeddedResource("icon.png"), assuming that no other embedded files have the same name.</code>
        /// </summary>
        /// <exception cref="IOException">Thrown if none or more than one resources were found matching the given resourceFileName</exception>
        public static byte[] GetEmbeddedResource(string resourceFileName, Assembly containingAssembly = null)
        {
            var autodetectedAssembly = false;
            if (containingAssembly == null)
            {
                containingAssembly = Assembly.GetCallingAssembly();
                autodetectedAssembly = true;
            }

            var resourceNames = containingAssembly.GetManifestResourceNames().Where(str => str.EndsWith(resourceFileName)).Take(2).ToList();

            if (resourceNames.Count == 0 && autodetectedAssembly)
            {
                var origAssembly = containingAssembly;

                // Try to use StackFrame in case calling method was Harmony patched because then Assembly.GetCallingAssembly can return wrong assembly
                containingAssembly = new StackFrame(1).GetMethod().DeclaringType.Assembly;

                if (origAssembly != containingAssembly)
                {
                    resourceNames = containingAssembly.GetManifestResourceNames().Where(str => str.EndsWith(resourceFileName)).Take(2).ToList();
                    if (resourceNames.Count == 0) throw new IOException($"Could not find resource with name {resourceFileName} inside assembly {containingAssembly} or {origAssembly} - make sure the name and assembly are correct. Two assemblies were checked likely because your method has been harmony patched");
                }
            }

            if (resourceNames.Count == 0)
                throw new IOException($"Could not find resource with name {resourceFileName} inside assembly {containingAssembly} - make sure the name and assembly are correct");

            if (resourceNames.Count == 2)
                throw new IOException($"Found more than one resource with name {resourceFileName} inside assembly {containingAssembly} - include more of the path in the name to make it not ambiguous");

            using (var stream = containingAssembly.GetManifestResourceStream(resourceNames[0]))
                return ReadAllBytes(stream ?? throw new InvalidOperationException($"The resource {resourceFileName} was not found inside assembly {containingAssembly} or it failed to load"));
        }
    }
}
