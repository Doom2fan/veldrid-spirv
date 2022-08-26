using System;
using System.Text;

namespace Veldrid.SPIRV
{
    /// <summary>
    /// Static functions for compiling GLSL to SPIR-V.
    /// </summary>
    public static class SpirvCompilation
    {
        /// <summary>
        /// Compiles the given GLSL source code into SPIR-V.
        /// </summary>
        /// <param name="sourceText">The shader source code.</param>
        /// <param name="fileName">A descriptive name for the shader. May be null.</param>
        /// <param name="stage">The <see cref="ShaderStages"/> which the shader is used in.</param>
        /// <param name="options">Parameters for the GLSL compiler.</param>
        /// <returns>A <see cref="SpirvCompilationResult"/> containing the compiled SPIR-V bytecode.</returns>
        public static unsafe SpirvCompilationResult CompileGlslToSpirv(
            string sourceText,
            string fileName,
            ShaderStages stage,
            GlslCompileOptions options)
        {
            int sourceAsciiCount = Encoding.ASCII.GetByteCount(sourceText);
            byte* sourceAsciiPtr = stackalloc byte[sourceAsciiCount];
            fixed (char* sourceTextPtr = sourceText)
            {
                Encoding.ASCII.GetBytes(sourceTextPtr, sourceText.Length, sourceAsciiPtr, sourceAsciiCount);
            }

            int macroCount = options.Macros.Length;
            NativeMacroDefinition* macros = stackalloc NativeMacroDefinition[macroCount];
            for (int i = 0; i < macroCount; i++)
            {
                macros[i] = new NativeMacroDefinition(options.Macros[i]);
            }

            return CompileGlslToSpirv(
                (uint)sourceAsciiCount,
                sourceAsciiPtr,
                fileName,
                stage,
                options.Debug,
                (uint)macroCount,
                macros);
        }

        internal static unsafe SpirvCompilationResult CompileGlslToSpirv(
            uint sourceLength,
            byte* sourceTextPtr,
            string fileName,
            ShaderStages stage,
            bool debug,
            uint macroCount,
            NativeMacroDefinition* macros)
        {
            GlslCompileInfo info;
            info.Kind = GetShadercKind(stage);
            info.SourceText = new InteropArray(sourceLength, sourceTextPtr);
            info.Debug = debug;
            info.Macros = new InteropArray(macroCount, macros);

            if (string.IsNullOrEmpty(fileName)) { fileName = "<veldrid-spirv-input>"; }
            int fileNameAsciiCount = Encoding.ASCII.GetByteCount(fileName);
            byte* fileNameAsciiPtr = stackalloc byte[fileNameAsciiCount];
            if (fileNameAsciiCount > 0)
            {
                fixed (char* fileNameTextPtr = fileName)
                {
                    Encoding.ASCII.GetBytes(fileNameTextPtr, fileName.Length, fileNameAsciiPtr, fileNameAsciiCount);
                }
            }
            info.FileName = new InteropArray((uint)fileNameAsciiCount, fileNameAsciiPtr);

            CompilationResult* result = null;
            try
            {
                result = VeldridSpirvNative.CompileGlslToSpirv(&info);
                if (!result->Succeeded)
                {
                    throw new SpirvCompilationException(
                        "Compilation failed: " + Util.GetString((byte*)result->GetData(0), result->GetLength(0)));
                }

                uint length = result->GetLength(0);
                byte[] spirvBytes = new byte[(int)length];
                fixed (byte* spirvBytesPtr = &spirvBytes[0])
                {
                    Buffer.MemoryCopy(result->GetData(0), spirvBytesPtr, length, length);
                }

                return new SpirvCompilationResult(spirvBytes);
            }
            finally
            {
                if (result != null)
                {
                    VeldridSpirvNative.FreeResult(result);
                }
            }
        }

        private static ShadercShaderKind GetShadercKind(ShaderStages stage)
        {
            return stage switch
            {
                ShaderStages.Vertex => ShadercShaderKind.Vertex,
                ShaderStages.Geometry => ShadercShaderKind.Geometry,
                ShaderStages.TessellationControl => ShadercShaderKind.TessellationControl,
                ShaderStages.TessellationEvaluation => ShadercShaderKind.TessellationEvaluation,
                ShaderStages.Fragment => ShadercShaderKind.Fragment,
                ShaderStages.Compute => ShadercShaderKind.Compute,
                _ => throw new SpirvCompilationException($"Invalid shader stage: {stage}"),
            };
        }
    }
}
