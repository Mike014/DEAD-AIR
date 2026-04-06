namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Workaround (Polyfill) per abilitare l'uso della keyword 'init' (C# 9+) in Unity.
    /// Il compilatore C# cerca questo tipo specifico per gestire le proprietà init-only,
    /// ma non è presente nelle versioni di .NET Standard 2.1 o precedenti usate da Unity.
    /// </summary>
    internal static class IsExternalInit { }
}