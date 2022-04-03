namespace Loretta.RetargettingCompiler.Core.Converters
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    internal class ConverterAttribute : Attribute
    {
        public ConverterAttribute(LuaVersion sourceVersion, LuaVersion targetVersion)
        {
            SourceVersion = sourceVersion;
            TargetVersion = targetVersion;
        }

        public LuaVersion SourceVersion { get; }
        public LuaVersion TargetVersion { get; }
    }
}
