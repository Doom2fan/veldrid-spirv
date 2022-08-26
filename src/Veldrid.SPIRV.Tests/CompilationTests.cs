using Xunit;

namespace Veldrid.SPIRV.Tests
{
    public class CompilationTests
    {
        [Theory]
        [InlineData("instance.vert", ShaderStages.Vertex)]
        [InlineData("instance.frag", ShaderStages.Fragment)]
        [InlineData("planet.vert", ShaderStages.Vertex)]
        [InlineData("planet.frag", ShaderStages.Fragment)]
        [InlineData("starfield.vert", ShaderStages.Vertex)]
        [InlineData("starfield.frag", ShaderStages.Fragment)]
        [InlineData("texel-fetch-no-sampler.frag", ShaderStages.Fragment)]
        [InlineData("simple.comp", ShaderStages.Compute)]
        public void GlslToSpirv_Succeeds(string name, ShaderStages stage)
        {
            SpirvCompilationResult result = SpirvCompilation.CompileGlslToSpirv(
                TestUtil.LoadShaderText(name),
                name,
                stage,
                new GlslCompileOptions(
                    false,
                    new MacroDefinition("Name0", "Value0"),
                    new MacroDefinition("Name1", "Value1"),
                    new MacroDefinition("Name2")));

            Assert.NotNull(result.SpirvBytes);
            Assert.True(result.SpirvBytes.Length > 4);
            Assert.True(result.SpirvBytes.Length % 4 == 0);
        }
    }
}
