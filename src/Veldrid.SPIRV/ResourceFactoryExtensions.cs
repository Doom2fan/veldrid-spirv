using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Veldrid.SPIRV
{
    /// <summary>
    /// Contains extension methods for loading <see cref="Shader"/> modules from SPIR-V bytecode.
    /// </summary>
    public static class ResourceFactoryExtensions
    {
        /// <summary>
        /// Creates a vertex and fragment shader pair from the given <see cref="ShaderDescription"/> pair containing SPIR-V
        /// bytecode or GLSL source code.
        /// </summary>
        /// <param name="factory">The <see cref="ResourceFactory"/> used to compile the translated shader code.</param>
        /// <param name="vertexShaderDescription">The vertex shader's description. <see cref="ShaderDescription.ShaderBytes"/>
        /// should contain SPIR-V bytecode or Vulkan-style GLSL source code which can be compiled to SPIR-V.</param>
        /// <param name="fragmentShaderDescription">The fragment shader's description.
        /// <see cref="ShaderDescription.ShaderBytes"/> should contain SPIR-V bytecode or Vulkan-style GLSL source code which
        /// can be compiled to SPIR-V.</param>
        /// <returns>A two-element array, containing the vertex shader (element 0) and the fragment shader (element 1).</returns>
        public static Shader[] CreateFromSpirv(
            this ResourceFactory factory,
            ShaderDescription vertexShaderDescription,
            ShaderDescription fragmentShaderDescription)
        {
            vertexShaderDescription.ShaderBytes = EnsureSpirv (vertexShaderDescription);
            fragmentShaderDescription.ShaderBytes = EnsureSpirv (fragmentShaderDescription);

            return new Shader []
            {
                factory.CreateShader(vertexShaderDescription),
                factory.CreateShader(fragmentShaderDescription)
            };
        }

        /// <summary>
        /// Creates a compute shader from the given <see cref="ShaderDescription"/> containing SPIR-V bytecode or GLSL source
        /// code.
        /// </summary>
        /// <param name="factory">The <see cref="ResourceFactory"/> used to compile the translated shader code.</param>
        /// <param name="computeShaderDescription">The compute shader's description.
        /// <see cref="ShaderDescription.ShaderBytes"/> should contain SPIR-V bytecode or Vulkan-style GLSL source code which
        /// can be compiled to SPIR-V.</param>
        /// <returns>The compiled compute <see cref="Shader"/>.</returns>
        public static Shader CreateFromSpirv(
            this ResourceFactory factory,
            ShaderDescription computeShaderDescription)
        {
            computeShaderDescription.ShaderBytes = EnsureSpirv (computeShaderDescription);
            return factory.CreateShader (computeShaderDescription);
        }

        private static unsafe byte[] EnsureSpirv(ShaderDescription description)
        {
            if (Util.HasSpirvHeader(description.ShaderBytes))
            {
                return description.ShaderBytes;
            }

            fixed (byte* sourceAsciiPtr = description.ShaderBytes)
            {
                SpirvCompilationResult glslCompileResult = SpirvCompilation.CompileGlslToSpirv(
                    (uint)description.ShaderBytes.Length,
                    sourceAsciiPtr,
                    null,
                    description.Stage,
                    description.Debug,
                    0,
                    null);
                return glslCompileResult.SpirvBytes;
            }
        }
    }
}
